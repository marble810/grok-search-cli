## 1. Shell detection utilities (Unix)

- [x] 1.1 Add `detect_shell()` function to `scripts/install.sh` that checks `$SHELL` and parent process name to determine the active shell (bash/zsh/fish), with a safe fallback to bash.
- [x] 1.2 Add `get_profile_path()` function that returns the appropriate profile file path for the detected shell (`~/.bashrc`, `~/.zshrc`, `~/.config/fish/config.fish`), creating parent directories as needed.

## 2. PATH addition in Unix install script

- [x] 2.1 Add `add_to_path()` function to `scripts/install.sh` that appends a sentinel-wrapped `export PATH` block to the detected shell profile, with idempotency check (skip if sentinel block already exists).
- [x] 2.2 Add `backup_profile()` function that creates a timestamped backup of the profile file before any modification.
- [x] 2.3 Replace the manual PATH guidance block (the `echo` instructions at the end of `scripts/install.sh`) with a call to `add_to_path()`, and update the success message to report which profile was modified.

## 3. PATH removal in Unix uninstall script

- [x] 3.1 Add `remove_from_path()` function to `scripts/uninstall.sh` that finds and removes the sentinel-wrapped PATH block from the detected shell profile, with a no-op when no block is found.
- [x] 3.2 Add shell detection and profile backup to `scripts/uninstall.sh` (reuse the same detection logic from install, or factor into a shared approach).
- [x] 3.3 Replace the manual PATH cleanup reminder in `scripts/uninstall.sh` with a call to `remove_from_path()`, and add detection for residual manual PATH entries after managed block removal.

## 4. Windows PowerShell install PATH automation

- [x] 4.1 Add `Add-GrokToUserPath` function to `scripts/install.ps1` that uses `[Environment]::SetEnvironmentVariable` to add the install directory to the User PATH, with idempotency check.
- [x] 4.2 Replace the manual PATH instructions in `scripts/install.ps1` with a call to `Add-GrokToUserPath`, and update the success message.

## 5. Windows PowerShell uninstall PATH cleanup

- [x] 5.1 Add `Remove-GrokFromUserPath` function to `scripts/uninstall.ps1` that removes the install directory from the User PATH via `[Environment]::SetEnvironmentVariable`, with a no-op when the entry is absent.
- [x] 5.2 Replace the manual PATH cleanup reminder in `scripts/uninstall.ps1` with a call to `Remove-GrokFromUserPath`, and add detection for residual PATH entries after removal.

## 6. Verification

- [x] 6.1 Manually verify install + uninstall idempotency on macOS (zsh): run install twice, confirm no duplicate PATH entries; run uninstall, confirm sentinel block is removed; run uninstall again, confirm clean no-op.
- [x] 6.2 Manually verify install + uninstall on Linux (bash): same idempotency checks.
- [x] 6.3 Verify profile backup files are created with correct timestamps on modification.
- [x] 6.4 Verify that when PATH was added manually (no sentinel), uninstall correctly reports the residual entry without crashing.
