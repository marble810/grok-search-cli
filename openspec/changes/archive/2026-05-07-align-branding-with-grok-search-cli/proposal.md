## Why

The repository directory and future release surface are named grok-search-cli, but the implemented CLI still presents itself as xai-search across the executable name, project metadata, skill guidance, and user-facing references. This split branding will leak into release assets, install instructions, and agent automation, so the public name should be aligned before release and deployment work begins.

## What Changes

- Rename the CLI's public-facing brand from xai-search to grok-search-cli across the executable name, project metadata, and invocation guidance.
- Align repository-local agent guidance and examples so skills, docs, and scripted usage all point to grok-search-cli as the canonical command name.
- Update solution and test-facing project labels where they are part of the visible developer experience, while keeping the change scoped to branding rather than unrelated runtime behavior.
- Keep existing search behavior, output contract, and credential rules unchanged unless a user-facing name must be updated to reflect the new brand.

## Capabilities

### New Capabilities
<!-- None. -->

### Modified Capabilities
- `unified-search-command`: the command identity and invocation examples should use grok-search-cli as the canonical executable name.
- `agent-test-skill`: the repository-local testing skill should describe and exercise grok-search-cli rather than xai-search.

## Impact

- Affected code and project metadata under `src/xai-search-cli/` and `tests/xai-search-cli.Tests/`
- Solution and build metadata in `grok-search-cli.sln`
- Repository-local agent guidance under `.claude/skills/`
- Existing OpenSpec requirement text and future release/install instructions that currently mention xai-search