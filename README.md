# grok-search-cli

> [中文](#简介) | [English](#introduction)

---

## 简介

通过 xAI API 在命令行中搜索网页和 X（Twitter）的工具。以结构化 JSON 返回答案和来源引用，适合 AI Agent 或 Shell 管道调用。

## 安装

### Windows (PowerShell)

```powershell
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.ps1) }"
```

### Linux / macOS (Bash)

```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.sh | bash
```

安装完成后，配置 xAI API Key：

```bash
grok-search-cli auth login
```

或直接设置环境变量 `XAI_API_KEY`。

完整安装选项及校验和说明见 [INSTALL.md](INSTALL.md)。

## 卸载

使用卸载脚本移除已安装的 CLI 文件（不影响凭证数据）：

### Windows (PowerShell)

```powershell
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.ps1) }"
```

### Linux / macOS (Bash)

```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.sh | bash
```

如果使用了自定义安装目录，传入对应参数：

```powershell
# Windows
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.ps1) }" -InstallDir D:\tools\grok-search-cli

# Linux / macOS
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.sh | bash -s -- --dir /path/to/install
```

## 使用

```bash
# 网页搜索
grok-search-cli --tool web "最新 AI 新闻"

# X（Twitter）搜索
grok-search-cli --tool x "产品发布" --allow-handle techcrunch --from-date 2026-01-01

# 网页 + X 联合搜索
grok-search-cli --tool both "主题"

# 通过 stdin 传入查询
printf "我的查询" | grok-search-cli --tool web

# 过滤域名
grok-search-cli --tool web --allow-domain reuters.com --exclude-domain spam.com "主题"
```

**输出** — stdout 输出一个 JSON 对象：

```json
{
  "tool": "web",
  "model": "grok-4.3",
  "answer": "...",
  "citations": [{ "url": "https://...", "title": "..." }]
}
```

stderr 仅用于诊断信息。退出码：`0` 成功 · `1` 输入/配置错误 · `2` API 错误。

### 发现机制

```bash
grok-search-cli help              # 根帮助
grok-search-cli help search       # 搜索全部参数
grok-search-cli describe          # 机器可读 JSON 契约
```

### 主要参数

| 参数 | 说明 |
|---|---|
| `--tool web\|x\|both` | **必填。** 搜索来源 |
| `--model <name>` | 指定模型（默认：`grok-4.3`） |
| `--allow-domain <d>` | 网页搜索白名单域名（可重复） |
| `--exclude-domain <d>` | 网页搜索黑名单域名（可重复） |
| `--allow-handle <h>` | X 账号白名单（可重复） |
| `--exclude-handle <h>` | X 账号黑名单（可重复） |
| `--from-date <YYYY-MM-DD>` | X 搜索最早日期 |
| `--to-date <YYYY-MM-DD>` | X 搜索最晚日期 |
| `--enable-image-understanding` | 启用图片分析 |
| `--enable-video-understanding` | 启用视频分析（仅限 X） |

## 开发

### 前置条件

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### 构建

```bash
dotnet build src/grok-search-cli
```

### 源码运行

```bash
dotnet run --project src/grok-search-cli -- --tool web "测试查询"
```

### 测试

```bash
dotnet test tests/grok-search-cli.Tests
```

### AOT 发布

```bash
dotnet publish src/grok-search-cli -c Release
# 二进制：src/grok-search-cli/bin/Release/net10.0/publish/grok-search-cli
```

发布和打 Tag 流程见 [RELEASE.md](RELEASE.md)。

## AI Agent 集成

本项目内置 [Agent Skill](skills/grok-search-cli/SKILL.md)，可教会 AI Agent 如何调用本工具。用以下命令安装到你的编程助手：

```bash
npx skills add marble810/grok-search-cli
```

支持 Claude Code、Cursor、GitHub Copilot、Codex 及 [50+ 其他 Agent](https://github.com/vercel-labs/skills#supported-agents)。

## 许可证

MIT

---

## Introduction

A command-line tool for searching the web and X (Twitter) via the xAI API. Returns structured JSON with an answer and source citations, designed for use by AI agents or shell pipelines.

## Installation

### Windows (PowerShell)

```powershell
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.ps1) }"
```

### Linux / macOS (Bash)

```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/install.sh | bash
```

After installation, configure your xAI API key:

```bash
grok-search-cli auth login
```

Or set the `XAI_API_KEY` environment variable directly.

See [INSTALL.md](INSTALL.md) for full installation options and checksum verification.

## Uninstall

Use the supported uninstall scripts to remove the installer-managed CLI files (credentials are not affected):

### Windows (PowerShell)

```powershell
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.ps1) }"
```

### Linux / macOS (Bash)

```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.sh | bash
```

If you used a custom install directory, pass it through:

```powershell
# Windows
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.ps1) }" -InstallDir D:\tools\grok-search-cli

# Linux / macOS
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/scripts/uninstall.sh | bash -s -- --dir /path/to/install
```

## Usage

```bash
# Web search
grok-search-cli --tool web "latest AI news"

# X (Twitter) search
grok-search-cli --tool x "product launch" --allow-handle techcrunch --from-date 2026-01-01

# Combined web + X search
grok-search-cli --tool both "topic"

# Query via stdin
printf "my query" | grok-search-cli --tool web

# Filter by domain
grok-search-cli --tool web --allow-domain reuters.com --exclude-domain spam.com "topic"
```

**Output** — one JSON object on stdout:

```json
{
  "tool": "web",
  "model": "grok-4.3",
  "answer": "...",
  "citations": [{ "url": "https://...", "title": "..." }]
}
```

Stderr carries diagnostics only. Exit codes: `0` success · `1` input/config error · `2` API error.

### Discovery

```bash
grok-search-cli help              # root help
grok-search-cli help search       # all search flags
grok-search-cli describe          # machine-readable JSON contract
```

### Key Flags

| Flag | Description |
|---|---|
| `--tool web\|x\|both` | **Required.** Search source |
| `--model <name>` | Model override (default: `grok-4.3`) |
| `--allow-domain <d>` | Whitelist domain for web search (repeatable) |
| `--exclude-domain <d>` | Blacklist domain for web search (repeatable) |
| `--allow-handle <h>` | Whitelist X handle (repeatable) |
| `--exclude-handle <h>` | Blacklist X handle (repeatable) |
| `--from-date <YYYY-MM-DD>` | Earliest date for X results |
| `--to-date <YYYY-MM-DD>` | Latest date for X results |
| `--enable-image-understanding` | Include image analysis |
| `--enable-video-understanding` | Include video analysis (X only) |

## Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Build

```bash
dotnet build src/grok-search-cli
```

### Run from source

```bash
dotnet run --project src/grok-search-cli -- --tool web "test query"
```

### Tests

```bash
dotnet test tests/grok-search-cli.Tests
```

### AOT publish

```bash
dotnet publish src/grok-search-cli -c Release
# Binary: src/grok-search-cli/bin/Release/net10.0/publish/grok-search-cli
```

See [RELEASE.md](RELEASE.md) for the release and tagging process.

## AI Agent Integration

This project ships a built-in [Agent Skill](skills/grok-search-cli/SKILL.md) that teaches AI agents how to invoke the CLI. Install it into your coding agent with:

```bash
npx skills add marble810/grok-search-cli
```

Supports Claude Code, Cursor, GitHub Copilot, Codex, and [50+ other agents](https://github.com/vercel-labs/skills#supported-agents).

## License

MIT
