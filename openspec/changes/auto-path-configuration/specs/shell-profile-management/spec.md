## ADDED Requirements

### Requirement: Installer detects the user's shell and platform
The installer SHALL detect the user's active shell on Unix-like systems and the platform on Windows, then select the appropriate PATH configuration mechanism for that environment.

#### Scenario: Bash detected on Linux
- **WHEN** the installer runs on Linux and `$SHELL` or the parent process indicates bash
- **THEN** the installer modifies `~/.bashrc` for PATH configuration

#### Scenario: Zsh detected on macOS
- **WHEN** the installer runs on macOS and `$SHELL` or the parent process indicates zsh
- **THEN** the installer modifies `~/.zshrc` for PATH configuration

#### Scenario: Fish detected
- **WHEN** the installer runs and `$SHELL` or the parent process indicates fish
- **THEN** the installer modifies `~/.config/fish/config.fish` for PATH configuration

#### Scenario: PowerShell on Windows
- **WHEN** the installer runs on Windows via PowerShell
- **THEN** the installer modifies the User PATH via the Windows registry

#### Scenario: Ambiguous shell detection
- **WHEN** the installer cannot determine the shell or the detected shell is unsupported
- **THEN** the installer falls back to modifying `~/.bashrc` and prints a note about which file was modified

### Requirement: PATH entry is wrapped with identifiable sentinels
The installer SHALL wrap each PATH modification block in Unix profile files with sentinel comments that uniquely identify the block as managed by grok-search-cli.

#### Scenario: Sentinel-wrapped PATH block is appended
- **WHEN** the installer adds PATH to a Unix shell profile
- **THEN** the entry is wrapped with `# grok-search-cli PATH begin` and `# grok-search-cli PATH end` sentinel comments
- **THEN** the comment block contains the full `export PATH="...:$PATH"` line

### Requirement: PATH additions are idempotent
The installer SHALL check for an existing grok-search-cli sentinel block before adding PATH entries, and SHALL not add duplicate entries.

#### Scenario: Install run when PATH block already exists
- **WHEN** the installer detects an existing grok-search-cli PATH block in the target profile
- **THEN** it skips adding a new entry and reports that PATH is already configured

#### Scenario: Install run when entry is missing
- **WHEN** the installer detects no grok-search-cli PATH block in the target profile
- **THEN** it appends a new sentinel-wrapped PATH entry

### Requirement: Profile modifications create a backup
The installer and uninstaller SHALL create a timestamped backup of any Unix profile file before modifying it.

#### Scenario: Backup created before profile modification
- **WHEN** the installer or uninstaller modifies a shell profile file
- **THEN** a copy of the file is saved with a `.grok-search-cli-backup-<timestamp>` suffix in the same directory

### Requirement: Uninstaller removes managed PATH entries
The uninstaller SHALL detect and remove grok-search-cli-managed PATH blocks from shell profiles, and SHALL remove the install directory from the Windows User PATH.

#### Scenario: Uninstall removes sentinel-wrapped PATH block on Unix
- **WHEN** the uninstaller runs on a Unix-like system
- **THEN** it removes the entire block between `# grok-search-cli PATH begin` and `# grok-search-cli PATH end` sentinels
- **THEN** it reports which profile file was modified

#### Scenario: Uninstall removes User PATH entry on Windows
- **WHEN** the uninstaller runs on Windows via PowerShell
- **THEN** it removes the install directory from the User PATH environment variable via the registry
- **THEN** it reports that the User PATH was updated

#### Scenario: Uninstall when no managed PATH block exists
- **WHEN** the uninstaller finds no grok-search-cli sentinel block and no install directory in PATH
- **THEN** it exits successfully without error and reports that no PATH cleanup was needed

### Requirement: Uninstaller reports residual manual PATH entries
The uninstaller SHALL detect whether the install directory remains in the runtime PATH after removing managed entries, and SHALL advise the user of any manual cleanup needed.

#### Scenario: Manual PATH entry remains after uninstall
- **WHEN** the uninstaller removes the managed PATH block but the install directory is still in the runtime `$PATH`
- **THEN** the uninstaller reports that a manual PATH entry may remain and advises the user to check their shell profiles
