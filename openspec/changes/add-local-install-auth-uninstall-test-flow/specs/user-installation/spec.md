## ADDED Requirements

### Requirement: Installation scripts support local validation assets
The supported PowerShell and Bash installation entrypoints SHALL allow a caller to install a specific grok-search-cli version from a caller-provided local asset directory that contains the same archive and checksum filenames used by published release assets.

#### Scenario: PowerShell install from local validation assets
- **WHEN** a Windows caller runs the supported PowerShell installer with an explicit version and a local asset directory containing the matching `win-*` archive and checksum files
- **THEN** the installer resolves the expected local asset names for the current platform without querying GitHub Releases
- **THEN** the installer verifies the checksum and installs the CLI into the requested user-scoped directory

#### Scenario: Bash install from local validation assets
- **WHEN** a caller runs the supported Bash installer with an explicit version and a local asset directory containing the matching Unix archive and checksum files
- **THEN** the installer resolves the expected local asset names for the current platform without querying GitHub Releases
- **THEN** the installer verifies the checksum and installs the CLI into the requested user-scoped directory

#### Scenario: Missing local validation asset is rejected deterministically
- **WHEN** a caller selects a local asset directory but the expected archive or checksum file for the requested version and platform is missing
- **THEN** the installer exits with a clear error that identifies the missing local asset contract
- **THEN** it does not fall back silently to downloading from GitHub Releases