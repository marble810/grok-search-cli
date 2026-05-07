## Context

The repository already has first-party PowerShell and Bash installers, an auth lifecycle in the compiled CLI, uninstall scripts, and manual test docs that describe the intended user journey. What is missing is a supported way to exercise that exact journey locally before a GitHub release exists. Today, local development can compile the binary, but the documented installer path still depends on GitHub-hosted scripts and release assets, which means contributors cannot validate archive naming, checksum verification, install messaging, auth handoff, and uninstall cleanup as one local end-to-end flow.

This change crosses multiple surfaces: packaging, installer resolution, manual test docs, and smoke coverage. The design needs to preserve the released-user path as the default while adding a deterministic local override that is narrow enough for contributors and CI to use safely.

## Goals / Non-Goals

**Goals:**
- Let contributors build release-like local assets and run the supported install -> auth -> uninstall lifecycle without GitHub Releases.
- Reuse the same archive names and checksum layout as published release assets so local validation exercises the real installer contract.
- Keep the default install behavior unchanged for normal users while adding an explicit opt-in local source for testing.
- Update manual test docs to describe a pure local workflow for Bash and PowerShell.
- Add automated verification for local asset resolution and the lifecycle messages that should remain stable.

**Non-Goals:**
- Replacing GitHub Releases as the primary distribution path.
- Changing auth command semantics, credential precedence, or managed-store behavior.
- Adding a new long-lived packaging service or artifact registry.
- Making installers guess local versions automatically from arbitrary directories without caller intent.

## Decisions

### Add an explicit local asset directory override to the installers
Add an opt-in parameter to each installer that points at a local directory containing release-like archives and checksum files. The default path remains GitHub Releases, but when the override is supplied the script resolves the archive and checksum from disk instead of downloading them.

Rationale:
- A filesystem directory is simpler and more portable for local validation than teaching the scripts to support both HTTP and `file://` URL semantics.
- It keeps the released-user path unchanged while making local mode obvious and testable.
- The same directory contract works for both PowerShell and Bash without introducing network requirements.

Alternatives considered:
- Supporting an arbitrary base URL was rejected because it broadens the fetch surface unnecessarily for a local-only validation need.
- Adding a separate local-only installer script was rejected because it would fork behavior away from the supported install entrypoints.

### Require release-like local artifacts instead of raw binaries
Introduce a local packaging step that emits the same archive and checksum names already used by release installs. Local validation will target those packaged artifacts rather than copying the compiled binary directly.

Rationale:
- This validates the naming and checksum contract, not just the final executable.
- It keeps local and released install flows structurally aligned, which lowers the chance of release-only regressions.

Alternatives considered:
- Pointing the local workflow at `dotnet build` output directly was rejected because it bypasses the packaging and checksum behavior the install scripts actually depend on.

### Make local mode explicit about version selection
When an installer is given a local asset directory, the caller must also provide the version tag to install. The scripts resolve the expected archive and checksum names from that version and platform RID and fail clearly if the files are missing.

Rationale:
- Local directories do not have a canonical “latest release” concept.
- Requiring an explicit version keeps local test runs deterministic and avoids filename guessing heuristics.

Alternatives considered:
- Scanning local files to infer the newest semantic version was rejected because it adds parsing complexity and nondeterministic behavior for a test-only path.

### Document lifecycle validation as a repository-local workflow
Update the manual test docs to describe how to build local artifacts, invoke the local installer entrypoints, run auth checks, and remove the installation afterward. The release-only remote commands remain documented separately for published-release validation.

Rationale:
- Contributors need a supported workflow before release, not an ad hoc note buried in implementation details.
- Keeping both local and release validation paths in the docs clarifies which path to use during development versus release verification.

Alternatives considered:
- Replacing the published-release test steps entirely was rejected because post-release validation against GitHub-hosted assets is still needed.

## Risks / Trade-offs

- [Installer options diverge across shells] -> Mirror parameter names and behavior as closely as each shell allows, and cover both scripts in smoke tests.
- [Local packaging output drifts from release naming] -> Reuse the existing release asset naming convention and add tests that assert the expected archive and checksum filenames.
- [Contributors misunderstand local mode as a supported end-user distribution channel] -> Keep local source selection opt-in, document it as validation-only, and leave GitHub Releases as the default path.
- [Manual tests become longer or harder to follow] -> Split local setup from lifecycle assertions and keep release-only checks in a separate subsection.

## Migration Plan

1. Add a repository-local packaging path that produces release-like archives and checksums for supported local validation.
2. Extend PowerShell and Bash installers with an explicit local asset directory override and deterministic error handling for missing local files.
3. Update manual test docs to add a pure local install -> auth -> uninstall workflow while preserving release validation steps.
4. Extend smoke tests or script-focused tests to cover the local asset path and stable lifecycle messaging.

Rollback strategy:
- Remove the local asset override and local packaging guidance, then revert the docs to the current release-only installer validation path.

## Open Questions

- Whether the local packaging step should be a documented `dotnet publish` plus archive commands sequence or a dedicated helper script can be decided during implementation, as long as the output naming stays aligned with the release contract.