## 1. Add supported uninstall scripts

- [x] 1.1 Add `uninstall.ps1` with the same default and caller-specified install-directory contract as `install.ps1`.
- [x] 1.2 Add `uninstall.sh` with the same default and caller-specified install-directory contract as `install.sh`.
- [x] 1.3 Make both uninstall scripts remove only installer-managed CLI files, prune the install directory only when it is empty, and succeed cleanly when managed files are already absent.

## 2. Document the install-to-uninstall lifecycle

- [x] 2.1 Update `INSTALL.md` with supported uninstall commands for PowerShell and Bash, including custom-directory usage.
- [x] 2.2 Update release and test guidance to describe the uninstall cleanup boundary for PATH follow-up and credential preservation.

## 3. Verify uninstall behavior

- [x] 3.1 Extend release smoke coverage to verify uninstall script presence and lifecycle argument support.
- [x] 3.2 Add smoke-test assertions that uninstall stays idempotent, preserves credentials outside the install directory, and reports any manual PATH cleanup follow-up.