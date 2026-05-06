---
name: xai-search-cli-test
description: Invoke the local xAI search CLI (`xai-search`) for web, X, or combined search queries. Supports `--tool web|x|both`, stdin/arg query input, domain/handle/date filters, and returns JSON results on stdout with citations. Use when Claude needs to validate CLI behavior or exercise the xAI search contract from this repository.
---

# xAI Search CLI Test

Build: `dotnet build src/xai-search-cli`
Run: `dotnet run --project src/xai-search-cli -- <args>`
AOT binary: `src/xai-search-cli/bin/Release/net10.0/publish/xai-search.exe`

## Flags

| Flag | Required | Description |
|------|----------|-------------|
| `--tool web\|x\|both` | Yes | Search scope: web, X/Twitter, or both combined |
| query | Yes* | Positional argument or stdin pipe (mutually exclusive) |
| `--allow-domain <d>` | No | Web filter: include only this domain (repeatable) |
| `--exclude-domain <d>` | No | Web filter: exclude this domain (repeatable) |
| `--allow-handle <h>` | No | X filter: include only this handle (repeatable) |
| `--exclude-handle <h>` | No | X filter: exclude this handle (repeatable) |
| `--from-date <yyyy-mm-dd>` | No | X filter: include results on or after this date |
| `--to-date <yyyy-mm-dd>` | No | X filter: include results on or before this date |

## Credentials

Set `XAI_API_KEY` env var, or create a `.env` file at the repo root with `XAI_API_KEY=...`.

## Output

Stdout: one JSON document with `{tool, model, answer, citations[], id?}`.
Stderr: diagnostics only (errors, warnings). Exit code 0 on success, 1 on input/config error, 2 on API error.

## Examples

```bash
xai-search --tool web "latest AI news"
xai-search --tool x "product launch" --allow-handle techcrunch --from-date 2026-01-01
xai-search --tool both --exclude-domain spam.com "topic"
printf "query" | xai-search --tool web
```

## Implementation Style

- Always choose `--tool web|x|both` explicitly.
- Provide the query via one source only: positional argument or stdin.
- Expect JSON-only stdout and keep diagnostics on stderr.
- Prefer let-it-crash behavior at public contract boundaries.
- Keep control flow concise and avoid blanket retries or broad recovery logic unless the contract requires them.
- Keep follow-on changes Native AOT-friendly and avoid dynamic configuration systems.
