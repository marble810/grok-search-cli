# Release Process

grok-search-cli uses **GitHub Releases** as its only supported distribution
surface for precompiled binaries. Each tagged release publishes platform-specific
archives and checksums consumed by the [PowerShell](install.ps1) and
[Bash](install.sh) installers.

The supported lifecycle also includes the repo-root [PowerShell](uninstall.ps1)
and [Bash](uninstall.sh) uninstallers. Those uninstallers remove only the
installer-managed CLI files, leave credentials untouched, and may require manual
PATH cleanup if the install directory was added to a shell profile or User PATH.

## Creating a Release

1. Ensure the working tree is clean and the commit that will be tagged contains
   the intended version of the code.

2. Create and push a version tag:

   ```bash
   git tag v<semver>
   git push origin v<semver>
   ```

   The tag **must** start with `v` (e.g., `v1.0.0`, `v0.2.1`).

3. The [Release workflow](.github/workflows/release.yml) triggers on tags
   matching `v*`. It will:
   - Build the AOT-compiled binary for each supported platform.
   - Package each binary into a named archive.
   - Compute a SHA-256 checksum for each archive.
   - Attach all assets to the matching GitHub Release.

4. After the workflow completes, edit the release notes on GitHub if desired.

## Supported Platforms

| RID          | Archive Format | Build Runner      |
|--------------|----------------|-------------------|
| `win-x64`    | `.zip`         | `windows-latest`  |
| `linux-x64`  | `.tar.gz`      | `ubuntu-latest`   |
| `osx-arm64`  | `.tar.gz`      | `macos-latest`    |
| `osx-x64`    | `.tar.gz`      | `macos-latest`    |

## Asset Naming Convention

```
grok-search-cli_<version>_<rid>.{zip,tar.gz}
grok-search-cli_<version>_<rid>.sha256
checksums_<version>.txt
```

For example, tag `v1.0.0` produces:

```
grok-search-cli_v1.0.0_win-x64.zip
grok-search-cli_v1.0.0_win-x64.sha256
grok-search-cli_v1.0.0_linux-x64.tar.gz
grok-search-cli_v1.0.0_linux-x64.sha256
grok-search-cli_v1.0.0_osx-arm64.tar.gz
grok-search-cli_v1.0.0_osx-arm64.sha256
grok-search-cli_v1.0.0_osx-x64.tar.gz
grok-search-cli_v1.0.0_osx-x64.sha256
checksums_v1.0.0.txt
```

## Rollback

To roll back a release:

1. Delete or edit the GitHub Release to remove the assets.
2. Revert the workflow file if the pipeline itself has a defect.
3. Instruct users to reinstall a prior working version via the install scripts
   with the `--version` flag pointing to an earlier tag.

## Uninstall Boundary

Use the checked-in uninstall scripts when documenting or testing removal of a
user-scoped install. They intentionally do not reverse shell-profile edits,
delete environment variables, remove `.env` files, or clear auth-managed
credential stores outside the install directory.
