#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Uninstall grok-search-cli from a user-scoped location without requiring
    administrator privileges.

.DESCRIPTION
    Removes the installer-managed grok-search-cli binary from the default or
    caller-specified install directory. The script removes only managed CLI
    files, prunes the install directory when it is empty, and reports any
    remaining manual PATH cleanup the user may need.

    Credentials configured through XAI_API_KEY, .env files, or auth-managed
    storage outside the install directory are not modified.

.PARAMETER InstallDir
    The directory where grok-search-cli was installed.
    Default: $env:LOCALAPPDATA\grok-search-cli\bin

.EXAMPLE
    .\uninstall.ps1

.EXAMPLE
    .\uninstall.ps1 -InstallDir D:\tools\grok-search-cli
#>

[CmdletBinding()]
param(
    [string]$InstallDir
)

$ErrorActionPreference = "Stop"

function Get-ManagedPaths {
    param([string]$TargetDir)

    return @(
        (Join-Path $TargetDir "grok-search-cli.exe")
    )
}

function Test-DirectoryEmpty {
    param([string]$TargetDir)

    if (-not (Test-Path $TargetDir -PathType Container)) {
        return $false
    }

    return (Get-ChildItem -Path $TargetDir -Force | Measure-Object).Count -eq 0
}

function Test-UserPathContains {
    param([string]$TargetDir)

    $userPath = [Environment]::GetEnvironmentVariable("Path", "User")
    if (-not $userPath) {
        return $false
    }

    $normalizedTarget = $TargetDir.TrimEnd('\\')
    foreach ($entry in $userPath.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        if ($entry.Trim().TrimEnd('\\') -ieq $normalizedTarget) {
            return $true
        }
    }

    return $false
}

Write-Host "=== grok-search-cli Uninstaller ===" -ForegroundColor Cyan
Write-Host ""

if (-not $InstallDir) {
    $InstallDir = Join-Path $env:LOCALAPPDATA "grok-search-cli" "bin"
}

Write-Host "Install target: $InstallDir" -ForegroundColor Green

$removedPaths = @()
$removedDirectory = $false

if (Test-Path $InstallDir -PathType Container) {
    foreach ($managedPath in Get-ManagedPaths -TargetDir $InstallDir) {
        if (Test-Path $managedPath -PathType Leaf) {
            Remove-Item -Path $managedPath -Force
            $removedPaths += $managedPath
        }
    }

    if (Test-DirectoryEmpty -TargetDir $InstallDir) {
        Remove-Item -Path $InstallDir -Force
        $removedDirectory = $true
    }
}

Write-Host ""
if ($removedPaths.Count -gt 0) {
    Write-Host "Removed managed grok-search-cli files:" -ForegroundColor Cyan
    foreach ($removedPath in $removedPaths) {
        Write-Host "  $removedPath" -ForegroundColor White
    }
}
else {
    Write-Host "No managed grok-search-cli files were present to remove." -ForegroundColor Yellow
}

if ($removedDirectory) {
    Write-Host "Removed empty install directory: $InstallDir" -ForegroundColor Green
}
elseif (Test-Path $InstallDir -PathType Container) {
    Write-Host "Install directory left in place because it still contains non-managed files." -ForegroundColor Yellow
}

Write-Host ""
if (Test-UserPathContains -TargetDir $InstallDir) {
    Write-Host "NOTE: The install directory may still be present in your User PATH. Remove '$InstallDir' manually if you no longer want that entry." -ForegroundColor Yellow
}
else {
    Write-Host "No current User PATH entry was detected for the install directory." -ForegroundColor Green
}

Write-Host "Credential configuration via XAI_API_KEY, .env files, or auth-managed storage was not modified." -ForegroundColor Yellow
Write-Host ""
Write-Host "Uninstall complete!" -ForegroundColor Cyan