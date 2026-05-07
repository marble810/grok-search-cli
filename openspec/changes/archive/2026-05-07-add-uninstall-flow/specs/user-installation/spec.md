## ADDED Requirements

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