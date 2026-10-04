#define MyAppName "Vehicle Weight Measurement System"
#define MyAppVersion "1.0"
#define MyAppPublisher "Farasoo Twzin Company"
#define MyAppExeName "VehicleWeightMeasurementSystemDemo.exe"

[Setup]
AppId={{A1B2C3D4-VEHICLE-WEIGHT}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}

DefaultDirName={pf}\VehicleWeightMeasurementSystem
DefaultGroupName={#MyAppName}

OutputDir=Output
OutputBaseFilename=VehicleWeightMeasurementSystemSetup

Compression=lzma
SolidCompression=yes

ArchitecturesInstallIn64BitMode=x64

; Silent support
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "VehicleWeightMeasurementSystemDemo\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch application"; Flags: nowait postinstall skipifsilent
