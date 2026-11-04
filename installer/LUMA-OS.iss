#define MyAppName "LUMA OS Preview"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "LUMA"
#ifndef PublishDir
  #define PublishDir "..\publish\win-x64"
#endif

[Setup]
AppId={{7D8D23D8-1E42-4E42-AC3B-2C6B41C6B8D4}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://github.com/borzenkoofficial-lab/LUMA-OS
DefaultDirName={localappdata}\Programs\LUMA
DefaultGroupName=LUMA
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
MinVersion=10.0.17763
OutputDir=..\artifacts
OutputBaseFilename=LUMA-OS-Setup
UninstallDisplayName=LUMA OS Preview
UninstallDisplayIcon={app}\Luma.App.exe
WizardStyle=modern
Compression=lzma2/ultra64
SolidCompression=yes
WizardSizePercent=100
CloseApplications=no
RestartApplications=no
Uninstallable=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "Создать ярлык на рабочем столе"; GroupDescription: "Дополнительные ярлыки:"; Flags: unchecked
Name: "startup"; Description: "Запускать LUMA автоматически при входе в Windows"; GroupDescription: "Автозапуск:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\LUMA"; Filename: "{app}\Luma.App.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\LUMA"; Filename: "{app}\Luma.App.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "LUMA"; ValueData: """{app}\Luma.App.exe"""; Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\Luma.App.exe"; Description: "Запустить LUMA OS Preview"; Flags: nowait postinstall skipifsilent
