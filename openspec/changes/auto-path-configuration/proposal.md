## Why

Currently, install scripts only print manual PATH-adding instructions to stderr, and uninstall scripts only print manual PATH-removal reminders. Users must copy-paste shell-specific commands and remember to clean up profiles later. Automating this eliminates a common onboarding friction point and the risk of stale PATH entries accumulating across shell profiles.

## What Changes

- Install scripts automatically detect the user's shell (bash, zsh, fish, PowerShell) and add the install directory to PATH in the appropriate shell profile file, instead of printing manual instructions.
- Uninstall scripts automatically remove the install directory from PATH in the detected shell profiles, instead of printing manual reminders.
- Both operations are idempotent: re-running install does not duplicate entries, re-running uninstall safely handles already-absent entries.
- The automatic behavior respects platform conventions: `~/.bashrc`/`~/.zshrc`/`~/.config/fish/config.fish` on Unix, and User PATH registry on Windows.
- Install and uninstall scripts remain non-destructive: they create timestamped backups of modified profile files, and the PowerShell variant uses the registry-based User PATH (no profile file editing on Windows).

## Capabilities

### New Capabilities
- `shell-profile-management`: detect the user's active shell and platform, then safely add or remove the install directory from PATH in the appropriate shell profile or system configuration.

### Modified Capabilities
- `user-installation`: install and uninstall entrypoints SHALL automatically configure and clean up PATH instead of only printing manual instructions.

## Impact

- `scripts/install.sh`, `scripts/uninstall.sh` — add shell detection, profile file editing with backup, PATH entry management
- `scripts/install.ps1`, `scripts/uninstall.ps1` — add User PATH registry manipulation for install/uninstall
- `openspec/specs/user-installation/spec.md` — updated requirements for automatic PATH management
- New spec `openspec/specs/shell-profile-management/spec.md` — shell and platform detection behavior
