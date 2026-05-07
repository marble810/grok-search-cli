## Context

The repository currently ships only source code and OpenSpec artifacts. There is no CI workflow, no versioned release asset layout, and no supported install path for users or agents outside the repo. That is a blocker for the next stage of adoption because the intended callers should be able to fetch a published binary and install it without cloning the repository or setting up a .NET SDK.

This design follows the decisions already discussed: GitHub Releases will be the only supported distribution surface for the first iteration; release artifacts will be precompiled binaries rather than source-only packages; installation will default to user-scoped locations without administrator privileges; and repository-local skills remain a repo concern rather than a globally installed agent integration mechanism.

## Goals / Non-Goals

**Goals:**
- Produce deterministic, versioned release assets from GitHub Actions when a release tag is created.
- Publish install entrypoints that work from PowerShell and Bash against GitHub Release assets.
- Keep installation user-scoped and upgrade-friendly so humans and agents can self-install safely.
- Keep installation scripts free of secret collection and hand credential setup off to a dedicated auth command.
- Document the supported release and install path in a way that later CLI self-description can reference without redefining the pipeline.

**Non-Goals:**
- Publishing to package managers such as winget, Homebrew, apt, or npm.
- Installing or registering agent-private skills outside the repository.
- Adding auto-update daemons, background services, or system-wide installers.
- Expanding CLI behavior beyond what is needed to package and distribute the existing executable.
- Defining or implementing the dedicated auth command itself in this change.

## Decisions

### Use GitHub Releases as the only first-party distribution surface
Version tags will trigger a GitHub Actions workflow that builds release assets and attaches them to a GitHub Release.

Rationale:
- It gives one stable URL pattern for humans, scripts, and agents.
- It avoids splitting trust and versioning across multiple registries in v1.
- It keeps the delivery mechanism close to the source repository.

Alternatives considered:
- Publishing to package managers in the first change was rejected because it multiplies packaging and support work.
- Requiring source checkout plus local `dotnet publish` was rejected because it is too heavy for agent and user installation.

### Publish precompiled, versioned archives plus checksums
Each supported target runtime will produce a named archive asset and a checksum manifest so install scripts can download known release artifacts and validate them.

Rationale:
- Precompiled artifacts avoid forcing local SDK setup.
- Archive assets travel better through GitHub Releases than loose binary collections.
- Checksums provide a simple integrity boundary for scripted installs.

Alternatives considered:
- Loose binaries without checksums were rejected because install scripts would have no stable integrity check.

### Default to user-scoped installation locations
The install scripts will place the binary in a user-owned location and update user-level PATH guidance rather than requiring administrator rights.

Rationale:
- It matches the agreed deployment posture.
- It is safer for both humans and agents.
- It reduces failure cases related to elevated permissions.

Alternatives considered:
- System-wide installation was rejected because it increases permissions and rollback complexity.

### Keep installers secret-free and delegate credentials to a dedicated auth command
The PowerShell and Bash installers should not collect or persist `XAI_API_KEY`. Instead, they should end with clear guidance that credential bootstrap belongs to a dedicated post-install auth flow, for example a future `groksearch auth` command.

Rationale:
- It avoids handling secrets inside installation scripts.
- It keeps the trust boundary narrow: install scripts manage files, auth flows manage credentials.
- It avoids mixing human-interactive secret entry with release automation surfaces.

Alternatives considered:
- Prompting for `XAI_API_KEY` during install was rejected because installers are the wrong place to collect secrets.
- Writing credentials from the installer into user profiles or `.env` files was rejected because it broadens risk and complicates review.

### Treat PowerShell and Bash as the supported install entrypoints
PowerShell will cover Windows user installs, while Bash will cover Unix-like environments where Bash is available.

Rationale:
- These entrypoints match the intended operator environments.
- They are easy for both humans and automation to invoke from a one-liner.

Alternatives considered:
- A single language-specific installer was rejected because it would either underserve Windows or require additional runtime assumptions.

### Keep agent usage discovery out of the release pipeline change
This change will document release and install steps, but agent-facing self-description and exported skill templates remain separate work.

Rationale:
- It keeps the first delivery pipeline tractable.
- It avoids coupling the release flow to any one agent platform's private install mechanism.

Alternatives considered:
- Embedding agent skill installation into the installer was rejected because it expands scope and creates per-agent coupling.

## Risks / Trade-offs

- [GitHub Actions matrix becomes brittle] -> Start with a small supported runtime set and validate publish commands in CI before widening coverage.
- [Install scripts drift from asset naming] -> Derive download URLs from a single documented asset naming convention and verify them in CI smoke tests.
- [User PATH updates differ across shells and OSes] -> Keep installation location deterministic and document the PATH rule explicitly; only automate the user-level path updates that are safe.
- [Users expect install to finish fully configured] -> End the installer with a clear next step that points to the dedicated auth flow or manual credential setup.
- [Release-only distribution limits discoverability] -> Accept this for v1 and layer package manager publishing in a later change if needed.

## Migration Plan

1. Add GitHub Actions workflow and release asset naming convention.
2. Add PowerShell and Bash installers that consume those assets without collecting credentials.
3. Add release/install documentation and smoke-test the scripts against produced assets and the post-install auth handoff.
4. Cut the first tagged release using the new flow.

Rollback strategy:
- Disable or revert the workflow and installer scripts, then continue distributing from source-only instructions until a corrected flow is ready.

## Open Questions

- Which initial runtime matrix should be guaranteed in v1: Windows x64 only, or Windows plus a Unix target from day one?
- Should install scripts update the user PATH automatically where possible, or print the required export commands and leave PATH mutation manual?
- Should the future auth command be named `groksearch auth` exactly, or align with the final branded executable spelling decided in the branding change?