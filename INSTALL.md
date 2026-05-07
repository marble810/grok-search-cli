# Installing grok-search-cli

> [中文](#安装-grok-search-cli) | [English](#installing-grok-search-cli-1)

---

## 安装 grok-search-cli

grok-search-cli 通过 **GitHub Releases** 分发预编译二进制文件，使用安装脚本即可完成安装，无需 .NET SDK 或克隆仓库。

### 快速安装

#### Windows (PowerShell)

```powershell
# 最新版本
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.ps1) }"

# 指定版本
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.ps1) }" -Version v1.0.0
```

#### Linux / macOS (Bash)

```bash
# 最新版本
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.sh | bash

# 指定版本
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.sh | bash -s -- --version v1.0.0
```

### 手动安装

1. 前往 [Releases 页面](https://github.com/marble810/grok-search-cli/releases)。
2. 下载适合你平台的压缩包（Windows: `win-x64`，Linux: `linux-x64`，macOS: `osx-x64` 或 `osx-arm64`）。
3. 下载对应的 `.sha256` 文件并验证校验和。
4. 解压二进制文件，放入 `PATH` 中的目录。

### 校验和验证

每个 Release 资产都附带 `.sha256` 校验和文件，安装前请验证：

```bash
# Unix
sha256sum -c grok-search-cli_<version>_<rid>.tar.gz.sha256

# Windows (PowerShell)
Get-FileHash grok-search-cli_<version>_<rid>.zip -Algorithm SHA256
```

### 配置凭证

安装完成后，配置 xAI API Key。安装脚本**不会**收集任何密钥。

#### 交互式配置（推荐）

```bash
grok-search-cli auth login
```

按提示安全输入 API Key。

#### 非交互式配置

通过 stdin 传入：

```bash
echo "<your-api-key>" | grok-search-cli auth login --api-key-stdin
```

#### 环境变量

```bash
export XAI_API_KEY="<your-api-key>"
```

或在工作目录创建 `.env` 文件：

```
XAI_API_KEY=<your-api-key>
```

### 验证安装

```bash
grok-search-cli auth status
```

### 升级

重新运行安装脚本，指定目标版本：

```bash
# 安装最新版本（覆盖现有安装）
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.sh | bash

# 安装指定版本
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.sh | bash -s -- --version v1.1.0
```

### 卸载

使用卸载脚本移除安装脚本管理的二进制文件。

#### Windows (PowerShell)

```powershell
# 默认安装目录
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.ps1) }"

# 自定义安装目录
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.ps1) }" -InstallDir D:\tools\grok-search-cli
```

#### Linux / macOS (Bash)

```bash
# 默认安装目录
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.sh | bash

# 自定义安装目录
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.sh | bash -s -- --dir /tmp/grok-test
```

卸载脚本仅删除由安装脚本管理的 CLI 文件，不影响凭证数据。

---

## Installing grok-search-cli

grok-search-cli is distributed as precompiled binaries through **GitHub
Releases**. You can install it with one of the supported installer scripts;
no .NET SDK or repository clone is required.

### Quick Install

#### Windows (PowerShell)

```powershell
# Latest release
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.ps1) }"

# Specific version
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.ps1) }" -Version v1.0.0
```

#### Linux / macOS (Bash)

```bash
# Latest release
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.sh | bash

# Specific version
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.sh | bash -s -- --version v1.0.0
```

### Manual Install

1. Go to the [Releases page](https://github.com/marble810/grok-search-cli/releases).
2. Download the archive for your platform (`win-x64` for Windows,
   `linux-x64` for Linux, `osx-x64` or `osx-arm64` for macOS).
3. Download the matching `.sha256` file and verify the archive checksum.
4. Extract the binary and place it in a directory on your `PATH`.

### Verifying Checksums

Each release asset includes a `.sha256` checksum file. Verify before using:

```bash
# Unix
sha256sum -c grok-search-cli_<version>_<rid>.tar.gz.sha256

# Windows (PowerShell)
Get-FileHash grok-search-cli_<version>_<rid>.zip -Algorithm SHA256
```

### Setting Up Credentials

After installation, configure your xAI API key. grok-search-cli provides a
dedicated auth flow — the installers do **not** collect secrets.

#### Interactive Setup (recommended)

```bash
grok-search-cli auth login
```

You will be prompted to enter your API key securely.

#### Non-interactive Setup

Pipe the key via stdin:

```bash
echo "<your-api-key>" | grok-search-cli auth login --api-key-stdin
```

#### Environment Variable

```bash
export XAI_API_KEY="<your-api-key>"
```

Or create a `.env` file in your working directory:

```
XAI_API_KEY=<your-api-key>
```

### Checking Your Setup

```bash
grok-search-cli auth status
```

### Upgrading

Re-run the installer with the desired version:

```bash
# Installs the latest version, replacing any existing install
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.sh | bash

# Install a specific version
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.sh | bash -s -- --version v1.1.0
```

The installer places the binary in the same user-scoped location, replacing
the previous version.

### Uninstalling

Use the supported uninstall scripts to remove the installer-managed binary from
the same user-scoped location used during installation.

#### Windows (PowerShell)

```powershell
# Default install directory
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.ps1) }"

# Custom install directory
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.ps1) }" -InstallDir D:\tools\grok-search-cli
```

#### Linux / macOS (Bash)

```bash
# Default install directory
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.sh | bash

# Custom install directory
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.sh | bash -s -- --dir /tmp/grok-test
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
