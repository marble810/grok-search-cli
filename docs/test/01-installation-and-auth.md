# 01 - 安装与认证测试

## 1.1 安装脚本测试（仅限发布后）

> 以下测试在发布二进制可用后执行。开发阶段可跳过本节。

### 1.1.1 Bash 安装器：默认安装最新版

**WHEN**: 运行
```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.sh | bash
```

**THEN**:
- [ ] 输出包含 "Platform RID: " 后跟平台标识（如 `osx-arm64`）
- [ ] 输出包含 "Latest release: " 后跟版本标签
- [ ] 输出包含 "Downloading archive..." 和 "Downloading checksum..."
- [ ] 输出包含 "Checksum verified."
- [ ] 输出包含 "grok-search-cli <version> installed to: " 和安装路径
- [ ] 输出包含 "Next Steps: Credential Setup" 部分
- [ ] 输出包含 `grok-search-cli auth login` 命令指引
- [ ] 安装目录 `~/.grok-search-cli/bin/grok-search-cli` 文件存在且可执行

### 1.1.2 Bash 安装器：指定版本安装

**WHEN**: 运行
```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.sh | bash -s -- --version v1.0.0
```

**THEN**:
- [ ] 输出包含 "Requested version: v1.0.0"
- [ ] 下载的归档文件名为 `grok-search-cli_v1.0.0_<rid>.tar.gz`
- [ ] 其余流程与默认安装一致

### 1.1.3 Bash 安装器：自定义安装目录

**WHEN**: 运行
```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.sh | bash -s -- --dir /tmp/grok-test
```

**THEN**:
- [ ] 输出包含 "Install target: /tmp/grok-test"
- [ ] 二进制安装到 `/tmp/grok-test/grok-search-cli`
- [ ] PATH 指引显示 `/tmp/grok-test`

**清理**:
```bash
rm -rf /tmp/grok-test
```

### 1.1.4 Bash 安装器：重复安装（升级）

**前提**: 已安装过一次。

**WHEN**: 再次运行安装脚本

**THEN**:
- [ ] 安装目录中的二进制文件被覆盖为新版本
- [ ] 安装过程无报错
- [ ] 二元仍然可执行

### 1.1.5 PowerShell 安装器：默认安装

**WHEN**: 在 Windows PowerShell 中运行
```powershell
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.ps1) }"
```

**THEN**:
- [ ] 输出包含 "Latest release: " 和版本号
- [ ] 输出包含 "Platform RID: win-x64"
- [ ] 输出包含 "Checksum verified."
- [ ] 二进制安装到 `$env:LOCALAPPDATA\grok-search-cli\bin\grok-search-cli.exe`
- [ ] 文件存在
- [ ] 输出包含 "Next Steps: Credential Setup"

### 1.1.6 安装器：不收集凭证

**WHEN**: 运行任意安装脚本，不提供 API Key

**THEN**:
- [ ] 安装过程中没有任何提示要求输入 API Key
- [ ] 输出明确说明 "grok-search-cli does NOT collect API keys during installation"
- [ ] 输出指引用户安装后运行 `grok-search-cli auth login`

### 1.1.7 Bash 卸载器：默认目录卸载

**前提**: 已通过 Bash 安装器在默认目录安装。

**WHEN**: 运行
```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/uninstall.sh | bash
```

**THEN**:
- [ ] 输出包含 `Install target: ~/.grok-search-cli/bin` 对应的实际目录
- [ ] 输出包含已删除的 `grok-search-cli` 路径，或在重复执行时说明没有受管文件可删除
- [ ] 默认安装目录中的 `grok-search-cli` 二进制被移除
- [ ] 输出包含 PATH 后续清理提示或说明当前 PATH 未检测到该目录
- [ ] 输出明确说明不会修改 `XAI_API_KEY`、`.env` 或 auth-managed credential storage

### 1.1.8 Bash 卸载器：自定义目录与重复执行

**前提**: 已在 `/tmp/grok-test` 安装，且目录中额外放置了非 CLI 文件。

