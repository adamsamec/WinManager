; WinManager installer configuration

[CustomMessages]
MyAppName=WinManager
en.MyDescription=Utility for easier and more accessible application and windows switching and closing in Microsoft Windows
en.LaunchAfterInstall=Start WinManager after finishing installation
cs.MyDescription=Nástroj pro snadné a přístupnější přepínání a zavírání aplikací a oken v Microsoft Windows
cs.LaunchAfterInstall=Spustit WinManager po dokončení instalace
ExecutableFilename={cm:MyAppName}.exe

[Setup]
OutputBaseFilename=WinManager-1.0.5-win32-setup
AppVersion=1.0.5
AppName={cm:MyAppName}
AppId=WinManager
;PrivilegesRequired=lowest
DisableProgramGroupPage=yes
WizardStyle=modern
DefaultDirName={autopf}\{cm:MyAppName}
DefaultGroupName={cm:MyAppName}
VersionInfoDescription={cm:MyAppName} Setup
VersionInfoProductName={cm:MyAppName}
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
Name: "{group}\{cm:MyAppName}"; Filename: "{app}\{cm:ExecutableFilename}"
Name: "{commondesktop}\{cm:MyAppName}"; Filename: "{app}\{cm:ExecutableFilename}"

[Run]
Filename: {app}\{cm:MyAppName}.exe; Description: {cm:LaunchAfterInstall,{cm:MyAppName}}; Flags: nowait postinstall skipifsilent
