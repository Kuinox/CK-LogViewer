[Code]
procedure AddServiceFirewallRule(RuleName: string; ServiceName: string; Port: string);
var
  ResultCode: Integer;
begin
  Exec(
    ExpandConstant('{cmd}'),
    '/C netsh advfirewall firewall add rule name='+RuleName+' service='+ServiceName+' dir=in action=allow protocol=tcp remoteip=127.0.0.1 localport='+Port,
    ExpandConstant('{sys}'),
    SW_HIDE,
    ewWaitUntilTerminated,
    ResultCode
    );
end;

procedure RemoveFirewallRule(RuleName: string);
var
  ResultCode: Integer;
begin
  Exec(
    ExpandConstant('{cmd}'),
    '/C netsh advfirewall firewall delete rule name='+RuleName,
    ExpandConstant('{sys}'),
    SW_HIDE,
    ewWaitUntilTerminated,
    ResultCode
    );
end;