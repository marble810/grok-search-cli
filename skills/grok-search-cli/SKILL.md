---
name: grok-search-cli
description: Invoke the `grok-search-cli` binary to perform web search, X (Twitter) search, or combined search via the xAI API, returning structured JSON results with citations. Use this skill whenever you need to search the web or X for current information, news, links, or real-time content on behalf of the user. Triggers on requests like "search the web for...", "find recent tweets about...", "look up...", "what's the latest on...", or any task requiring live external search results.
---

# grok-search-cli

A CLI tool that queries the xAI API for web and/or X search results, returning structured JSON on stdout.

## Prerequisites

The binary must be installed and `XAI_API_KEY` must be available. To check:

```bash
grok-search-cli auth status
```

**If credentials are not configured**: Do NOT ask the user for their API key or attempt to configure it yourself. Instead, pause using the `AskUserQuestion` tool (or equivalent) and instruct the user to set up credentials manually:

> "Please configure your xAI API key first by running one of the following, then let me know when it's done:"
>
> ```bash
> # Interactive (recommended)
> grok-search-cli auth login
>
> # Or set an environment variable
> export XAI_API_KEY="xai-..."   # Linux/macOS
> $env:XAI_API_KEY = "xai-..."   # Windows PowerShell
> ```

Never read, print, forward, or store the user's API key.

## Discovery

Before composing commands, read the machine-readable contract:

```bash
grok-search-cli describe      # JSON: all commands, flags, rules, examples
grok-search-cli help search   # human-readable search flags
```

## Basic Usage

```bash
# Required: --tool web | x | both
grok-search-cli --tool web "latest AI news"
grok-search-cli --tool x "product launch" --allow-handle techcrunch
grok-search-cli --tool both "topic"

# stdin input (use one source only—arg OR stdin, never both)
printf "my query" | grok-search-cli --tool web
```

## Key Flags

| Flag | Description |
|---|---|
| `--tool web\|x\|both` | **Required.** Search source |
| `--model <name>` | Model override (default: `grok-4.3`) |
| `--allow-domain <d>` | Whitelist domain (web; repeatable) |
| `--exclude-domain <d>` | Blacklist domain (web; repeatable) |
| `--allow-handle <h>` | Whitelist X handle (x; repeatable) |
| `--exclude-handle <h>` | Blacklist X handle (x; repeatable) |
| `--from-date <YYYY-MM-DD>` | Results from date (x) |
| `--to-date <YYYY-MM-DD>` | Results to date (x) |
| `--enable-image-understanding` | Include image analysis |
| `--enable-video-understanding` | Include video analysis (x only) |

## Output Contract

Stdout: one JSON object. Stderr: diagnostics only.

```json
{
  "tool": "web",
  "model": "grok-4.3",
  "answer": "...",
  "citations": [{ "url": "https://...", "title": "..." }],
  "id": "..."
}
```

Exit codes: `0` = success, `1` = input/config error, `2` = API error.

## Credentials

Resolved in order: `XAI_API_KEY` env var → `.env` file in working directory → stored credential from `auth login`. The agent must never handle the key value directly—always defer to the user to configure credentials.

## Guidelines

- Always specify `--tool` explicitly.
- Parse `answer` for the natural-language result; use `citations` for source URLs.
- Prefer `--allow-domain` / `--allow-handle` to scope noisy queries.
- Use `--from-date` for recency-sensitive X searches.
