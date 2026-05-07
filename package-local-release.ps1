#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    [string]$RuntimeId,
    [string]$OutputDir
)

$ErrorActionPreference = "Stop"

function Get-DefaultRuntimeId {
    $arch = $env:PROCESSOR_ARCHITECTURE
    if ($arch -eq "AMD64") { return "win-x64" }
    if ($arch -eq "ARM64") { return "win-arm64" }
    throw "Unsupported architecture: $arch"
}

if (-not $RuntimeId) {
    $RuntimeId = Get-DefaultRuntimeId
}

if (-not $OutputDir) {
    $OutputDir = Join-Path $PSScriptRoot "artifacts" "local-release"
}

$projectPath = Join-Path $PSScriptRoot "src" "grok-search-cli" "grok-search-cli.csproj"
$publishDir = Join-Path ([System.IO.Path]::GetTempPath()) ("grok-search-cli-local-package-" + [System.Guid]::NewGuid().ToString("n"))
$binaryName = if ($RuntimeId.StartsWith("win")) { "grok-search-cli.exe" } else { "grok-search-cli" }
$archiveName = if ($RuntimeId.StartsWith("win")) { "grok-search-cli_${Version}_${RuntimeId}.zip" } else { "grok-search-cli_${Version}_${RuntimeId}.tar.gz" }
$checksumName = "grok-search-cli_${Version}_${RuntimeId}.sha256"
$combinedChecksumName = "checksums_${Version}.txt"

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

try {
    Write-Host "Publishing grok-search-cli for $RuntimeId ..."
    & dotnet publish $projectPath --configuration Release --runtime $RuntimeId --self-contained -p:PublishAot=true -p:StripSymbols=true -p:DebugType=None -o $publishDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $RuntimeId"
    }

    $archivePath = Join-Path $OutputDir $archiveName
    $checksumPath = Join-Path $OutputDir $checksumName
    $combinedChecksumPath = Join-Path $OutputDir $combinedChecksumName
    $binaryPath = Join-Path $publishDir $binaryName

    if (-not (Test-Path -LiteralPath $binaryPath -PathType Leaf)) {
        throw "Published binary not found: $binaryPath"
    }

    if ($RuntimeId.StartsWith("win")) {
        if (Test-Path -LiteralPath $archivePath) {
            Remove-Item -LiteralPath $archivePath -Force
        }

        Compress-Archive -Path $binaryPath -DestinationPath $archivePath
    }
    else {
        & tar -czf $archivePath -C $publishDir $binaryName
        if ($LASTEXITCODE -ne 0) {
            throw "tar packaging failed for $archivePath"
        }
    }

    $hash = (Get-FileHash $archivePath -Algorithm SHA256).Hash.ToLower()
    "$hash  $archiveName" | Out-File -FilePath $checksumPath -Encoding ASCII
    Copy-Item -LiteralPath $checksumPath -Destination $combinedChecksumPath -Force

    Write-Host "Created archive: $archivePath"
    Write-Host "Created checksum: $checksumPath"
    Write-Host "Updated combined checksums: $combinedChecksumPath"
}
finally {
    if (Test-Path -LiteralPath $publishDir) {
        Remove-Item -LiteralPath $publishDir -Recurse -Force
    }
}