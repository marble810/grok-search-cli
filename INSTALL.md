# Installing grok-search-cli

grok-search-cli is distributed as precompiled binaries through **GitHub
Releases**. You can install it with one of the supported installer scripts;
no .NET SDK or repository clone is required.

## Quick Install

### Windows (PowerShell)

```powershell
# Latest release
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.ps1) }"

# Specific version
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.ps1) }" -Version v1.0.0
```

### Linux / macOS (Bash)

```bash
# Latest release
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.sh | bash

# Specific version
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.sh | bash -s -- --version v1.0.0
```

## Manual Install

1. Go to the [Releases page](https://github.com/marble810/grok-search-cli/releases).
2. Download the archive for your platform (`win-x64` for Windows,
   `linux-x64` for Linux, `osx-x64` or `osx-arm64` for macOS).
3. Download the matching `.sha256` file and verify the archive checksum.
4. Extract the binary and place it in a directory on your `PATH`.

## Verifying Checksums

Each release asset includes a `.sha256` checksum file. Verify before using:

```bash
# Unix
sha256sum -c grok-search-cli_<version>_<rid>.tar.gz.sha256

# Windows (PowerShell)
Get-FileHash grok-search-cli_<version>_<rid>.zip -Algorithm SHA256
```

## Setting Up Credentials

After installation, configure your xAI API key. grok-search-cli provides a
dedicated auth flow — the installers do **not** collect secrets.

### Interactive Setup

```bash
grok-search-cli auth login
```

You will be prompted to enter your API key securely.

### Non-interactive Setup

Pipe the key via stdin:

```bash
echo "<your-api-key>" | grok-search-cli auth login --api-key-stdin
```

### Alternative: Environment Variable

```bash
export XAI_API_KEY="<your-api-key>"
```

Or create a `.env` file in your working directory:

```
XAI_API_KEY=<your-api-key>
```

## Checking Your Setup

```bash
grok-search-cli auth status
```

## Upgrading

Re-run the installer with the desired version:

```bash
# Installs the latest version, replacing any existing install
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.sh | bash

# Install a specific version
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.sh | bash -s -- --version v1.1.0
```

The installer places the binary in the same user-scoped location, replacing
the previous version.

## Uninstalling

Use the supported uninstall scripts to remove the installer-managed binary from
the same user-scoped location used during installation.

### Windows (PowerShell)

```powershell
# Default install directory
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/uninstall.ps1) }"

# Custom install directory
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/uninstall.ps1) }" -InstallDir D:\tools\grok-search-cli
```

### Linux / macOS (Bash)

```bash
# Default install directory
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/uninstall.sh | bash

# Custom install directory
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/uninstall.sh | bash -s -- --dir /tmp/grok-test
```

The uninstallers remove only installer-managed CLI files. If the install
directory still contains unrelated files, it is left in place.

The uninstallers do **not**:
- remove credentials configured through `XAI_API_KEY`
- delete `.env` files
- delete auth-managed credential storage outside the install directory
- edit your PATH automatically

After uninstalling, remove any PATH entry you added manually if you no longer
want the install directory referenced by your shell or User PATH.
