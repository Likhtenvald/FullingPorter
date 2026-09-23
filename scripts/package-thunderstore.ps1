param(
    [string]$DllPath = "$PSScriptRoot\..\artifacts\FullingPorter.dll",
    [string]$OutputDir = "$PSScriptRoot\..\dist"
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$manifestPath = Join-Path $repoRoot "manifest.json"
$iconPath = Join-Path $repoRoot "packaging\icon.png"
$readmePath = Join-Path $repoRoot "packaging\README.md"
$changelogPath = Join-Path $repoRoot "packaging\CHANGELOG.md"

foreach ($path in @($manifestPath, $iconPath, $readmePath, $changelogPath, $DllPath)) {
    if (!(Test-Path $path -PathType Leaf)) { throw "Required package file not found: $path" }
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$version = [string]$manifest.version_number
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw "Invalid package version: $version" }
if ([string]$manifest.name -ne "FullingPorter") { throw "Unexpected package name: $($manifest.name)" }
if (!$manifest.dependencies -or $manifest.dependencies.Count -lt 3) { throw "Package dependencies are missing." }

$pluginSource = Get-Content (Join-Path $repoRoot "src\Plugin.cs") -Raw
$pluginVersion = [regex]::Match($pluginSource, 'PluginVersion\s*=\s*"([^"]+)"').Groups[1].Value
[xml]$project = Get-Content (Join-Path $repoRoot "FullingPorter.csproj") -Raw
$projectVersion = [string]$project.Project.PropertyGroup.Version
$dllVersion = [Reflection.AssemblyName]::GetAssemblyName((Resolve-Path $DllPath).Path).Version.ToString(3)
if ($version -ne $pluginVersion -or $version -ne $projectVersion -or $version -ne $dllVersion) {
    throw "Version mismatch: manifest=$version, plugin=$pluginVersion, project=$projectVersion, DLL=$dllVersion. Rebuild and increase the version before packaging changed code."
}

Add-Type -AssemblyName System.Drawing
$icon = [System.Drawing.Image]::FromFile((Resolve-Path $iconPath).Path)
try {
    if ($icon.Width -ne 256 -or $icon.Height -ne 256) { throw "icon.png must be 256x256 pixels." }
}
finally { $icon.Dispose() }

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$outputPath = Join-Path $OutputDir ("FullingPorter-" + $version + "-thunderstore.zip")
if (Test-Path $outputPath) { throw "Package already exists: $outputPath. Increase the version before creating a new upload." }

$stage = Join-Path ([IO.Path]::GetTempPath()) ("FullingPorter-package-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $stage | Out-Null
try {
    Copy-Item $manifestPath (Join-Path $stage "manifest.json")
    Copy-Item $iconPath (Join-Path $stage "icon.png")
    Copy-Item $readmePath (Join-Path $stage "README.md")
    Copy-Item $changelogPath (Join-Path $stage "CHANGELOG.md")
    Copy-Item $DllPath (Join-Path $stage "FullingPorter.dll")

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::CreateFromDirectory($stage, $outputPath)
    $archive = [IO.Compression.ZipFile]::OpenRead($outputPath)
    try {
        $actual = @($archive.Entries | ForEach-Object { $_.FullName } | Sort-Object)
        $expected = @("CHANGELOG.md", "FullingPorter.dll", "README.md", "icon.png", "manifest.json")
        if (($actual -join "|") -ne ($expected -join "|")) { throw "Unexpected ZIP contents: $($actual -join ', ')" }
    }
    finally { $archive.Dispose() }
}
catch {
    if (Test-Path $outputPath) { Remove-Item $outputPath -Force }
    throw
}
finally { Remove-Item $stage -Recurse -Force }

Write-Host "Thunderstore/r2modman package ready: $outputPath"
