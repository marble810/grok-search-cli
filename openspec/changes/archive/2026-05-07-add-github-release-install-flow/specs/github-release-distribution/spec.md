## ADDED Requirements

### Requirement: Version tags publish GitHub Release assets
The repository SHALL publish versioned grok-search-cli release assets through GitHub Releases when a supported release tag is created.

#### Scenario: Tagged release builds assets
- **WHEN** a maintainer pushes or creates a supported release tag
- **THEN** GitHub Actions builds the configured grok-search-cli release artifacts
- **THEN** the workflow attaches those artifacts to the matching GitHub Release

#### Scenario: Release build fails
- **WHEN** one of the required release artifacts fails to build or package
- **THEN** the workflow reports the failure and does not publish a partial successful release set as complete

### Requirement: Release assets follow a stable machine-consumable naming convention
The published release SHALL expose per-platform artifact names and checksum data that installation scripts can resolve deterministically.

#### Scenario: Install script resolves asset URL
- **WHEN** an installer targets a supported version and platform
- **THEN** it can derive the expected release asset name from the documented naming convention

#### Scenario: Integrity data is available
- **WHEN** a user or script downloads a release asset
- **THEN** the corresponding release provides checksum data for integrity verification