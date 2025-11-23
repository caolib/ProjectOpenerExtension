; Inno Setup Script for Project Opener Extension
; This script creates an EXE installer for the extension

#define AppVersion "0.0.1.0"
#define AppName "Project Opener Extension"
#define AppPublisher "caolib"
#define AppURL "https://github.com/caolib/ProjectOpenerExtension"
#define ExeName "ProjectOpenerExtension.exe"
; IMPORTANT: Replace this CLSID with your extension's CLSID from ProjectOpenerExtension.cs
#define CLSID "bfe7c130-2e5c-49db-86b4-1c178ea8527b"

[Setup]
AppId={{bfe7c130-2e5c-49db-86b4-1c178ea8527b}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}/issues
AppUpdatesURL={#AppURL}/releases
DefaultDirName={autopf}\ProjectOpenerExtension
DisableProgramGroupPage=yes
OutputDir=ProjectOpenerExtension\bin\Release\installer
OutputBaseFilename=ProjectOpenerExtension-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
MinVersion=10.0.19041
PrivilegesRequired=lowest
; SetupIconFile requires .ico format, commenting out for now
; SetupIconFile=ProjectOpenerExtension\Assets\StoreLogo.png

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Files]
Source: "ProjectOpenerExtension\bin\Release\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Registry]
; Register COM server
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{{{#CLSID}}}"; ValueType: string; ValueName: ""; ValueData: "ProjectOpenerExtension"; Flags: uninsdeletekey
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{{{#CLSID}}}\LocalServer32"; ValueType: string; ValueName: ""; ValueData: """{app}\{#ExeName}"" -RegisterProcessAsComServer"; Flags: uninsdeletevalue

[Run]
; Register the extension after installation
Filename: "{app}\{#ExeName}"; Parameters: "-RegisterProcessAsComServer"; Flags: runhidden

[UninstallRun]
; Unregister before uninstall
Filename: "{app}\{#ExeName}"; Parameters: "-UnregisterProcessAsComServer"; Flags: runhidden

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    Log('Extension installed successfully');
  end;
end;
