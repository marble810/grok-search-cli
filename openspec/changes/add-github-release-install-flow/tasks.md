## 1. Build and publish release assets

- [ ] 1.1 Add a GitHub Actions workflow that builds the configured grok-search-cli release artifacts from supported version tags.
- [ ] 1.2 Package the release outputs using a stable asset naming convention and publish checksum data alongside the assets.

## 2. Add supported installation entrypoints

- [ ] 2.1 Add a PowerShell installer that downloads a requested grok-search-cli release version into a user-scoped Windows location.
- [ ] 2.2 Add a Bash installer that downloads a requested grok-search-cli release version into a user-scoped Unix-like location.
- [ ] 2.3 Make both installers support pinned-version installs and repeatable upgrades of an existing user-scoped install.
- [ ] 2.4 End both installers with guidance that hands credential setup off to a dedicated auth flow instead of collecting secrets during install.

## 3. Document and verify the flow

- [ ] 3.1 Add checked-in release and installation guidance that points users and agents to GitHub Releases as the supported distribution surface.
- [ ] 3.2 Add guidance for the post-install auth step and manual credential setup after installation.
- [ ] 3.3 Add smoke-test coverage for the release asset naming convention, installer download/upgrade flow, and secret-free installer behavior.