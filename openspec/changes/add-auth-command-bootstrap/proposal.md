## Why

The release and install flow now keeps installers secret-free, but users and agents still lack a supported post-install way to configure credentials without manually editing shell state or repository files. A dedicated auth flow is needed so credential bootstrap becomes predictable, scoped, and easier to automate without pushing secret handling back into installers.

## What Changes

- Add a dedicated auth command surface that can bootstrap, inspect, and remove CLI credentials after installation.
- Add a managed user-scoped credential store that the CLI can read as a fallback without breaking existing `XAI_API_KEY` and project `.env` behavior.
- Add both interactive and non-interactive auth entrypoints so humans and agents can configure credentials without hand-editing files.
- Add checked-in guidance that separates installation from credential setup and points users to the auth flow as the preferred post-install path.
- Keep OS keychain integrations, package-manager-specific secret stores, and multi-profile account management out of scope for this first auth change.

## Capabilities

### New Capabilities
- `credential-bootstrap`: bootstrap, inspect, and remove CLI credentials through a dedicated auth command and managed user-scoped store.

### Modified Capabilities
- `runtime-configuration`: credential resolution gains a managed user-scoped fallback and missing-credential guidance that points to the auth flow.

## Impact

- CLI argument parsing and command dispatch under `src/xai-search-cli/`
- Credential loading logic under `src/xai-search-cli/Configuration/`
- User-facing auth guidance in release/install docs and future help surfaces
- Tests covering credential precedence, auth lifecycle, and managed-store behavior