**WHEN**: 运行
```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/uninstall.sh | bash -s -- --dir /tmp/grok-test
```

**THEN**:
- [ ] 输出包含 `Install target: /tmp/grok-test`
- [ ] `grok-search-cli` 二进制被移除
- [ ] 非 CLI 文件仍保留，目录不会被递归删除
- [ ] 再次执行同一条卸载命令时退出成功
- [ ] 第二次执行输出说明没有受管文件可删除

**清理**:
```bash
rm -rf /tmp/grok-test
```

### 1.1.9 PowerShell 卸载器：默认目录与边界提示

**前提**: 已通过 PowerShell 安装器在默认目录安装。

**WHEN**: 在 Windows PowerShell 中运行
```powershell
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/uninstall.ps1) }"
```

**THEN**:
- [ ] 输出包含默认安装目录 `$env:LOCALAPPDATA\grok-search-cli\bin`
- [ ] `grok-search-cli.exe` 被移除
- [ ] 如果目录中仍有非 CLI 文件，则输出说明目录被保留
- [ ] 输出包含 User PATH 后续清理提示或说明当前未检测到 PATH 项
- [ ] 输出明确说明不会修改 `XAI_API_KEY`、`.env` 或 auth-managed credential storage

---

## 1.2 Auth Login 测试

### 1.2.1 交互式登录

**前提**: 清除现有凭证
```bash
grok-search-cli auth logout
unset XAI_API_KEY
```

**WHEN**: 运行
```bash
grok-search-cli auth login
```

**THEN**:
- [ ] 显示 "Enter your xAI API key: " 提示（无回显）
- [ ] 输入的字符不会显示在屏幕上
- [ ] 按 Backspace 可以删除已输入的字符
- [ ] 按 Enter 后显示 "API key saved to managed credential store."
- [ ] 运行 `grok-search-cli auth status` 显示 "status: configured via managed credential store"

### 1.2.2 空 API Key 拒绝

**WHEN**: 运行 `grok-search-cli auth login` 并直接按 Enter（不输入任何内容）

**THEN**:
- [ ] 输出 "error: API key cannot be empty"（在 stderr）
- [ ] 退出码非零
- [ ] `grok-search-cli auth status` 不受影响（保持之前的状态）

### 1.2.3 stdin 方式登录（带 --api-key-stdin）

**前提**: 清除现有凭证

**WHEN**: 运行
```bash
echo "xai-test-key-12345" | grok-search-cli auth login --api-key-stdin
```

**THEN**:
- [ ] 输出 "API key saved to managed credential store."
- [ ] 没有任何交互提示
- [ ] `grok-search-cli auth status` 显示 "status: configured via managed credential store"

### 1.2.4 stdin 为空时拒绝

**WHEN**: 运行
```bash
echo "" | grok-search-cli auth login --api-key-stdin
```

**THEN**:
- [ ] 输出 "error: API key cannot be empty"
- [ ] 退出码非零

### 1.2.5 无 --api-key-stdin 但有 stdin 重定向时拒绝

**WHEN**: 运行
```bash
echo "some-key" | grok-search-cli auth login
```

**THEN**:
- [ ] 输出包含 "stdin is redirected but --api-key-stdin was not provided"
- [ ] 退出码非零

### 1.2.6 --api-key-stdin 无 stdin 重定向时拒绝

**WHEN**: 在交互终端中运行
```bash
grok-search-cli auth login --api-key-stdin
```

**THEN**:
- [ ] 输出 "error: --api-key-stdin requires stdin to be redirected"
- [ ] 退出码非零

---

## 1.3 Auth Status 测试

### 1.3.1 环境变量优先级最高

**前提**:
```bash
export XAI_API_KEY="env-test-key"
```

**WHEN**: 运行 `grok-search-cli auth status`

**THEN**:
- [ ] 输出 "status: configured via XAI_API_KEY environment variable"
- [ ] 不会显示 key 的实际值

### 1.3.2 .env 文件优先级次之

