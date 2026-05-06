## Context

The repository root already establishes grok-search-cli as the project identity, but the implemented CLI still exposes xai-search through its assembly name, executable name, solution label, test references, and repository-local agent skill content. That mismatch is survivable during local prototyping, but it becomes expensive once release assets, install scripts, and agent-facing usage guidance are introduced.

This change is branding-focused rather than behavior-focused. The search contract, JSON output shape, credential loading, model selection, and tool flags should remain stable. The technical problem is to align the command identity everywhere a human or agent is likely to copy, invoke, or discover it, while preserving the existing xAI-specific upstream terminology inside API payloads.

## Goals / Non-Goals

**Goals:**
- Establish grok-search-cli as the canonical public name for the CLI and related developer-facing assets.
- Align project metadata, executable naming, solution labels, tests, and repository-local skill guidance around the same command identity.
- Keep agent-facing invocation guidance deterministic so future release and deployment work can build on a single command name.
- Avoid changing runtime behavior beyond the name updates required to keep the public contract coherent.

**Non-Goals:**
- Changing xAI Responses API terminology such as `x_search`, `web_search`, or model identifiers.
- Adding new release, install, or deployment capabilities in this change.
- Reworking the CLI output contract, credential resolution rules, or filter semantics.
- Introducing compatibility aliases unless implementation evidence shows the rename would otherwise break required flows.

## Decisions

### Use grok-search-cli as the only canonical external name
The executable, visible project labels, and invocation examples should all use grok-search-cli.

Rationale:
- It matches the repository identity the user wants to ship.
- It reduces friction in release assets, automation, and agent documentation.
- A single canonical name is easier to teach and validate than a mixed-brand setup.

Alternatives considered:
- Keeping xai-search as the command while only changing repo docs was rejected because it preserves the existing mismatch.
- Supporting two first-class command names was rejected because it complicates release and install guidance.

### Rename visible project metadata where it leaks into the developer experience
The solution entry, assembly name, generated executable name, and test/project references should be updated where they are part of commands, output, or release artifacts.

Rationale:
- Branding drift often survives through build metadata even after docs are updated.
- Release and deployment automation will key off these names.

Alternatives considered:
- Limiting the change to docs and skill text was rejected because generated outputs would still ship the old name.

### Preserve internal xAI-specific API language
The rename should not touch upstream API field names, DTO property names that mirror the API, or model/tool identifiers required by xAI.

Rationale:
- Those names are protocol-level, not product branding.
- Renaming them would add unnecessary churn and bug risk.

Alternatives considered:
- Full internal namespace and model renaming was rejected for this change because it increases scope without changing the external contract.

### Align the repository-local skill with the new command identity
The local testing skill should use grok-search-cli in its metadata, examples, and discovery path so agents do not learn an outdated invocation pattern.

Rationale:
- The skill is a direct discovery surface for agent automation.
- Keeping it aligned prevents stale examples from reintroducing the old brand.

Alternatives considered:
- Leaving the skill untouched until a later agent-focused change was rejected because it would keep teaching the wrong command name.

## Risks / Trade-offs

- [Rename reaches more files than expected] -> Constrain implementation to tracked source, tests, solution metadata, and repository-local guidance; avoid generated obj/bin output.
- [Downstream scripts still call xai-search] -> Decide during implementation whether to add a short-lived compatibility alias or treat the rename as a deliberate breaking change.
- [Partial rename leaves mixed branding] -> Validate with repository-wide targeted search for the old command name after implementation.
- [Namespace/file renames create more churn than value] -> Favor the smallest set of source-level renames needed to make the public surface coherent.

## Migration Plan

1. Update proposal-backed requirements so the canonical command identity becomes grok-search-cli.
2. Rename visible build and solution metadata plus any checked-in guidance that instructs users or agents how to invoke the CLI.
3. Run focused tests and a repository search to confirm the old command name is gone from supported invocation paths.
4. If downstream automation breakage is found, either add a temporary alias in the same change or document the rename as a breaking release note.

Rollback strategy:
- Revert the branding-alignment changeset to restore the current xai-search-facing surfaces.

## Open Questions

- Should the implementation keep a temporary `xai-search` alias for one release cycle, or treat the rename as immediately breaking?
- Should source directory and csproj file names also be renamed to grok-search-cli, or is aligning assembly/output and visible labels sufficient for this iteration?