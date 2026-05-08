## Context

The install scripts currently detect whether the install directory is in PATH and print manual copy-paste instructions tailored to bash, zsh, and fish. The uninstall scripts similarly detect whether the directory is in PATH and remind users to clean up. These prompts are helpful but manual—they add friction to onboarding and risk stale entries when users forget to remove them.

The target behavior is for install to add the entry automatically and uninstall to remove it automatically, across platforms (Linux, macOS, Windows) and shells (bash, zsh, fish, PowerShell). The scripts already have shell-specific awareness in their messaging; this change upgrades that awareness into action.

## Goals / Non-Goals

**Goals:**
- Automatically add the install directory to the user's shell profile PATH on Unix (bash, zsh, fish) and to the User PATH registry on Windows (PowerShell).
- Automatically remove the entry during uninstall, with the same platform/shell coverage.
- Detect the user's active shell reliably without requiring explicit `--shell` flags.
- Ensure idempotency: duplicate runs must not create duplicate PATH entries.
- Preserve safety: create timestamped backups before editing shell profile files; log what was changed so users can revert manually.

**Non-Goals:**
- System-wide PATH configuration (requires admin/sudo). Only user-scoped PATH is modified.
- Editing shell profiles for shells the user is not actively using.
- Supporting csh/tcsh or other niche shells—only bash, zsh, fish, and PowerShell.
- Modifying credential configuration or any other environment variables beyond PATH.

## Decisions

### Edit shell profile files for Unix, use registry for Windows

On Unix, we append/remove a marked `export PATH=...` block to the user's shell profile file (`~/.bashrc`, `~/.zshrc`, `~/.config/fish/config.fish`). On Windows, we use `[Environment]::SetEnvironmentVariable` to manage the User PATH registry entry (which is how Windows PowerShell natively manages persistent PATH).

**Rationale:**
- Shell profile files are the idiomatic, portable way to set persistent PATH on Unix. Marking the block with sentinel comments makes it safe to identify and remove later.
- The Windows User PATH registry is the canonical persistent PATH store on Windows. Editing PowerShell profile files would miss cmd.exe and other contexts, while the registry entry is universal.

**Alternatives considered:**
- Using `path_helper` or `/etc/paths.d` on macOS was rejected because it requires `sudo` and is not user-scoped.
- Editing `~/.profile` was rejected because it's not sourced by all modern shells (zsh on macOS reads `.zprofile` instead).

### Shell detection via parent process chain and environment variables

The Unix scripts detect the shell by checking `$SHELL` first, then examining the parent process name (`ps -p $PPID -o comm=`). On Windows, PowerShell is the only supported shell for the `.ps1` scripts.

**Rationale:**
- `$SHELL` is set by the login process and is the most common indicator of user preference.
- `$PPID` inspection catches cases where the user invokes a different shell than their login default (e.g., `bash` inside a zsh session).
- We use both to prioritize the actual running shell over the login shell.

**Alternatives considered:**
- `$0` inspection was rejected because it can be unreliable in sourced scripts.
- Auto-detecting all shells in PATH was rejected as over-engineered—we only need the active one.

### Sentinels in profile files for safe add/remove

Each PATH entry inserted into a Unix profile file is wrapped with sentinel comments: `# grok-search-cli PATH begin` and `# grok-search-cli PATH end`. Uninstall finds and removes the entire block including these sentinels. This avoids fragile line-by-line matching against PATH values that may have been manually edited.

**Rationale:**
- A delimited block is unambiguous to find and safe to remove.
- It survives whitespace reformatting and minor user edits around the block.

**Alternatives considered:**
- Matching the exact `export PATH=` line by regex was rejected because users might edit the PATH value or surrounding comments, breaking matching.
- Storing a separate tracking file (e.g., `~/.grok-search-cli/path-entries`) was rejected as unnecessary state; sentinels in the actual profile are self-documenting.

### Idempotent operations

Install checks for existing sentinel blocks and skips adding if already present. Uninstall checks for sentinel blocks and removes them if found, exiting cleanly if none exist.

**Rationale:**
- Users may run install/uninstall multiple times. Duplicate PATH entries are confusing and degrade shell startup performance.

## Risks / Trade-offs

- [Profile corruption from concurrent edits] → The scripts edit profile files using shell redirects (`>>`), which are atomic for line appends but not safe against concurrent install runs. Mitigation: document that install scripts should not be run concurrently.
- [Shell detection edge cases] → Some container or CI environments may have unusual `$SHELL` values (e.g., `/bin/sh` pointing to dash). Mitigation: fall back to bash profile when detection is ambiguous, and always print a note about which file was modified.
- [Fish shell requires a directory] → `~/.config/fish/config.fish` may need the parent directory created. Mitigation: `mkdir -p` before writing.
- [Windows registry corruption] → `SetEnvironmentVariable` persists immediately and globally for the user. Mitigation: this API is the documented Windows approach and is widely used by installers.

## Migration Plan

1. Deploy updated install/uninstall scripts. Users who already installed manually will have their existing PATH entries untouched—the new installers will detect the sentinel block is missing and add it.
2. Users with manually-added PATH entries from the old installer will have both the manual entry and the new sentinel-wrapped entry. The uninstaller will remove only the sentinel-wrapped entry and report if a manual entry may remain (detected by checking `:${PATH}:` after removal).
3. Rollback: revert to old scripts that print instructions. Users keep any PATH entries added by the new scripts until they manually edit their profiles.

## Open Questions

- Whether to support `--no-path-modify` flag for users who prefer manual PATH management (e.g., in managed environments). This can be decided during implementation.
