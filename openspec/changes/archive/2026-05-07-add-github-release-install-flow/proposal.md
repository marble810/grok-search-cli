## Why

The CLI can be built locally, but it still has no release or deployment path that a user or agent can consume directly. Before broader adoption, the project needs a single, deterministic way to publish signed release assets and install them without requiring a local .NET SDK or repository checkout.

## What Changes

- Add a GitHub Release-based distribution flow that builds precompiled CLI binaries as release assets.
- Add user-level installation and upgrade entrypoints for PowerShell and Bash so the CLI can be installed from release assets without administrator privileges.
- Add checked-in release and install guidance that treats GitHub Releases as the only supported deployment surface for the first iteration.
- Keep installation scripts secret-free and leave credential setup to a dedicated post-install auth flow rather than collecting API keys during installation.
- Keep package manager publishing, agent-private skill installation, and broader deployment targets out of scope for this change.

## Capabilities

### New Capabilities
- `github-release-distribution`: build and publish versioned release assets through GitHub Releases.
- `user-installation`: install and upgrade the released CLI from PowerShell or Bash into a user-scoped location.

### Modified Capabilities
<!-- None. -->

## Impact

- New CI/CD automation under `.github/workflows/`
- New install/upgrade scripts and any checked-in support files
- Release and installation documentation surfaced to humans and agents
- A documented handoff from installation into a future dedicated auth command flow
- Build and publish invocation for the CLI project under `src/`