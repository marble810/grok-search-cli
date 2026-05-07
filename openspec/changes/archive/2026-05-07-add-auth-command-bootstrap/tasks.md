## 1. Add auth command infrastructure

- [x] 1.1 Add command dispatch for an `auth` command group while preserving the existing search invocation path.
- [x] 1.2 Add shared helpers for resolving the managed credential-store path and reading or writing its env-style payload safely.

## 2. Implement auth flows and precedence

- [x] 2.1 Implement `auth login` with a masked interactive prompt and a `--api-key-stdin` automation path.
- [x] 2.2 Implement `auth status` and `auth logout` without exposing secret values or mutating unrelated env or project `.env` sources.
- [x] 2.3 Update runtime credential resolution and missing-credential guidance to honor environment, upward `.env`, then the managed auth store.

## 3. Verify and document the auth flow

- [x] 3.1 Add tests for credential precedence, managed-store lifecycle, and non-interactive auth behavior.
- [x] 3.2 Add checked-in docs that connect the release/install flow to the new auth bootstrap path and explain manual overrides.