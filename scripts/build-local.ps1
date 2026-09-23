param(
    [string]$ValheimDir = $env:VALHEIM_DIR,
    [string]$ThunderstoreProfile = $env:THUNDERSTORE_PROFILE,
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

if ([string]::IsNullOrWhiteSpace($ThunderstoreProfile)) {
    $defaultProfile = Join-Path $env:APPDATA "Thunderstore Mod Manager\DataFolder\Valheim\profiles\Default"
    if (Test-Path $defaultProfile) { $ThunderstoreProfile = $defaultProfile }
}
if ([string]::IsNullOrWhiteSpace($ThunderstoreProfile) -or !(Test-Path $ThunderstoreProfile)) {
    throw "Thunderstore profile not found. Pass -ThunderstoreProfile or set THUNDERSTORE_PROFILE."
}

$managed = Join-Path $ValheimDir "valheim_Data\Managed"
$core = Join-Path $ThunderstoreProfile "BepInEx\core"
$plugins = Join-Path $ThunderstoreProfile "BepInEx\plugins"

if (!(Test-Path (Join-Path $core "BepInEx.dll"))) {
    throw "BepInEx.dll not found in Thunderstore profile: $core"
}
$jotunn = Get-ChildItem $plugins -Filter "Jotunn.dll" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
if (!$jotunn) {
    throw "Jotunn.dll not found in Thunderstore profile: $plugins"
}

$env:VALHEIM_MANAGED = $managed
$env:BEPINEX_CORE = $core
$env:JOTUNN_DLL = $jotunn.FullName

Write-Host "Valheim managed: $managed"
Write-Host "Thunderstore:    $ThunderstoreProfile"
Write-Host "BepInEx core:    $core"
Write-Host "Jotunn:          $($jotunn.FullName)"

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
dotnet build "$PSScriptRoot\..\FullingPorter.csproj" -c Release -p:OutputPath="$OutputDir\"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Build OK: $OutputDir\FullingPorter.dll"
