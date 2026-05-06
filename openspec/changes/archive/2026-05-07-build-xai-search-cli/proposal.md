## Why

Teams increasingly want small, scriptable search tools that AI agents can call without UI-oriented noise or bespoke SDK glue. This change defines a .NET AOT CLI that wraps xAI Responses API search tools so agents can issue web and X searches through a stable, machine-readable contract.

## What Changes

- Add a .NET AOT command-line tool focused on agent invocation rather than human-oriented terminal UX.
- Support xAI Web Search and X Search through an explicit tool selector: `web`, `x`, or `both`.
- Return a stable JSON result on stdout containing the model's final answer, citations, and basic metadata.
- Send diagnostics, warnings, and failures to stderr with meaningful exit codes.
- Load credentials from environment variables and `.env`, keeping secrets out of command history and process arguments.
- Ignore `.env` in version control as part of the runtime setup contract.
- Standardize a small set of stable search filters instead of exposing raw request passthrough.
- Add a companion Claude skill under `.claude/skills` so agents can exercise the CLI consistently during testing.
- Constrain the implementation style toward let-it-crash semantics, minimal defensive branching, concise code paths, and high-performance defaults.

## Capabilities

### New Capabilities
- `unified-search-command`: Provide one CLI search command that can execute Web Search, X Search, or both via xAI Responses API using `grok-4-1-fast-reasoning`.
- `agent-json-output`: Produce agent-safe JSON on stdout with citations and metadata, while reserving stderr for diagnostics and using deterministic exit codes.
- `runtime-configuration`: Resolve API credentials and runtime defaults from environment variables and `.env` in a way that remains compatible with .NET AOT packaging.
- `agent-test-skill`: Provide a repository-local Claude skill that teaches agents how to invoke and validate the CLI contract during testing.

### Modified Capabilities
- None.

## Impact

- Adds a new .NET native AOT CLI implementation and build/publish workflow.
- Introduces an xAI Responses API integration layer for `web_search` and `x_search` tools.
- Defines the initial external CLI contract for agent callers.
- Adds a repository-local agent skill for repeatable CLI testing.
- Requires dependency choices that are safe for AOT, `.env` loading, JSON serialization, and HTTP interactions.