**前提**:
```bash
unset XAI_API_KEY
echo 'XAI_API_KEY="dotenv-test-key"' > /tmp/test-grok/.env
cd /tmp/test-grok
grok-search-cli auth logout  # 清除 managed store
```

**WHEN**: 运行 `grok-search-cli auth status`

**THEN**:
- [ ] 输出包含 "status: configured via .env file"
- [ ] 输出包含 `.env` 文件的完整路径

**清理**:
```bash
rm -rf /tmp/test-grok
```

### 1.3.3 向上搜索 .env 文件

**前提**:
```bash
unset XAI_API_KEY
grok-search-cli auth logout
echo 'XAI_API_KEY="parent-env-key"' > /tmp/test-grok/.env
mkdir -p /tmp/test-grok/subdir
cd /tmp/test-grok/subdir
```

**WHEN**: 运行 `grok-search-cli auth status`

**THEN**:
- [ ] 输出包含 "status: configured via .env file"
- [ ] 路径指向 `/tmp/test-grok/.env`（父目录，非当前子目录）

**清理**:
```bash
rm -rf /tmp/test-grok
```

### 1.3.4 Managed store 优先级最低

**前提**:
```bash
unset XAI_API_KEY
cd $(mktemp -d)  # 一个没有 .env 的目录
grok-search-cli auth login  # 先设置 managed store
```

**WHEN**: 运行 `grok-search-cli auth status`

**THEN**:
- [ ] 输出 "status: configured via managed credential store"

### 1.3.5 未配置状态

**前提**:
```bash
unset XAI_API_KEY
cd $(mktemp -d)
grok-search-cli auth logout
```

**WHEN**: 运行 `grok-search-cli auth status`

**THEN**:
- [ ] 输出 "status: not configured. Run 'grok-search-cli auth login' to set up credentials."

---

## 1.4 Auth Logout 测试

### 1.4.1 清除 managed store

**前提**: 确保 managed store 中有凭证（先 `auth login`）

**WHEN**: 运行 `grok-search-cli auth logout`

**THEN**:
- [ ] 输出 "managed credential store cleared."
- [ ] `auth status` 不再显示 managed store

### 1.4.2 重复 logout 无副作用

**WHEN**: 连续运行两次 `grok-search-cli auth logout`

**THEN**:
- [ ] 第二次输出 "no managed credentials found. Nothing to remove."
- [ ] 退出码为零

### 1.4.3 Logout 后仍提示其他来源

**前提**:
```bash
export XAI_API_KEY="still-here-key"
grok-search-cli auth login  # 也写入 managed store
```

**WHEN**: 运行 `grok-search-cli auth logout`

**THEN**:
- [ ] 输出 "managed credential store cleared."
- [ ] 输出 "note: XAI_API_KEY is still set in the environment. Search will continue to use it."

**清理**:
```bash
unset XAI_API_KEY
```

### 1.4.4 从 .env 来源时的 logout 提示

**前提**:
```bash
unset XAI_API_KEY
echo 'XAI_API_KEY="from-dotenv"' > /tmp/test-grok/.env
cd /tmp/test-grok
grok-search-cli auth login  # 写入 managed store
```

**WHEN**: 运行 `grok-search-cli auth logout`

**THEN**:
- [ ] 输出包含 "note: XAI_API_KEY is still set in a .env file. Search will continue to use it."

**清理**:
```bash
rm -rf /tmp/test-grok
```

---

## 1.5 凭证存储安全测试

### 1.5.1 凭证文件权限（Unix）

**WHEN**: 检查 credential store 文件权限
```bash
ls -la ~/.config/grok-search-cli/credentials.env
```

**THEN**:
- [ ] 文件权限为 `-rw-------`（仅 owner 可读写）
- [ ] 文件内容格式为 `XAI_API_KEY=<key>`

### 1.5.2 Auth status 不泄露 key 值

**WHEN**: 在任何已配置状态下运行 `grok-search-cli auth status`

**THEN**:
- [ ] 输出不会以任何形式显示 API key 的实际值
- [ ] 只显示配置来源（环境变量 / .env 路径 / managed store）
