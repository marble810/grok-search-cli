# user-installation Specification

## Purpose
TBD - created by archiving change add-github-release-install-flow. Update Purpose after archive.
## Requirements
### Requirement: Users can install grok-search-cli without administrator privileges
The project SHALL provide supported installation entrypoints for PowerShell and Bash that install grok-search-cli into a user-scoped location from GitHub Release assets.

#### Scenario: PowerShell install on Windows
- **WHEN** a Windows user runs the supported PowerShell installation command
- **THEN** the installer downloads the requested grok-search-cli release asset for Windows
- **THEN** the installer places the CLI in a user-scoped location without requiring administrator rights

#### Scenario: Bash install on a Unix-like system
- **WHEN** a user runs the supported Bash installation command on a supported Unix-like environment
- **THEN** the installer downloads the requested grok-search-cli release asset for that platform
- **THEN** the installer places the CLI in a user-scoped location without requiring administrator rights

### Requirement: Installation scripts do not collect secrets
The supported installation entrypoints SHALL not prompt for, persist, or otherwise collect `XAI_API_KEY` during installation.

#### Scenario: Human-operated install completes
- **WHEN** a human runs the supported installer interactively
- **THEN** the installer completes without prompting for `XAI_API_KEY`
- **THEN** the installer points the user to the supported post-install auth or manual credential setup path

#### Scenario: Unattended or agent-driven install
- **WHEN** the installer is running in an unattended or agent-driven context
- **THEN** the installer completes without any credential prompt branch
- **THEN** the installer leaves runtime credential configuration to a separate auth or environment setup step

### Requirement: Installation flow supports deterministic upgrades
The supported installation entrypoints SHALL allow a caller to install a specific version or refresh an existing user-scoped installation from GitHub Release assets.

#### Scenario: Install a pinned version
- **WHEN** a caller requests a specific release version
- **THEN** the installer fetches that exact version rather than guessing the latest available asset

#### Scenario: Upgrade existing install
- **WHEN** a caller reruns the installer for a newer supported version
- **THEN** the installer replaces the existing user-scoped CLI with the requested version and reports the installed version clearly

### Requirement: Users can uninstall grok-search-cli without administrator privileges
The project SHALL provide supported uninstall entrypoints for PowerShell and Bash that remove the grok-search-cli executable from a user-scoped installation directory without requiring administrator privileges.

#### Scenario: PowerShell uninstall on Windows
- **WHEN** a Windows user runs the supported PowerShell uninstall command
- **THEN** the uninstall flow removes the managed grok-search-cli executable from the default or caller-specified install directory
- **THEN** the uninstall flow reports whether any managed CLI files were removed

#### Scenario: Bash uninstall on a Unix-like system
- **WHEN** a user runs the supported Bash uninstall command on a supported Unix-like environment
- **THEN** the uninstall flow removes the managed grok-search-cli executable from the default or caller-specified install directory
- **THEN** the uninstall flow reports whether any managed CLI files were removed

### Requirement: Uninstall cleanup is idempotent and bounded
The supported uninstall entrypoints SHALL be safe to rerun and SHALL remove only installer-managed CLI files, while preserving separately managed credentials and reporting any remaining manual PATH cleanup the user may need.

#### Scenario: Uninstall after files are already absent
- **WHEN** a caller reruns the supported uninstall flow after the managed grok-search-cli executable has already been removed
- **THEN** the uninstall flow exits successfully without requiring force flags or administrator privileges
- **THEN** the uninstall flow reports that no managed CLI files were present to remove

#### Scenario: Uninstall preserves credentials and reports follow-up cleanup
- **WHEN** the uninstall flow removes the managed CLI files from the install directory
- **THEN** it does not delete credential configuration stored through `XAI_API_KEY`, `.env`, or auth-managed storage outside the install directory
- **THEN** it reports whether the install directory may still need manual PATH cleanup after uninstall

