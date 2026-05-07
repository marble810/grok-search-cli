#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [string]$Version = "v0.0.0"
)

$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$sessionRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("grok-search-cli-local-session-" + [System.Guid]::NewGuid().ToString("n"))
$assetDir = Join-Path $sessionRoot "assets"
$installDir = Join-Path $sessionRoot "bin"
$packageScript = Join-Path $repoRoot "package-local-release.ps1"
$installScript = Join-Path $repoRoot "install.ps1"
$uninstallScript = Join-Path $repoRoot "uninstall.ps1"

Write-Host "Preparing local grok-search-cli test session..." -ForegroundColor Cyan
Write-Host "Session root: $sessionRoot" -ForegroundColor Green

& $packageScript -Version $Version -OutputDir $assetDir
& $installScript -Version $Version -AssetDir $assetDir -InstallDir $installDir

$pathEntries = $env:Path -split ';' | Where-Object { $_ }
if ($pathEntries -notcontains $installDir) {
    $env:Path = "$installDir;$env:Path"
}

$env:GROK_SEARCH_CLI_LOCAL_ROOT = $sessionRoot
$env:GROK_SEARCH_CLI_LOCAL_ASSET_DIR = $assetDir
$env:GROK_SEARCH_CLI_LOCAL_INSTALL_DIR = $installDir

Write-Host "" 
Write-Host "Local test session is ready." -ForegroundColor Cyan
Write-Host "grok-search-cli is now available in this PowerShell session from:" -ForegroundColor Green
Write-Host "  $installDir" -ForegroundColor White
Write-Host "" 
Write-Host "Current session helpers:" -ForegroundColor Yellow
Write-Host "  grok-search-cli help" -ForegroundColor White
Write-Host "  grok-search-cli auth login" -ForegroundColor White
Write-Host "  pwsh ./uninstall.ps1 -InstallDir `"$env:GROK_SEARCH_CLI_LOCAL_INSTALL_DIR`"" -ForegroundColor White
Write-Host "  Remove-Item -Recurse -Force `"$env:GROK_SEARCH_CLI_LOCAL_ROOT`"" -ForegroundColor White