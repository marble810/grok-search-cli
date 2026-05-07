# grok-search-cli

A command-line tool for searching the web and X (Twitter) via the xAI API. Returns structured JSON with an answer and source citations, designed for use by AI agents or shell pipelines.

## Installation

### Windows (PowerShell)

```powershell
iex "& { $(iwr -useb https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.ps1) }"
```

### Linux / macOS (Bash)

```bash
curl -fsSL https://raw.githubusercontent.com/marble810/grok-search-cli/main/install.sh | bash
```

After installation, configure your xAI API key:

```bash
grok-search-cli auth login
```

Or set the `XAI_API_KEY` environment variable directly.

See [INSTALL.md](INSTALL.md) for full installation options, checksum verification, and uninstall instructions.

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
