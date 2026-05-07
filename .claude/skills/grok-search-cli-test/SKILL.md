---
name: grok-search-cli-test
description: Invoke the local xAI search CLI (`grok-search-cli`) for web, X, or combined search queries. Supports `--tool web|x|both`, stdin/arg query input, domain/handle/date filters, and returns JSON results on stdout with citations. Use when Claude needs to validate CLI behavior or exercise the xAI search contract from this repository.
---

# grok-search-cli Test

Build: `dotnet build src/grok-search-cli`
Run: `dotnet run --project src/grok-search-cli -- <args>`
AOT binary: `src/grok-search-cli/bin/Release/net10.0/publish/grok-search-cli`

## Contract Discovery

For canonical command usage, flags, examples, and output contracts, inspect the installed CLI directly:

- **Human-readable help**: `grok-search-cli help` (root), `grok-search-cli help search`, `grok-search-cli help auth`
- **Machine-readable discovery**: `grok-search-cli describe` (JSON document with all command groups, flags, rules, examples, and credential prerequisites)

Use these discovery entrypoints before composing commands. The sections below only document repo-specific build and execution details that the CLI binary cannot know.

## Credentials

Set `XAI_API_KEY` env var, or create a `.env` file at the repo root with `XAI_API_KEY=...`, or run `grok-search-cli auth login`.

## Output

Stdout: one JSON document with `{tool, model, answer, citations[], id?}`.
Stderr: diagnostics only (errors, warnings). Exit code 0 on success, 1 on input/config error, 2 on API error.

## Examples

```bash
grok-search-cli --tool web "latest AI news"
grok-search-cli --tool x "product launch" --allow-handle techcrunch --from-date 2026-01-01
grok-search-cli --tool both --exclude-domain spam.com "topic"
printf "query" | grok-search-cli --tool web
```

## Implementation Style

- Always choose `--tool web|x|both` explicitly.
- Provide the query via one source only: positional argument or stdin.
- Expect JSON-only stdout and keep diagnostics on stderr.
- Prefer let-it-crash behavior at public contract boundaries.
- Keep control flow concise and avoid blanket retries or broad recovery logic unless the contract requires them.
- Keep follow-on changes Native AOT-friendly and avoid dynamic configuration systems.
