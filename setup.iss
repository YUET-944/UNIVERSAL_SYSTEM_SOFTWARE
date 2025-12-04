[Setup]
AppName=Universal Business System
AppVersion=1.0.0
AppPublisher=Your Company
DefaultDirName={pf}\Universal Business System
DefaultGroupName=Universal Business System
UninstallDisplayIcon={app}\UniversalBusinessSystem.exe
Compression=lzma2
SolidCompression=yes
OutputDir=Output
OutputBaseFilename=UniversalBusinessSystem_Setup

[Files]
Source: "PublishedApp\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Universal Business System"; Filename: "{app}\UniversalBusinessSystem.exe"
Name: "{commondesktop}\Universal Business System"; Filename: "{app}\UniversalBusinessSystem.exe"

[Run]
Filename: "{app}\UniversalBusinessSystem.exe"; Description: "Launch Universal Business System"; Flags: postinstall nowait skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
