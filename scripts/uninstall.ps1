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

function Remove-GrokFromUserPath {
    param([string]$InstallDir)

    $userPath = [Environment]::GetEnvironmentVariable("Path", "User")
    if (-not $userPath) {
        Write-Host "No grok-search-cli PATH entry was present to remove." -ForegroundColor Green
        return
    }

    $normalizedDir = $InstallDir.TrimEnd('\')
    $entries = $userPath.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)
    $filtered = $entries | Where-Object { $_.Trim().TrimEnd('\') -ne $normalizedDir }

    if ($filtered.Count -eq $entries.Count) {
        Write-Host "No grok-search-cli PATH entry was present to remove." -ForegroundColor Green
        return
    }

    $newPath = if ($filtered.Count -gt 0) { $filtered -join ';' } else { $null }
    [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
    Write-Host "Removed grok-search-cli from User PATH." -ForegroundColor Cyan

    # Check if the directory still appears in the process-level PATH (residual)
    $currentPath = [Environment]::GetEnvironmentVariable("Path", "Process")
    if ($currentPath -and $currentPath.Contains($InstallDir)) {
        Write-Host "NOTE: The install directory may still be present in your current session PATH." -ForegroundColor Yellow
        Write-Host "      Start a new terminal session or update your PATH manually." -ForegroundColor Yellow
    }
}

function Get-CredentialStorePath {
    $configHome = if ($env:XDG_CONFIG_HOME) { $env:XDG_CONFIG_HOME } else { Join-Path $env:USERPROFILE ".config" }
    return Join-Path $configHome "grok-search-cli" "credentials.env"
}

function Invoke-CredentialCleanup {
    $credFiles = @()

    # Check managed credential store
    $credStore = Get-CredentialStorePath
    if (Test-Path -LiteralPath $credStore -PathType Leaf) {
        $credFiles += @{ Type = "managed"; Path = $credStore }
    }

    # Check $env:USERPROFILE\.env for XAI_API_KEY
    $homeEnv = Join-Path $env:USERPROFILE ".env"
    if (Test-Path -LiteralPath $homeEnv -PathType Leaf) {
        $content = Get-Content -LiteralPath $homeEnv -Raw -ErrorAction SilentlyContinue
        if ($content -and $content.Contains("XAI_API_KEY")) {
            $credFiles += @{ Type = "home"; Path = $homeEnv }
        }
    }

    # Check current directory .env for XAI_API_KEY
    $cwdEnv = Join-Path (Get-Location) ".env"
    if (Test-Path -LiteralPath $cwdEnv -PathType Leaf) {
        $content = Get-Content -LiteralPath $cwdEnv -Raw -ErrorAction SilentlyContinue
        if ($content -and $content.Contains("XAI_API_KEY")) {
            $credFiles += @{ Type = "cwd"; Path = $cwdEnv }
        }
    }

    if ($credFiles.Count -eq 0) {
        Write-Host "No API key files (.env or credential store) were found." -ForegroundColor Green
        return
    }

    Write-Host ""
    Write-Host "=== Credential Cleanup ===" -ForegroundColor Cyan

    foreach ($entry in $credFiles) {
        Write-Host ""
        switch ($entry.Type) {
            "managed" {
                Write-Host "Managed credential store detected: $($entry.Path)" -ForegroundColor Yellow
                $response = Read-Host "Clear managed credentials (equivalent to 'grok-search-cli auth logout')? [y/N]"
                if ($response -match '^[Yy]') {
                    Remove-Item -LiteralPath $entry.Path -Force
                    Write-Host "Managed credential store cleared." -ForegroundColor Green
                } else {
                    Write-Host "Skipped. Clear manually with: grok-search-cli auth logout" -ForegroundColor Yellow
                }
            }
            "home" {
                Write-Host "API key file detected: $($entry.Path)" -ForegroundColor Yellow
                $response = Read-Host "Delete this file? [y/N]"
                if ($response -match '^[Yy]') {
                    Remove-Item -LiteralPath $entry.Path -Force
                    Write-Host "Deleted: $($entry.Path)" -ForegroundColor Green
                } else {
                    Write-Host "Skipped." -ForegroundColor Yellow
                }
            }
            "cwd" {
                Write-Host "API key file detected: $($entry.Path)" -ForegroundColor Yellow
                $response = Read-Host "Delete this file? [y/N]"
                if ($response -match '^[Yy]') {
                    Remove-Item -LiteralPath $entry.Path -Force
                    Write-Host "Deleted: $($entry.Path)" -ForegroundColor Green
                } else {
                    Write-Host "Skipped." -ForegroundColor Yellow
                }
            }
        }
    }

    Write-Host ""
    Write-Host "Note: XAI_API_KEY environment variable (if set) is managed outside this script." -ForegroundColor Yellow
    Write-Host "      To unset it, remove it from your system or user environment variables." -ForegroundColor Yellow
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
Remove-GrokFromUserPath -InstallDir $InstallDir

Invoke-CredentialCleanup
Write-Host ""
Write-Host "Uninstall complete!" -ForegroundColor Cyan