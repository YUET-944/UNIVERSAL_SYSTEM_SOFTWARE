; Universal Business System Installer Script
; Generated for Inno Setup 6.x

#define MyAppName "Universal Business System"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Universal Software Solutions"
#define MyAppURL "https://www.universalsoftware.com"
#define MyAppExeName "UniversalBusinessSystem.exe"
#define MyAppAssocName "Universal Business System"
#define MyAppAssocExt ".ubs"
#define MyAppAssocKey "UniversalBusinessSystem"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\Universal Business System
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=LICENSE.txt
InfoBeforeFile=README.txt
OutputDir=Installer\Output
OutputBaseFilename=UniversalBusinessSystem-Setup-{#MyAppVersion}
SetupIconFile=Resources\app-icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=Modern
WizardImageFile=Resources\wizard-image.bmp
WizardSmallImageFile=Resources\wizard-small.bmp
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; OnlyBelowVersion: 6.1; Flags: unchecked

[Files]
Source: "UniversalBusinessSystem\bin\Release\net7.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Resources\*"; DestDir: "{app}\Resources"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "CHANGELOG.txt"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Dirs]
Name: "{app}\logs"
Name: "{app}\data"
Name: "{app}\backups"
Name: "{app}\temp"
Attribs: hidden

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"
Type: filesandordirs; Name: "{app}\data"
Type: filesandordirs; Name: "{app}\backups"
Type: filesandordirs; Name: "{app}\temp"

[Registry]
Root: HKCR; Subkey: ".ubs"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocKey}"; Flags: uninsdeletevalue
Root: HKCR; Subkey: ".ubs\OpenWithProgids"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocKey}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKCR; Subkey: "{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

[Code]
function GetUninstallString(): String;
var
  UninstallString: String;
begin
  UninstallString := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  Result := UninstallString;
end;

function IsUpgrade(): Boolean;
begin
  Result := RegKeyExists(HKEY_LOCAL_MACHINE, GetUninstallString());
end;

function GetInstalledVersion(): String;
var
  Version: String;
begin
  if RegQueryStringValue(HKEY_LOCAL_MACHINE, GetUninstallString(), 'DisplayVersion', Version) then
    Result := Version
  else
    Result := '';
end;

procedure InitializeSetup;
begin
  if IsUpgrade() then
  begin
    if MsgBox('An older version of {#MyAppName} is already installed. Do you want to upgrade?', 
              mbConfirmation, MB_YESNO) = IDNO then
    begin
      Abort();
    end;
  end;
  
  // Check .NET 7.0 Runtime
  if not RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v7\Full') then
  begin
    if MsgBox('.NET 7.0 Runtime is required to run {#MyAppName}. Would you like to download it now?', 
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/7.0', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
      Abort();
    end
    else
    begin
      MsgBox('.NET 7.0 Runtime is required. Please install it before continuing.', mbError, MB_OK);
      Abort();
    end;
  end;
  
  // Check Windows version
  if (GetWindowsVersion < $06030000) then // Windows 8.1
  begin
    MsgBox('{#MyAppName} requires Windows 8.1 or later.', mbError, MB_OK);
    Abort();
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
begin
  if (CurStep = ssPostInstall) then
  begin
    // Create initial configuration directory
    CreateDir(ExpandConstant('{userappdata}\UniversalBusinessSystem'));
    
    // Set permissions for data directory
    if FileExists(ExpandConstant('{app}\data')) then
    begin
      ShellExec('runas', 'icacls', '"' + ExpandConstant('{app}\data') + '" /grant Users:F', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if (CurUninstallStep = usPostUninstall) then
  begin
    // Ask user if they want to keep user data
    if MsgBox('Do you want to keep user data and configuration files?', 
              mbConfirmation, MB_YESNO) = IDNO then
    begin
      DeleteFile(ExpandConstant('{userappdata}\UniversalBusinessSystem\*.*'));
      RemoveDir(ExpandConstant('{userappdata}\UniversalBusinessSystem'));
    end;
  end;
end;

[Messages]
WelcomeLabel2=This will install [name] on your computer.%n%nIt is recommended that you close all other applications before continuing.
SelectDirDesc3=The application will be installed in the following folder.%n%nTo install in a different folder, click Browse and select another folder. Click Next to continue.
SelectDirBrowseLabel=To continue, click Next. If you want to select a different folder, click Browse.
ReadyLabel1=Setup is now ready to begin installing [name] on your computer.
ReadyLabel2a=Click Install to continue with the installation, or click Back if you want to review or change any settings.
ReadyLabel2b=Click Install to continue with the installation.

[CustomMessages]
english.LaunchProgram=Launch {#MyAppName}
french.LaunchProgram=Lancer {#MyAppName}
german.LaunchProgram={#MyAppName} starten
spanish.LaunchProgram=Iniciar {#MyAppName}
