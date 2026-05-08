## MODIFIED Requirements

### Requirement: Users can install grok-search-cli without administrator privileges
The project SHALL provide supported installation entrypoints for PowerShell and Bash that install grok-search-cli into a user-scoped location from GitHub Release assets and automatically configure the user's PATH to include the install directory.

#### Scenario: PowerShell install on Windows
- **WHEN** a Windows user runs the supported PowerShell installation command
- **THEN** the installer downloads the requested grok-search-cli release asset for Windows
- **THEN** the installer places the CLI in a user-scoped location without requiring administrator rights
- **THEN** the installer adds the install directory to the User PATH via the Windows registry

#### Scenario: Bash install on a Unix-like system
- **WHEN** a user runs the supported Bash installation command on a supported Unix-like environment
- **THEN** the installer downloads the requested grok-search-cli release asset for that platform
- **THEN** the installer places the CLI in a user-scoped location without requiring administrator rights
- **THEN** the installer adds the install directory to the user's shell profile PATH

### Requirement: Users can uninstall grok-search-cli without administrator privileges
The project SHALL provide supported uninstall entrypoints for PowerShell and Bash that remove the grok-search-cli executable from a user-scoped installation directory and automatically clean up managed PATH entries without requiring administrator privileges.

#### Scenario: PowerShell uninstall on Windows
- **WHEN** a Windows user runs the supported PowerShell uninstall command
- **THEN** the uninstall flow removes the managed grok-search-cli executable from the default or caller-specified install directory
- **THEN** the uninstall flow removes the install directory from the User PATH registry entry
- **THEN** the uninstall flow reports whether any managed CLI files were removed

#### Scenario: Bash uninstall on a Unix-like system
- **WHEN** a user runs the supported Bash uninstall command on a supported Unix-like environment
- **THEN** the uninstall flow removes the managed grok-search-cli executable from the default or caller-specified install directory
- **THEN** the uninstall flow removes the managed PATH entry from the user's shell profile
- **THEN** the uninstall flow reports whether any managed CLI files were removed

### Requirement: Uninstall cleanup is idempotent and bounded
The supported uninstall entrypoints SHALL be safe to rerun and SHALL remove only installer-managed CLI files and installer-managed PATH entries, while preserving separately managed credentials and reporting any remaining manual PATH cleanup the user may need.

#### Scenario: Uninstall after files are already absent
- **WHEN** a caller reruns the supported uninstall flow after the managed grok-search-cli executable has already been removed
- **THEN** the uninstall flow exits successfully without requiring force flags or administrator privileges
- **THEN** the uninstall flow reports that no managed CLI files were present to remove
- **THEN** the uninstall flow still checks for and removes any managed PATH entries

#### Scenario: Uninstall preserves credentials and reports follow-up cleanup
- **WHEN** the uninstall flow removes the managed CLI files and PATH entries from the install directory
- **THEN** it does not delete credential configuration stored through `XAI_API_KEY`, `.env`, or auth-managed storage outside the install directory
- **THEN** it reports whether any manual PATH entries referencing the install directory may still remain
