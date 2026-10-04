#define MyAppName "RmtoSync"
#define MyAppVersion "1.0"
#define MyAppPublisher "Farasoo Twzin Company"
#define MyAppExeName "RmtoSync.exe"

[Setup]
AppId={{A1B2C3D4-VEHICLE-RmtoSync}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}

DefaultDirName={pf}\RmtoSyncVehicleWeightMeasurementSystem
DefaultGroupName={#MyAppName}

OutputDir=Output
OutputBaseFilename=RmtoSync

Compression=lzma
SolidCompression=yes

ArchitecturesInstallIn64BitMode=x64

; Silent support
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "RmtoSync\bin\Release\net10.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch application"; Flags: nowait postinstall skipifsilent
