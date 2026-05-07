## ADDED Requirements

### Requirement: Repository documents a pure local lifecycle validation flow
The repository SHALL document a supported local workflow for validating install, auth, and uninstall behavior without fetching installer scripts or release assets from GitHub.

#### Scenario: Contributor prepares local lifecycle validation
- **WHEN** a contributor follows the local lifecycle test guide from a fresh checkout
- **THEN** the guide identifies how to build or package release-like local assets before running installer validation
- **THEN** the guide distinguishes local validation steps from post-release GitHub-hosted verification steps

#### Scenario: Windows local lifecycle validation
- **WHEN** a contributor runs the documented PowerShell local workflow
- **THEN** the workflow installs the CLI from repository-local assets, validates the auth lifecycle, and removes the installed CLI without requiring remote release downloads

#### Scenario: Unix local lifecycle validation
- **WHEN** a contributor runs the documented Bash local workflow
- **THEN** the workflow installs the CLI from repository-local assets, validates the auth lifecycle, and removes the installed CLI without requiring remote release downloads

### Requirement: Local validation artifacts preserve the release contract
The repository SHALL provide a supported way to produce local validation artifacts whose filenames and checksum layout match the published release asset naming convention.

#### Scenario: Local validation assets are packaged predictably
- **WHEN** a maintainer or contributor prepares local validation assets for a supported version tag
- **THEN** the output includes the platform archive name and checksum filename expected by the installers for that version
- **THEN** the local assets can be consumed by the installer without renaming or manual patching

### Requirement: Local lifecycle coverage guards regressions
The repository SHALL include automated verification that exercises the local validation path closely enough to catch regressions in local asset resolution and lifecycle guidance.

#### Scenario: Local installer override remains covered
- **WHEN** repository tests or smoke checks validate the install lifecycle
- **THEN** they assert that the supported local asset override is still recognized by the installer scripts
- **THEN** they verify that lifecycle messaging for install handoff or uninstall boundaries remains intact for local validation