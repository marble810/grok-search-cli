#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Install or upgrade grok-search-cli from GitHub Release assets into a
    user-scoped location.  No credentials or secrets are collected during
    installation.

.DESCRIPTION
    Downloads a specified version (or the latest stable release) of the
    grok-search-cli binary for the current Windows platform, verifies its
    SHA-256 checksum, and places it under $LocalAppData\grok-search-cli\bin.

    After installation, set up credentials by running:
        grok-search-cli auth login

.PARAMETER Version
    The semantic version to install, including the leading "v" (e.g., "v1.0.0").
    If omitted, the latest stable release is fetched from GitHub.

.PARAMETER InstallDir
    The directory where grok-search-cli should be installed.
    Default: $env:LOCALAPPDATA\grok-search-cli\bin

.PARAMETER Repo
    GitHub repository in "owner/repo" format.
    Default: marble810/grok-search-cli

.EXAMPLE
    .\install.ps1

.EXAMPLE
    .\install.ps1 -Version v1.0.0

.EXAMPLE
    .\install.ps1 -Version v1.0.0 -InstallDir D:\tools\grok-search-cli
#>

[CmdletBinding()]
param(
    [string]$Version,
    [string]$InstallDir,
    [string]$Repo = "marble810/grok-search-cli"
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Helper functions
# ---------------------------------------------------------------------------

function Get-TempDir {
    $tmp = Join-Path ([System.IO.Path]::GetTempPath()) "grok-search-cli-install"
    if (-not (Test-Path $tmp)) { New-Item -ItemType Directory -Path $tmp -Force | Out-Null }
    return $tmp
}

function Get-PlatformRid {
    $arch = $env:PROCESSOR_ARCHITECTURE
    if ($arch -eq "AMD64") { return "win-x64" }
    if ($arch -eq "ARM64") { return "win-arm64" }
    throw "Unsupported architecture: $arch. grok-search-cli supports AMD64 systems."
}

function Get-LatestVersion {
    param([string]$Repo)
    $url = "https://api.github.com/repos/$Repo/releases/latest"
    Write-Host "Fetching latest release info from $url ..."
    try {
        $release = Invoke-RestMethod -Uri $url -Headers @{ Accept = "application/vnd.github.v3+json" }
        return $release.tag_name
    }
    catch {
        throw "Failed to resolve latest release: $_"
    }
}

function Get-DownloadUrls {
    param([string]$Repo, [string]$Version, [string]$Rid)
    $base = "https://github.com/$Repo/releases/download/$Version"
    $archiveName = "grok-search-cli_${Version}_${Rid}.zip"
    $checksumName = "grok-search-cli_${Version}_${Rid}.sha256"
    return @{
        Archive  = "$base/$archiveName"
        Checksum = "$base/$checksumName"
    }
}

function Get-BinaryName { return "grok-search-cli.exe" }

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

Write-Host "=== grok-search-cli Installer ===" -ForegroundColor Cyan
Write-Host ""

# 1. Resolve version
if (-not $Version) {
    $Version = Get-LatestVersion -Repo $Repo
    Write-Host "Latest release: $Version" -ForegroundColor Green
}
else {
    Write-Host "Requested version: $Version" -ForegroundColor Green
}

# 2. Resolve install directory
if (-not $InstallDir) {
    $InstallDir = Join-Path $env:LOCALAPPDATA "grok-search-cli" "bin"
}
Write-Host "Install target: $InstallDir" -ForegroundColor Green

# 3. Detect platform
$Rid = Get-PlatformRid
Write-Host "Platform RID: $Rid" -ForegroundColor Green

# 4. Build download URLs
$urls = Get-DownloadUrls -Repo $Repo -Version $Version -Rid $Rid
$archiveUrl = $urls.Archive
$checksumUrl = $urls.Checksum

# 5. Download
$tmpDir = Get-TempDir
$archivePath = Join-Path $tmpDir "grok-search-cli_${Version}_${Rid}.zip"
$checksumPath = Join-Path $tmpDir "grok-search-cli_${Version}_${Rid}.sha256"

Write-Host ""
Write-Host "Downloading archive..."
try {
    Invoke-WebRequest -Uri $archiveUrl -OutFile $archivePath -UseBasicParsing
}
catch {
    throw "Failed to download archive from $archiveUrl : $_"
}

Write-Host "Downloading checksum..."
try {
    Invoke-WebRequest -Uri $checksumUrl -OutFile $checksumPath -UseBasicParsing
}
catch {
    throw "Failed to download checksum from $checksumUrl : $_"
}

# 6. Verify checksum
Write-Host "Verifying checksum..."
$expectedHash = (Get-Content $checksumPath -Raw).Trim().Split(" ")[0]
$actualHash = (Get-FileHash $archivePath -Algorithm SHA256).Hash.ToLower()
if ($expectedHash -ne $actualHash) {
    throw "Checksum mismatch! Expected: $expectedHash, Actual: $actualHash"
}
Write-Host "Checksum verified." -ForegroundColor Green

# 7. Extract binary
Write-Host "Extracting..."
if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
}
$binaryName = Get-BinaryName
try {
    Expand-Archive -Path $archivePath -DestinationPath $tmpDir -Force
    # The archive contains a single binary; resolve it regardless of subfolder
    $extracted = Get-ChildItem -Path $tmpDir -Filter $binaryName -Recurse -File
    if ($extracted.Count -eq 0) { throw "Binary '$binaryName' not found in archive." }
    Copy-Item -Path $extracted[0].FullName -Destination (Join-Path $InstallDir $binaryName) -Force
}
catch {
    throw "Failed to extract binary: $_"
}

# 8. Report and PATH guidance
Write-Host ""
Write-Host "grok-search-cli $Version installed to: $InstallDir" -ForegroundColor Cyan

$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($userPath -and $userPath.Contains($InstallDir)) {
    Write-Host "Install directory is already in your PATH." -ForegroundColor Green
}
else {
    Write-Host ""
    Write-Host "NOTE: Add the install directory to your PATH to use 'grok-search-cli' from any terminal:" -ForegroundColor Yellow
    Write-Host "  [Environment]::SetEnvironmentVariable(""Path"", ""`$env:Path;$InstallDir"", ""User"")" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Or add it manually via: System Properties -> Environment Variables -> User PATH" -ForegroundColor Yellow
}

# 9. Credential setup handoff (no secrets collected during install)
Write-Host ""
Write-Host "=== Next Steps: Credential Setup ===" -ForegroundColor Cyan
Write-Host "grok-search-cli does NOT collect API keys during installation." -ForegroundColor Yellow
Write-Host "To set up your xAI API key, run the following command after installation:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  grok-search-cli auth login" -ForegroundColor White
Write-Host ""
Write-Host "You can also configure credentials via the XAI_API_KEY environment variable" -ForegroundColor Yellow
Write-Host "or a .env file in your working directory." -ForegroundColor Yellow
Write-Host ""
Write-Host "Installation complete!" -ForegroundColor Cyan
