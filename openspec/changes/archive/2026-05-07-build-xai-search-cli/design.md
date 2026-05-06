## Context

The repository is starting from an almost empty state, so this change establishes both the CLI contract and the implementation posture. The CLI is meant for AI agents rather than interactive terminal users, which makes determinism more important than presentation.

xAI exposes both `web_search` and `x_search` through the Responses API, and the requested model is `grok-4-1-fast-reasoning`. The implementation must remain friendly to .NET Native AOT, avoid unnecessary abstraction or defensive layers, and keep the runtime contract simple enough for agents to call repeatedly.

## Goals / Non-Goals

**Goals:**
- Ship one agent-first CLI contract for xAI search with explicit `web`, `x`, and `both` modes.
- Keep stdout machine-readable and free of incidental logs.
- Support `XAI_API_KEY` and `.env` as the only initial credential inputs, with `.env` ignored by git.
- Keep the implementation Native AOT-friendly, concise, and performant.
- Add a repository-local Claude skill that helps agents exercise the CLI contract consistently.

**Non-Goals:**
- Designing a human-first, colorized, or rich terminal UI.
- Adding raw passthrough for arbitrary xAI request JSON in the first version.
- Hiding upstream failures behind retries, fallback providers, or broad recovery logic.
- Supporting legacy Chat Completions search flows.
- Building a general-purpose config system with layered files, profiles, or secret stores.

## Decisions

### Use one search command with explicit tool selection
The CLI will expose one search entry point and require `--tool web|x|both`.

Rationale:
- Agents behave more predictably with explicit control than with auto-routing.
- One command keeps the public contract small while still covering both search surfaces.
- `both` will enable `web_search` and `x_search` in the same Responses API request so the model produces one answer and one citations set.

Alternatives considered:
- Auto-selecting the tool from the query was rejected because it makes tests and agent behavior less deterministic.
- Separate commands such as `search-web` and `search-x` were rejected because they duplicate output and validation logic.

### Prefer direct HTTP + System.Text.Json over an SDK wrapper
The implementation should call the xAI Responses API with `HttpClient` and use `System.Text.Json`, ideally with source generation for DTOs that are on the hot path.

Rationale:
- Native AOT compatibility is easier to control with the BCL than with a larger SDK surface.
- It keeps dependencies small and startup time predictable.
- The contract only needs a narrow part of the Responses API.

Alternatives considered:
- Using an xAI SDK was rejected for the first version because AOT behavior, transitive dependencies, and serialization shape are harder to control.

### Keep query input explicit and single-sourced
The command will accept the query from either a positional argument or stdin, but never both.

Rationale:
- Agents often pipe prompts, but short queries are simpler as arguments.
- Rejecting ambiguous input is more predictable than guessing precedence.

Alternatives considered:
- Position-only input was rejected because it is awkward for long prompts.
- Stdin-only input was rejected because it adds friction for simple invocations.

### Standardize a small JSON output contract
Successful runs will emit a single JSON document on stdout that contains at least the final answer, citations, and basic metadata such as selected tool and model. Diagnostics and error text go to stderr only.

Rationale:
- Agents need stable machine-readable output.
- Separating stdout and stderr avoids accidental parser breakage.
- A single JSON document is simpler than streaming for a first release.

Alternatives considered:
- Pretty terminal output by default was rejected because it is hostile to agent callers.
- Default streaming output was rejected because it complicates parsers and testing.

### Constrain the first release to stable, explicit filters
The CLI will expose only the common parameters already agreed during design: domain allow/exclude for web search, handle allow/exclude and date range for X search, plus image and video understanding flags where supported.

Rationale:
- The tool remains easy to document and test.
- Agents can use stable flags without coupling to full xAI payload shapes.

Alternatives considered:
- Raw JSON passthrough was rejected because it leaks upstream schema churn into the CLI contract.

### Favor let-it-crash at implementation boundaries
The implementation should validate only the external contract boundaries that matter to users and agents, then fail fast for misconfiguration, impossible states, or upstream contract drift. It should avoid blanket try/catch blocks, speculative fallbacks, and defensive object plumbing.

Rationale:
- This matches the requested coding style.
- Simpler code is easier to audit and usually performs better in a small AOT CLI.
- Fast failure makes contract regressions obvious during agent use.

Alternatives considered:
- Defensive wrappers around every parse and branch were rejected because they add code, hide failures, and blur the contract.

### Add a repository-local testing skill
The repository will include `.claude/skills/xai-search-cli-test/SKILL.md` as a focused testing and contract-validation aid for agents.

Rationale:
- The skill gives agents a repeatable way to exercise the CLI once implemented.
- It also preserves the style constraints for follow-on implementation and tests.

Alternatives considered:
- Keeping this guidance only in OpenSpec docs was rejected because agent tooling discovers local skills more directly than design documents.

## Risks / Trade-offs

- [AOT serialization gaps] -> Use narrow DTOs and source-generated JSON metadata for stable request and response types.
- [Upstream schema drift in Responses API] -> Keep the CLI contract thinner than the upstream payload and fail fast when mandatory fields disappear.
- [Both-mode latency or cost] -> Document that `both` is the most capable mode but not the cheapest; let callers choose explicitly.
- [Too little defensive code for operator comfort] -> Reserve validation for public contract boundaries and use clear stderr diagnostics with non-zero exit codes.
- [Skill drift from implementation] -> Keep the skill aligned with the OpenSpec artifacts and update it in the same changeset as contract changes.

## Migration Plan

This is a greenfield addition, so there is no production migration.

Implementation order:
1. Establish the .NET Native AOT CLI project and publish path.
2. Implement configuration loading and `.env` ignore rules.
3. Implement the Responses API client and JSON contract.
4. Implement the search command with explicit tool selection and filters.
5. Add the local Claude testing skill.

Rollback strategy:
- Revert the added CLI project and remove the corresponding skill if the contract proves unsuitable before adoption.

## Open Questions

- Choose the exact target framework at implementation time based on the current LTS .NET SDK available in the environment.
- Decide whether a future change should expose an opt-in raw response field or stream mode once the base contract is stable.