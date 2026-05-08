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

.PARAMETER AssetDir
    Optional local directory containing release-like archives and checksum files.
    When provided, the installer resolves assets from disk instead of GitHub Releases.
    Requires -Version.

.EXAMPLE
    .\install.ps1

.EXAMPLE
    .\install.ps1 -Version v1.0.0

.EXAMPLE
    .\install.ps1 -Version v1.0.0 -InstallDir D:\tools\grok-search-cli

.EXAMPLE
    .\install.ps1 -Version v1.0.0 -AssetDir .\artifacts -InstallDir D:\tools\grok-search-cli
#>

[CmdletBinding()]
param(
    [string]$Version,
    [string]$InstallDir,
    [string]$Repo = "marble810/grok-search-cli",
    [string]$AssetDir
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
    $combinedChecksumName = "checksums_${Version}.txt"
    return @{
        Archive          = "$base/$archiveName"
        Checksum         = "$base/$checksumName"
        CombinedChecksum = "$base/$combinedChecksumName"
    }
}

function Get-LocalAssetPaths {
    param([string]$AssetDir, [string]$Version, [string]$Rid)

    if (-not (Test-Path -LiteralPath $AssetDir -PathType Container)) {
        throw "Local asset directory not found: $AssetDir"
    }

    $resolvedDir = (Resolve-Path -LiteralPath $AssetDir).Path
    $archiveName = "grok-search-cli_${Version}_${Rid}.zip"
    $checksumName = "grok-search-cli_${Version}_${Rid}.sha256"
    $combinedChecksumName = "checksums_${Version}.txt"
    $archivePath = Join-Path $resolvedDir $archiveName
    $checksumPath = Join-Path $resolvedDir $checksumName
    $combinedChecksumPath = Join-Path $resolvedDir $combinedChecksumName

    if (-not (Test-Path -LiteralPath $archivePath -PathType Leaf)) {
        throw "Missing local asset '$archiveName' in $resolvedDir"
    }

    $hasDirectChecksum = Test-Path -LiteralPath $checksumPath -PathType Leaf
    $hasCombinedChecksum = Test-Path -LiteralPath $combinedChecksumPath -PathType Leaf
    if (-not $hasDirectChecksum -and -not $hasCombinedChecksum) {
        throw "Missing local asset '$checksumName' or '$combinedChecksumName' in $resolvedDir"
    }

    return @{
        AssetDir = $resolvedDir
        Archive = $archivePath
        Checksum = if ($hasDirectChecksum) { $checksumPath } else { $null }
        CombinedChecksum = if ($hasCombinedChecksum) { $combinedChecksumPath } else { $null }
    }
}

function Get-BinaryName { return "grok-search-cli.exe" }

function Invoke-DownloadWithRetry {
    param(
        [string]$Uri,
        [string]$OutFile,
        [string]$Description,
        [int]$MaxAttempts = 3
    )

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        try {
            if (Test-Path -LiteralPath $OutFile) {
                Remove-Item -LiteralPath $OutFile -Force
            }

            Invoke-WebRequest -Uri $Uri -OutFile $OutFile -UseBasicParsing
            return
        }
        catch {
            if ($attempt -eq $MaxAttempts) {
                throw "Failed to download $Description from $Uri after $MaxAttempts attempts: $_"
            }

            Write-Warning "Download attempt $attempt/$MaxAttempts failed for $Description from $Uri. Retrying..."
        }
    }
}

function Resolve-ExpectedHashFromChecksumFile {
    param(
        [string]$ChecksumPath,
        [string]$ArchiveName
    )

    $lines = Get-Content -LiteralPath $ChecksumPath | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    foreach ($line in $lines) {
        if ($line -match '^(?<hash>[A-Fa-f0-9]{64})\s+[* ]?(?<name>.+)$') {
            $name = Split-Path $Matches.name.Trim() -Leaf
            if ($name -eq $ArchiveName) {
                return $Matches.hash.ToLower()
            }
        }
    }

    throw "Checksum entry for '$ArchiveName' not found in $ChecksumPath"
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

Write-Host "=== grok-search-cli Installer ===" -ForegroundColor Cyan
Write-Host ""

# 1. Resolve version
if ($AssetDir -and -not $Version) {
    throw "Local asset installs require -Version so the expected archive name is deterministic."
}

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

# 4. Resolve asset source
$localAssets = $null
if ($AssetDir) {
    $localAssets = Get-LocalAssetPaths -AssetDir $AssetDir -Version $Version -Rid $Rid
    Write-Host "Asset source: local directory $($localAssets.AssetDir)" -ForegroundColor Green
}
else {
    $urls = Get-DownloadUrls -Repo $Repo -Version $Version -Rid $Rid
    $archiveUrl = $urls.Archive
    $checksumUrl = $urls.Checksum
    $combinedChecksumUrl = $urls.CombinedChecksum
    Write-Host "Asset source: GitHub Releases ($Repo)" -ForegroundColor Green
}

# 5. Download
$tmpDir = Get-TempDir
$archiveName = "grok-search-cli_${Version}_${Rid}.zip"
$archivePath = Join-Path $tmpDir $archiveName
$checksumPath = Join-Path $tmpDir "grok-search-cli_${Version}_${Rid}.sha256"
$checksumSource = $null

Write-Host ""
if ($localAssets) {
    Write-Host "Copying local archive..."
    Copy-Item -LiteralPath $localAssets.Archive -Destination $archivePath -Force

    if ($localAssets.Checksum) {
        Write-Host "Copying local checksum..."
        Copy-Item -LiteralPath $localAssets.Checksum -Destination $checksumPath -Force
        $checksumSource = "direct checksum"
    }
    else {
        Write-Host "Copying combined checksum manifest..."
        Copy-Item -LiteralPath $localAssets.CombinedChecksum -Destination $checksumPath -Force
        $checksumSource = "combined checksum manifest"
    }
}
else {
    Write-Host "Downloading archive..."
    Invoke-DownloadWithRetry -Uri $archiveUrl -OutFile $archivePath -Description "archive"

    Write-Host "Downloading checksum..."
    try {
        Invoke-DownloadWithRetry -Uri $checksumUrl -OutFile $checksumPath -Description "checksum"
        $checksumSource = "direct checksum"
    }
    catch {
        Write-Warning "Direct checksum download failed. Falling back to combined checksum manifest..."
        Invoke-DownloadWithRetry -Uri $combinedChecksumUrl -OutFile $checksumPath -Description "combined checksum manifest"
        $checksumSource = "combined checksum manifest"
    }
}

# 6. Verify checksum
Write-Host "Verifying checksum..."
$expectedHash = Resolve-ExpectedHashFromChecksumFile -ChecksumPath $checksumPath -ArchiveName $archiveName
$actualHash = (Get-FileHash $archivePath -Algorithm SHA256).Hash.ToLower()
if ($expectedHash -ne $actualHash) {
    throw "Checksum mismatch! Expected: $expectedHash, Actual: $actualHash"
}
Write-Host "Checksum verified using $checksumSource." -ForegroundColor Green

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
