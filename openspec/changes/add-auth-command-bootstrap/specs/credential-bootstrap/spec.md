## ADDED Requirements

### Requirement: Auth command bootstraps managed user credentials
The CLI SHALL expose an `auth login` flow that can persist `XAI_API_KEY` into a managed user-scoped credential store without requiring manual file edits.

#### Scenario: Interactive login
- **WHEN** a user runs `auth login` in an interactive terminal
- **THEN** the CLI prompts for the API key without echoing the secret value
- **THEN** the CLI writes the managed credential store entry on success

#### Scenario: Non-interactive login from stdin
- **WHEN** a user runs `auth login --api-key-stdin`
- **THEN** the CLI reads the API key from stdin instead of prompting
- **THEN** the CLI writes the managed credential store entry on success

#### Scenario: Ambiguous or missing secret input
- **WHEN** `auth login` is invoked in a non-interactive context without `--api-key-stdin`, or the provided secret input is empty
- **THEN** the CLI exits with an input error instead of guessing how to continue

### Requirement: Auth command reports credential status safely
The CLI SHALL expose an `auth status` flow that reports whether credentials are configured and which source currently wins precedence without printing secret material.

#### Scenario: Higher-precedence source present
- **WHEN** `XAI_API_KEY` in the environment or a project `.env` would override the managed store
- **THEN** `auth status` reports that higher-precedence source as the effective credential source
- **THEN** it does not print the API key value

#### Scenario: Only managed store present
- **WHEN** only the managed user-scoped credential store contains `XAI_API_KEY`
- **THEN** `auth status` reports the CLI as configured through the managed store

#### Scenario: No credential source configured
- **WHEN** no supported source provides `XAI_API_KEY`
- **THEN** `auth status` reports the CLI as unconfigured

### Requirement: Auth command can remove managed credentials
The CLI SHALL expose an `auth logout` flow that removes only the managed user-scoped credential store and leaves environment or project `.env` sources untouched.

#### Scenario: Managed credential exists
- **WHEN** a user runs `auth logout` and the managed credential store exists
- **THEN** the CLI deletes or clears the managed credential store entry

#### Scenario: Higher-precedence credentials remain
- **WHEN** environment or project `.env` sources still define `XAI_API_KEY` after `auth logout`
- **THEN** subsequent status or search invocations continue to use those higher-precedence sources