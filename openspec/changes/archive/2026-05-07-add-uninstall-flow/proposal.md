## Why

The project now has a supported install and upgrade path, but it still lacks a supported way to remove a user-scoped installation cleanly. That leaves users and agents guessing which files to delete, whether PATH cleanup is required, and whether uninstalling the CLI should affect separately managed credentials.

## What Changes

- Add supported uninstall entrypoints for PowerShell and Bash that remove the user-scoped grok-search-cli binary and any installer-owned wrapper files from the default or caller-specified install directory.
- Define uninstall behavior for already-missing files so rerunning the uninstall flow is safe and reports a clear outcome instead of failing noisily.
- Document the cleanup boundary explicitly: uninstall removes CLI install artifacts, reports any remaining manual PATH cleanup the user may need, and does not delete credentials configured through environment variables, `.env`, or the auth flow.
- Add release smoke coverage and installation documentation updates so install and uninstall are documented as a matched lifecycle.

## Capabilities

### New Capabilities
<!-- None. -->

### Modified Capabilities
- `user-installation`: add supported uninstall requirements, idempotent cleanup behavior, and documented handoff for PATH and credential state after removal.

## Impact

- Root-level installer lifecycle scripts and any shared helper logic for locating install directories
- Installation and release documentation, including lifecycle guidance for removing the CLI
- Release smoke coverage for supported install artifacts and uninstall behavior
- User expectations around PATH cleanup and credential persistence after uninstall