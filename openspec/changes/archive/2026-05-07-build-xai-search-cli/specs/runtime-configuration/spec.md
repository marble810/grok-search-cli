## ADDED Requirements

### Requirement: API credentials resolve from explicit runtime sources
The CLI SHALL resolve xAI credentials from `XAI_API_KEY` in the process environment first, then from a local `.env` file if the environment variable is absent.

#### Scenario: Environment variable present
- **WHEN** `XAI_API_KEY` is present in the environment
- **THEN** the CLI uses that value for authentication
- **THEN** the CLI does not require `.env` for the invocation to succeed

#### Scenario: `.env` fallback
- **WHEN** `XAI_API_KEY` is absent from the environment and a local `.env` file defines it
- **THEN** the CLI loads the key from `.env`

#### Scenario: No credential source available
- **WHEN** neither the environment nor `.env` provides `XAI_API_KEY`
- **THEN** the CLI exits with a configuration error

### Requirement: Repository setup avoids committing local secrets
The repository SHALL ignore `.env` so local secrets are not added to version control by default.

#### Scenario: Local secret file present
- **WHEN** a developer creates a `.env` file in the repository root
- **THEN** git ignore rules exclude that file from normal source control tracking

### Requirement: Configuration behavior remains Native AOT-friendly
The first release SHALL keep runtime configuration limited to explicit environment and `.env` loading so the CLI remains compatible with Native AOT packaging.

#### Scenario: Native AOT publish path
- **WHEN** the CLI is published with Native AOT
- **THEN** configuration loading does not depend on dynamic configuration providers that are unnecessary for the agreed contract