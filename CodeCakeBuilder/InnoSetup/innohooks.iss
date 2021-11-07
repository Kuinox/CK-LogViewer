

[Code]

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  TargetService: string;
begin
  TargetService := ExpandConstant('{#AppServiceName}');
  if IsServiceInstalled(TargetService) = true then  
    if IsServiceRunning(TargetService) = true then
      begin
        WizardForm.StatusLabel.Caption := CustomMessage('StoppingService');
        StopService(TargetService);
        while IsServiceWaiting(TargetService) do
          Sleep(200);
      end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  TargetService: string;
  TargetServicePort: string;
begin
  TargetService := ExpandConstant('{#AppServiceName}');
  TargetServicePort := ExpandConstant('{#ServicePort}');
  // Installing
  if CurStep = ssInstall then  
    begin
    	if IsServiceInstalled(TargetService) = true then
        begin
          WizardForm.StatusLabel.Caption := CustomMessage('UninstallingService');
          RemoveService(TargetService);
        end
    end
  else if CurStep = ssPostInstall then
    begin
      // Install Service
      WizardForm.StatusLabel.Caption := CustomMessage('InstallingService');
      InstallService(
        ExpandConstant('"{#DotnetPath}" "{app}\{#ServiceDllPath}"'), // FileName
        TargetService, // ServiceName
        ExpandConstant('{#AppName}'), // Display Name
        ExpandConstant('Hôte {#AppName}'), // Description
        SERVICE_WIN32_OWN_PROCESS,
        SERVICE_AUTO_START
      );

      // Setup Firewall
      WizardForm.StatusLabel.Caption := CustomMessage('ConfiguringFirewall');
      AddServiceFirewallRule(TargetService, TargetService, TargetServicePort);

      // Start service
      WizardForm.StatusLabel.Caption := CustomMessage('StartingService');
      StartService(TargetService);
      while IsServiceWaiting(TargetService) do
        Sleep(200);
    end
  else if CurStep = ssDone then
    begin
    end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  PreviousCaption: string;
  TargetService: string;
begin
  TargetService := ExpandConstant('{#AppServiceName}');
  if CurUninstallStep = usUninstall then
    begin
    	if IsServiceInstalled(TargetService) = true then
        begin
          PreviousCaption := UninstallProgressForm.StatusLabel.Caption;
          
          // Stop service  
          if IsServiceRunning(TargetService) = true then
            begin
              UninstallProgressForm.StatusLabel.Caption := CustomMessage('StoppingService');
              StopService(TargetService);
              while IsServiceWaiting(TargetService) do
                Sleep(200);    
            end;

          // Uninstall service
          UninstallProgressForm.StatusLabel.Caption := CustomMessage('UninstallingService');
          RemoveService(TargetService);

          // Remove firewall rules
          UninstallProgressForm.StatusLabel.Caption := CustomMessage('ConfiguringFirewall');
          RemoveFirewallRule(TargetService);

          UninstallProgressForm.StatusLabel.Caption := PreviousCaption;
        end
    end; 
end;

