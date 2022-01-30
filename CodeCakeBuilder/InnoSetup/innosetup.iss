; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "CK-LogViewer-Desktop"
#define MyAppPublisher "Kuinox (logviewer@kuinox.io)"
#define MyAppURL "https://github.com/Kuinox/CK-LogViewer/"
#define MyAppExeName "CK.LogViewer.Desktop.exe"
#define MyAppAssocName "Binary CK-Monitoring LogFile"
#define MyAppAssocExt ".ckmon"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt
#define AppServiceName "CK.LogViewer.WebApp"
#define ServicePort 8748
#define DotnetPath "C:\Program Files\dotnet\dotnet.exe"
#define ServiceDllPath "CK.LogViewer.WebApp\CK.LogViewer.WebApp.dll"
#define Appname "CK Desktop LogViewer"
[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{BE98E4C3-6B40-4B1F-9431-AE42CA974A88}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf64}\{#MyAppName}
ChangesAssociations=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename={#MyAppName}-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
DisableDirPage=auto

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl,CustomDefault.isl"

[Files]
Source: "..\Releases\CK.LogViewer.WebApp.Desktop\*"; DestDir: "{app}\CK.LogViewer.WebApp"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Releases\CK.LogViewer.Desktop\*"; DestDir: "{app}\CK.LogViewer.Desktop"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Releases\CK.LogViewer.Embedded\*"; DestDir: "{app}\CK.LogViewer.Embedded"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
Root: HKCR; Subkey: "{#MyAppAssocExt}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletevalue

Root: HKA; Subkey: "Software\Classes\{#MyAppAssocExt}\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".ckmon"; ValueData: ""

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

[_ISTool]
EnableISX=true

#include "innotools.iss"
#include "innofirewall.iss"
#include "innohooks.iss"
