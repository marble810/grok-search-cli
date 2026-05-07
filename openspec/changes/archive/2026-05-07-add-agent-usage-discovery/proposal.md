## Why

Once release/install and auth flows exist, agents still need a reliable way to discover the CLI contract without reverse-engineering code or relying on a repo-local skill as the only source of truth. The CLI should describe itself directly so humans and agents can inspect supported commands, flags, credential prerequisites, and output behavior from the installed binary.

## What Changes

- Add explicit human-readable and machine-readable usage discovery entrypoints to the CLI.
- Publish a stable self-description document for the supported command surface so agents can inspect search and auth behavior locally.
- Update repository-local agent guidance so the skill defers to the CLI's self-description for canonical usage and keeps only repo-specific build or execution details.
- Separate discovery output from search-result output so usage inspection does not weaken the JSON contract for actual search runs.
- Keep agent-platform-specific skill installation, marketplace packaging, and remote documentation hosting out of scope for this change.

## Capabilities

### New Capabilities
- `cli-usage-discovery`: expose human-readable help and machine-readable contract output for the supported CLI command surface.

### Modified Capabilities
- `unified-search-command`: the CLI command surface gains explicit discovery entrypoints that do not require search input.
- `agent-json-output`: successful search output remains JSON-only while discovery output follows its own explicit contract.
- `agent-test-skill`: the repository-local skill defers to CLI self-description for canonical usage and keeps repo-specific guidance narrow.

## Impact

- CLI command dispatch and any shared metadata models under `src/grok-search-cli/`
- Output-shaping logic for non-search discovery responses
- Repository-local skill content under `.claude/skills/`
- Tests covering help output, machine-readable discovery output, and search-output regression boundaries