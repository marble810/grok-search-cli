# grok-search-cli 人工测试总览

## 测试范围

grok-search-cli 是一个使用 xAI API 搜索 Web 和 X(Twitter) 的命令行工具，通过 GitHub Releases 分发 Native AOT 编译的二进制文件。

| 模块 | 测试文档 | 预估时间 |
|------|----------|----------|
| 安装与认证 | [01-installation-and-auth.md](01-installation-and-auth.md) | 25 分钟 |
| 搜索功能 | [02-search-functionality.md](02-search-functionality.md) | 30 分钟 |
| 发现与契约 | [03-discovery-and-contract.md](03-discovery-and-contract.md) | 15 分钟 |

## 测试环境准备

### 必需条件

- 一个有效的 xAI API Key（从 [xAI 控制台](https://console.x.ai) 获取）
- 网络连接可以访问 `api.x.ai`
- 一个终端（macOS/Linux 用 Terminal/zsh/bash，Windows 用 PowerShell/CMD）

### 获取二进制文件

**方式 A：从源码构建（开发阶段）**

```bash
cd /path/to/grok-search-cli
dotnet build src/grok-search-cli/grok-search-cli.csproj --configuration Release
# 二进制位置: src/grok-search-cli/bin/Release/net10.0/grok-search-cli(.exe)
```

**方式 B：通过安装脚本（发布后）**

macOS/Linux:
```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.sh | bash
```

Windows (PowerShell):
```powershell
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.ps1) }"
```

### 快速冒烟测试

构建/安装完成后，运行以下命令确认基本可用：

```bash
# 1. 验证可执行
grok-search-cli --version 2>&1 || grok-search-cli help

# 2. 验证帮助
grok-search-cli help

# 3. 验证 describe
grok-search-cli describe | python3 -m json.tool
```

三项均成功输出即为环境就绪。

### 凭证准备

测试搜索功能需要配置 `XAI_API_KEY`。测试时推荐使用环境变量方式：

```bash
export XAI_API_KEY="xai-your-api-key-here"
```

验证凭证：
```bash
grok-search-cli auth status
# 应输出: status: configured via XAI_API_KEY environment variable
```

## 测试约定

- **预期行为**：每个测试用例包含明确的 **WHEN**（操作）和 **THEN**（预期结果）
- **通过标准**：实际输出与预期完全一致即为通过
- **失败处理**：记录实际输出与预期的差异，附上完整命令和输出内容
- **清理**：部分测试需要清理状态（如删除 credential store），每个用例末尾标注了清理步骤

## 架构速览

```
grok-search-cli
├── help [search|auth]          # 人类可读帮助
├── describe                     # 机器可读 JSON 描述
├── auth
│   ├── login [--api-key-stdin] # 保存 API Key
│   ├── status                  # 查看凭证状态
│   └── logout                  # 清除存储的凭证
└── --tool web|x|both [filters] <query>  # 执行搜索
```

**凭证优先级** (搜索时):
1. `XAI_API_KEY` 环境变量（最高优先）
2. `.env` 文件（从当前目录向上搜索）
3. Managed credential store (`~/.config/grok-search-cli/credentials.env`)

**搜索输出**：stdout 输出一个 JSON 对象，包含 `tool`, `model`, `answer`, `citations`, `id` 字段。所有诊断信息（错误、警告、日志）均输出到 stderr。
