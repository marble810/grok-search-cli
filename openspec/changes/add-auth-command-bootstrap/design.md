## Context

The current CLI resolves credentials from `XAI_API_KEY` in the process environment first and then from a `.env` file found by walking upward from the current working directory. That is simple and AOT-friendly, but it leaves a post-install usability gap: a globally installed binary has no first-party way to bootstrap credentials unless the user edits shell config or creates ad hoc `.env` files by hand.

This design keeps the installer secret-free and moves secret handling into an explicit auth command. It also keeps compatibility with existing environment-variable and project-level `.env` workflows so current repository usage does not break.

## Goals / Non-Goals

**Goals:**
- Add a dedicated auth command group for login, status, and logout flows.
- Preserve existing credential precedence for `XAI_API_KEY` and upward `.env` discovery.
- Add a deterministic user-scoped credential fallback that works for an installed CLI outside a repository checkout.
- Support both interactive human setup and non-interactive agent automation without putting secrets on the process command line.
- Keep the implementation Native AOT-friendly and filesystem-based.

**Non-Goals:**
- Integrating with OS keychains or platform credential managers in this first version.
- Managing multiple named profiles or multiple API keys.
- Mutating shell startup files or silently exporting process-wide environment variables.
- Reworking release/install behavior beyond documenting the auth handoff.
- Designing the broader usage-discovery output surface in this change.

## Decisions

### Add an explicit `auth` command group
The CLI will grow an `auth` command group with at least `login`, `status`, and `logout` entrypoints, while the existing search invocation remains the default runtime action.

Rationale:
- It creates a single supported place for credential bootstrap.
- It keeps installer scope narrow.
- It gives later help and discovery work a stable command surface to describe.

Alternatives considered:
- Extending installers to collect secrets was rejected because installers are the wrong trust boundary.
- Requiring users to edit `.env` manually was rejected because it is cumbersome for installed usage.

### Store managed credentials in a user-scoped env-style file
The auth flow will persist only `XAI_API_KEY` in a deterministic user-scoped file such as `%APPDATA%/grok-search-cli/credentials.env` on Windows and `$XDG_CONFIG_HOME/grok-search-cli/credentials.env` or `~/.config/grok-search-cli/credentials.env` on Unix-like systems.

Rationale:
- It is cross-platform and easy to inspect, test, and migrate.
- An env-style file lets the implementation reuse the existing parsing model.
- It avoids adding heavier secure-store dependencies in the first iteration.

Alternatives considered:
- OS keychain integration was rejected for v1 because it expands scope and cross-platform complexity.
- Writing into shell profile files was rejected because it is invasive and shell-specific.

### Keep precedence as environment, then project `.env`, then managed auth store
Credential resolution will continue to prefer `XAI_API_KEY` from the process environment, then a `.env` discovered from the current directory upward, and only then the managed user-scoped auth store.

Rationale:
- It preserves the existing repository-local workflow.
- It lets project-specific credentials override the installed default when needed.
- It makes the managed store a safe fallback rather than a surprising override.

Alternatives considered:
- Putting the managed store ahead of project `.env` was rejected because it would change current project behavior.
- Replacing `.env` entirely was rejected because it would break an existing documented contract.

### Support interactive secret entry and stdin-based automation, but not argv secrets
`auth login` should support a masked interactive prompt when attached to a terminal and a `--api-key-stdin` mode for automation. It should not accept the raw API key as a positional argument or plain flag value.

Rationale:
- Interactive humans need a direct path.
- Agents and scripts need a non-interactive path.
- Avoiding argv secrets keeps them out of shell history and process listings.

Alternatives considered:
- A `--api-key <value>` flag was rejected because it leaks secrets more easily.

### Make status and logout source-aware but secret-safe
`auth status` will report whether credentials are configured and which source wins precedence, while `auth logout` will remove only the managed store. Neither flow should print secret values.

Rationale:
- Users need to understand precedence when env or `.env` overrides the managed store.
- Removing only managed credentials keeps the command from mutating unrelated sources.

Alternatives considered:
- Blindly deleting `.env` or environment sources was rejected because those may be owned by a project or shell configuration outside the CLI's control.

## Risks / Trade-offs

- [Managed credentials are still plaintext at rest] -> Keep the file user-scoped, apply restrictive permissions where the platform allows it, and document the trade-off explicitly.
- [Credential precedence confuses users] -> Make `auth status` report the effective source and document the precedence order.
- [Cross-platform config paths drift] -> Centralize path resolution and cover it with targeted tests.
- [Branding literals change underneath the auth docs] -> Reference the CLI generically in specs and align final command strings when the branding change lands.

## Migration Plan

1. Land the branding-alignment change or otherwise settle the final executable spelling before implementation-facing docs are frozen.
2. Add the auth command group and managed credential-store helpers.
3. Update runtime resolution to include the managed store as the lowest-priority fallback.
4. Add docs and release/install handoff text that points users from installation into `auth login`.
5. Follow with the usage-discovery change so agents can inspect the final auth surface programmatically.

Rollback strategy:
- Remove the auth command group and managed-store fallback, returning to the current environment-plus-`.env` contract.

## Open Questions

- Should the initial command spelling be `auth login`/`auth logout`, or would `auth set`/`auth clear` fit the final CLI voice better?
- Should a later change allow the auth command to write a project-local `.env` intentionally, or should managed user-scoped storage remain the only write target?