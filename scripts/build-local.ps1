param(
    [string]$ValheimDir = $env:VALHEIM_DIR,
    [string]$OutputDir = "$PSScriptRoot\..\artifacts"
)
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ValheimDir)) {
    $candidates = @(
        "$env:ProgramFiles(x86)\Steam\steamapps\common\Valheim",
        "$env:ProgramFiles\Steam\steamapps\common\Valheim"
    )
    $ValheimDir = $candidates | Where-Object { Test-Path "$_\valheim_Data\Managed\assembly_valheim.dll" } | Select-Object -First 1
}
if ([string]::IsNullOrWhiteSpace($ValheimDir) -or !(Test-Path "$ValheimDir\valheim_Data\Managed\assembly_valheim.dll")) {
    throw "Valheim installation not found. Pass -ValheimDir or set VALHEIM_DIR."
}
$managed = Join-Path $ValheimDir "valheim_Data\Managed"
$core = Join-Path $ValheimDir "BepInEx\core"
$jotunn = Get-ChildItem (Join-Path $ValheimDir "BepInEx\plugins") -Filter "Jotunn.dll" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
if (!$jotunn) { throw "Jotunn.dll not found under BepInEx\plugins." }
$env:VALHEIM_MANAGED = $managed
$env:BEPINEX_CORE = $core
$env:JOTUNN_DLL = $jotunn.FullName
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
dotnet build "$PSScriptRoot\..\FullingPorter.csproj" -c Release -p:OutputPath="$OutputDir\" 
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Build OK: $OutputDir\FullingPorter.dll"
