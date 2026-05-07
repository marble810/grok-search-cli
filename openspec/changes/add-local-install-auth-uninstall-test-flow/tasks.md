## 1. Add local validation artifact packaging

- [x] 1.1 Add a supported repository-local way to build release-like archives and checksum files for a chosen version tag.
- [x] 1.2 Keep the local artifact names aligned with the existing release naming convention for each supported RID.

## 2. Extend installer scripts for local asset resolution

- [x] 2.1 Add a PowerShell installer option that resolves archive and checksum files from a caller-provided local asset directory instead of GitHub Releases.
- [x] 2.2 Add a Bash installer option that resolves archive and checksum files from a caller-provided local asset directory instead of GitHub Releases.
- [x] 2.3 Make both installers require an explicit version in local mode and fail clearly when the expected local archive or checksum file is missing.

## 3. Document the pure local lifecycle workflow

- [x] 3.1 Update the manual testing overview with local asset preparation guidance and when to use the local lifecycle path versus the published-release path.
- [x] 3.2 Update the installation and auth test guide to include pure local PowerShell and Bash install -> auth -> uninstall steps and cleanup expectations.

## 4. Verify local lifecycle coverage

- [x] 4.1 Extend script or smoke-test coverage to assert the new local asset override parameters are present and that lifecycle messaging remains intact.
- [x] 4.2 Add verification that local lifecycle packaging outputs the expected archive and checksum filenames for supported platforms.