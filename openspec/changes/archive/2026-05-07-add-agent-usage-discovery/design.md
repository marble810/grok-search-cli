## Context

The current CLI has no built-in help or contract-export surface. Agents can learn the current invocation only by reading source, external docs, or the repository-local testing skill. That is fragile once the CLI is installed through release assets because the installed binary should be able to explain its own command surface without assuming repository context.

This change follows the already agreed posture: the CLI itself becomes the source of truth for command usage, while the repository-local skill remains a thin helper for repo-specific build and test workflows. It also needs to preserve the existing JSON search-result contract so discovery features do not pollute normal automation output.

## Goals / Non-Goals

**Goals:**
- Add a first-party discovery surface for humans and agents to inspect supported commands, flags, examples, and credential prerequisites.
- Keep normal search execution separate from discovery execution so stdout expectations stay clear.
- Generate human-readable help and machine-readable usage data from shared command metadata.
- Update the local skill so it points agents to the CLI's self-description before adding repo-specific guidance.
- Sequence this work so the discovery output reflects the final branded command and the new auth surface.

**Non-Goals:**
- Implementing package-manager or marketplace skill installation.
- Replacing the local skill entirely.
- Changing search semantics, filter behavior, or upstream API payloads.
- Adding remote telemetry or online schema fetches for discovery.
- Designing a full plugin system for third-party commands.

## Decisions

### Use explicit discovery entrypoints instead of overloading search execution
The CLI will expose explicit discovery entrypoints, with human-readable help and a machine-readable `describe --json` style command, rather than expecting agents to scrape error messages or infer flags from source.

Rationale:
- Explicit discovery is easier to automate and test.
- It keeps search invocation rules separate from introspection.
- It creates one stable place for future command groups such as `auth`.

Alternatives considered:
- Relying on the repo-local skill alone was rejected because installed binaries need a first-party description surface.
- Teaching agents to parse `--help` text only was rejected because a machine-readable contract is needed for robust automation.

### Drive help text and JSON discovery from shared command metadata
Human help output and machine-readable discovery output will be generated from a single in-process description model for commands, flags, precedence notes, examples, and output expectations.

Rationale:
- One source of truth reduces drift.
- It keeps examples and machine-readable metadata aligned.
- It gives tests a deterministic contract to validate.

Alternatives considered:
- Maintaining separate hand-written text and JSON documents was rejected because they would diverge quickly.

### Preserve JSON-only stdout for search runs
Search execution will continue to own the existing JSON result contract, while discovery entrypoints will have their own explicit output mode and will never trigger a network search request.

Rationale:
- Existing automation depends on search results being cleanly machine-readable.
- Discovery must not require credentials or network access just to explain usage.

Alternatives considered:
- Printing incidental help or warnings during search runs was rejected because it would weaken automation guarantees.

### Keep the repo-local skill as a thin adapter
The skill should tell agents to inspect the CLI's self-description first and only add repository-specific build, run, and implementation-style guidance that the installed binary cannot know.

Rationale:
- It reduces duplicate contract text.
- It keeps the skill useful inside the repo without making it the sole source of truth.

Alternatives considered:
- Removing the skill entirely was rejected because repo-local shortcuts and implementation posture still belong there.

### Implement discovery after branding and auth
The discovery change should follow branding alignment and auth-command work so the self-description reflects the final executable name and full command tree on first release.

Rationale:
- It avoids churn in command literals and examples.
- It lets the first exported contract include both search and auth surfaces.

Alternatives considered:
- Shipping discovery before auth was rejected because it would require an immediate follow-up rewrite of the exported command graph.

## Risks / Trade-offs

- [Discovery metadata drifts from actual parser behavior] -> Centralize command definitions and validate discovery output with focused tests.
- [Human help and JSON export compete for precedence] -> Keep both generated from the same description model and document each entrypoint clearly.
- [Discovery work lands before auth or branding] -> Treat this change as sequenced after those changes or update it only after the command tree is stable.
- [Skill guidance still duplicates too much contract text] -> Trim the skill down to repo-local concerns and link examples back to discovery output.

## Migration Plan

1. Land branding alignment and auth bootstrap so the command surface is stable.
2. Add shared command-description metadata plus explicit help and machine-readable discovery entrypoints.
3. Update the local skill and docs to point to the discovery entrypoints instead of duplicating the full contract.
4. Add regression tests that cover discovery output and confirm search stdout remains unchanged.

Rollback strategy:
- Remove the discovery entrypoints and revert the skill to its current fully descriptive form until a corrected self-description surface is ready.

## Open Questions

- Should the machine-readable entrypoint be spelled `describe --json`, `help --json`, or another explicit verb once branding lands?
- How much example data should the JSON discovery output include before it becomes too verbose for routine agent inspection?