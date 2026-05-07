## Context

The repository already treats installation as a supported lifecycle surface: it publishes release assets, ships root-level PowerShell and Bash installers, documents install and upgrade commands, and smoke-tests the install scripts. What is missing is the inverse path. Users who installed into the default user-scoped directories have no supported command to remove the binary, and callers using a custom install directory have no documented way to target that location consistently.

The design needs to extend the existing install lifecycle without broadening responsibility into credential deletion or shell-profile mutation. The current installers only print PATH guidance and hand credentials off to the auth flow, so uninstall should preserve that ownership boundary rather than inventing new side effects.

## Goals / Non-Goals

**Goals:**
- Add first-party PowerShell and Bash uninstall entrypoints that mirror the existing install surfaces.
- Reuse the same install-directory defaults and custom-directory options so uninstall can find the managed binary deterministically.
- Make uninstall safe to rerun and clear about whether it removed anything.
- Keep uninstall bounded to installer-owned CLI files while documenting any remaining PATH or credential follow-up.
- Extend docs and smoke tests so install, upgrade, and uninstall are covered as one supported lifecycle.

**Non-Goals:**
- Deleting credential data from environment variables, `.env` files, or auth-managed stores.
- Editing arbitrary shell startup files or Windows environment settings to remove PATH entries automatically.
- Removing system-wide installs or cleaning directories not owned by the current installer contract.
- Introducing a new compiled CLI subcommand for self-uninstall.

## Decisions

### Add dedicated uninstall scripts rather than overloading the installers
Add `uninstall.ps1` and `uninstall.sh` beside the existing install scripts, with their own one-line invocation examples and help text.

Rationale:
- It matches the current release surface, where PowerShell and Bash are already the supported operator entrypoints.
- A dedicated uninstall script is easier to document and discover than an installer-only `--uninstall` branch.
- It avoids mixing download logic with deletion logic inside one script and keeps each lifecycle action small.

Alternatives considered:
- Adding `--uninstall` to `install.ps1` and `install.sh` was rejected because it makes usage and test coverage less direct.
- Adding a CLI `self uninstall` command was rejected because uninstall should work even when the installed binary is broken or absent.

### Resolve uninstall targets with the same directory contract as install
Each uninstall script will use the same default install directory as its matching installer and accept the same caller override (`-InstallDir` for PowerShell, `--dir` for Bash).

Rationale:
- It keeps the lifecycle deterministic for both default and custom installs.
- It avoids adding state files or registry entries just to remember where the binary was placed.

Alternatives considered:
- Recording install metadata during install was rejected because it adds persistence and migration complexity for a simple user-scoped tool.

### Remove only installer-managed CLI files and only prune directories when safe
The uninstall scripts will delete the managed executable and any future installer-owned wrapper files in the target directory. They may remove the target directory afterward only when it becomes empty, and they will leave unrelated files in place.

Rationale:
- It satisfies the cleanup goal without risking user data loss in shared directories.
- It keeps reruns predictable because the managed file set is explicit.

Alternatives considered:
- Recursively deleting the install root was rejected because custom directories may contain unrelated tools.

### Report follow-up instead of mutating PATH or credentials
Uninstall will emit a clear summary of what was removed, state when nothing managed was found, and remind the user to clean PATH references manually if needed. Credential stores and environment configuration remain untouched.

Rationale:
- The installer does not own shell profile edits or credential storage, so uninstall should not guess at them.
- Manual follow-up messaging is safer than trying to reverse environment changes across shells and platforms.

Alternatives considered:
- Auto-editing shell profile files or user PATH values was rejected because the install flow never established a single reversible ownership model for those changes.

## Risks / Trade-offs

- [Custom install directories may be forgotten by callers] -> Keep the uninstall scripts' default directories aligned with install, and document the same override flag clearly.
- [Users may expect uninstall to remove every trace of prior setup] -> State explicitly that credentials and manual PATH edits are out of scope and report any follow-up action after removal.
- [Managed-file lists may drift if installers add wrappers later] -> Keep installer and uninstaller file ownership rules in one place in script comments/tests and extend smoke coverage when new managed files are introduced.
- [Shell differences can make PATH cleanup messaging noisy] -> Limit the script itself to neutral follow-up guidance rather than shell-specific destructive edits.

## Migration Plan

1. Add root-level uninstall scripts with directory resolution, bounded file cleanup, and idempotent success behavior.
2. Update installation documentation and test docs to publish supported uninstall commands and cleanup boundaries.
3. Extend release smoke tests to cover uninstall script presence, accepted arguments, and bounded cleanup expectations.

Rollback strategy:
- Remove the uninstall scripts and documentation updates, then fall back to manual file-deletion guidance until a corrected uninstall flow is ready.

## Open Questions

- None at proposal time.