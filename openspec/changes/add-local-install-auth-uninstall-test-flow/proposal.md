## Why

The current install and uninstall verification path depends on GitHub-hosted raw scripts and published release assets, so contributors cannot exercise the full install -> auth -> uninstall lifecycle locally before a release exists. That leaves a gap between developer validation and the documented user lifecycle, especially on Windows where the published PowerShell invocation is currently the only scripted install path in the test guide.

## What Changes

- Add a supported local-validation path for the installer lifecycle so contributors can build release-like artifacts in the repository and run install and uninstall flows without fetching scripts or payloads from GitHub.
- Extend the installer contract to accept a caller-specified local artifact source that preserves the existing archive and checksum naming rules, so local validation exercises the same package resolution logic as release installs.
- Document a pure local install -> auth -> uninstall test workflow for Bash and PowerShell, including local setup, lifecycle assertions, and cleanup expectations.
- Add verification coverage for the local lifecycle path so maintainers can catch regressions in local artifact resolution, installer messaging, and uninstall boundaries before publishing releases.

## Capabilities

### New Capabilities
- `local-lifecycle-validation`: define the supported repository-local workflow for building release-like artifacts and validating the install, auth, and uninstall lifecycle without a published GitHub release.

### Modified Capabilities
- `user-installation`: allow supported installers to resolve release-like assets from a caller-provided local source in addition to the default GitHub Release path so the lifecycle can be validated locally.

## Impact

- Root installer scripts and any shared packaging or asset-resolution helpers
- Manual test documentation under `docs/test/`, especially the installation and auth lifecycle guide
- Release or smoke-test coverage that currently assumes only GitHub-hosted install assets
- Developer workflow for validating install, auth, and uninstall behavior before a release is published