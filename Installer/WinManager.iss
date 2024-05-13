; WinManager installer configuration
#define MyAppName "WinManager"
#define MyAppVersion "1.0.6"
#define MyAppPublisher "Adam Samec"
#define MyAppExecutable "WinManager.exe"

[CustomMessages]
en.MyDescription=Utility for easier and more accessible application and windows switching and closing in Microsoft Windows
en.LaunchAfterInstall=Start WinManager after finishing installation
cs.MyDescription=Nástroj pro snadné a přístupnější přepínání a zavírání aplikací a oken v Microsoft Windows
cs.LaunchAfterInstall=Spustit WinManager po dokončení instalace

[Setup]
OutputBaseFilename=WinManager-{#MyAppVersion}-win32-setup
AppVersion={#MyAppVersion}
AppName={#MyAppName}
AppId={#MyAppName}
AppPublisher={#MyAppPublisher}
;PrivilegesRequired=lowest
DisableProgramGroupPage=yes
WizardStyle=modern
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}
; Uncomment the following line to disable the "Select Setup Language"
; dialog and have it rely solely on auto-detection.
;ShowLanguageDialog=no

[Languages]
Name: en; MessagesFile: "compiler:Default.isl"
Name: cs; MessagesFile: "compiler:Languages\Czech.isl"

[Messages]
en.BeveledLabel=English
cs.BeveledLabel=Čeština

[Files]
Source: "..\WinManager\bin\Release\net8.0-windows\win-x86\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExecutable}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExecutable}"

[Run]
Filename: {app}\{#MyAppExecutable}; Description: {cm:LaunchAfterInstall,{#MyAppName}}; Flags: nowait postinstall skipifsilent
