param(
    [string]$ValheimDir = $env:VALHEIM_DIR,
    [string]$ThunderstoreProfile = $env:THUNDERSTORE_PROFILE,
    [switch]$InspectTrader,
    [switch]$NoInstall,
    [switch]$ShowLog
)
$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

function Find-Valheim {
    param([string]$Explicit)
    if ($Explicit -and (Test-Path (Join-Path $Explicit "valheim_Data\Managed\assembly_valheim.dll"))) { return $Explicit }
    $roots = @()
    $pf86 = [Environment]::GetFolderPath("ProgramFilesX86")
    $pf = [Environment]::GetFolderPath("ProgramFiles")
    if ($pf86) { $roots += (Join-Path $pf86 "Steam\steamapps\common\Valheim") }
    if ($pf) { $roots += (Join-Path $pf "Steam\steamapps\common\Valheim") }
    foreach ($steamRoot in @($pf86, $pf)) {
        if (!$steamRoot) { continue }
        $vdf = Join-Path $steamRoot "Steam\steamapps\libraryfolders.vdf"
        if (!(Test-Path $vdf)) { continue }
        foreach ($line in Get-Content $vdf -ErrorAction SilentlyContinue) {
            if ($line -match '"path"\s+"([^"]+)"') {
                $library = $matches[1] -replace '\\\\','\'
                $roots += (Join-Path $library "steamapps\common\Valheim")
            }
        }
    }
    return $roots | Where-Object { Test-Path (Join-Path $_ "valheim_Data\Managed\assembly_valheim.dll") } | Select-Object -First 1
}

function Find-ThunderstoreProfile {
    param([string]$Explicit)
    if ($Explicit -and (Test-Path (Join-Path $Explicit "BepInEx\plugins"))) { return $Explicit }
    $profilesRoot = Join-Path $env:APPDATA "Thunderstore Mod Manager\DataFolder\Valheim\profiles"
    $default = Join-Path $profilesRoot "Default"
    if (Test-Path (Join-Path $default "BepInEx\plugins")) { return $default }
    if (Test-Path $profilesRoot) {
        return Get-ChildItem $profilesRoot -Directory -ErrorAction SilentlyContinue |
            Where-Object { Test-Path (Join-Path $_.FullName "BepInEx\plugins") } |
            Select-Object -First 1 -ExpandProperty FullName
    }
    return $null
}

$ValheimDir = Find-Valheim $ValheimDir
if (!$ValheimDir) { throw "Valheim not found. Run with -ValheimDir <path> once, or set VALHEIM_DIR." }
$ThunderstoreProfile = Find-ThunderstoreProfile $ThunderstoreProfile
if (!$ThunderstoreProfile) { throw "Thunderstore Valheim profile not found. Run with -ThunderstoreProfile <path> once, or set THUNDERSTORE_PROFILE." }

Write-Host "=== FullingPorter dev helper ==="
Write-Host "Repo:         $repoRoot"
Write-Host "Valheim:      $ValheimDir"
Write-Host "Thunderstore: $ThunderstoreProfile"
Write-Host ""

if ($InspectTrader) {
    & (Join-Path $PSScriptRoot "inspect-trader.ps1") -ValheimDir $ValheimDir
    exit $LASTEXITCODE
}

& (Join-Path $PSScriptRoot "build-local.ps1") -ValheimDir $ValheimDir -ThunderstoreProfile $ThunderstoreProfile
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$dll = Join-Path $repoRoot "artifacts\FullingPorter.dll"
if (!(Test-Path $dll)) { throw "Build reported success but FullingPorter.dll was not found: $dll" }

if (!$NoInstall) {
    $destination = Join-Path $ThunderstoreProfile "BepInEx\plugins\FullingPorter"
    New-Item -ItemType Directory -Force -Path $destination | Out-Null
    $installedDll = Join-Path $destination "FullingPorter.dll"
    Copy-Item $dll $installedDll -Force
    Write-Host ""
    Write-Host "Installed: $installedDll"
}

$log = Join-Path $ThunderstoreProfile "BepInEx\LogOutput.log"
if ($ShowLog -and (Test-Path $log)) {
    Write-Host ""
    Write-Host "=== Recent FullingPorter / error log lines ==="
    Get-Content $log -Tail 500 |
        Select-String -Pattern "FullingPorter|NullReferenceException|Exception|Error|Harmony" |
        Select-Object -Last 100 |
        ForEach-Object { $_.Line }
}

Write-Host ""
Write-Host "Ready. Launch Valheim Modded from Thunderstore for runtime testing."
Write-Host "Useful commands:"
Write-Host "  .\scripts\dev.ps1 -InspectTrader"
Write-Host "  .\scripts\dev.ps1 -ShowLog"
Write-Host "  .\scripts\dev.ps1 -NoInstall"
