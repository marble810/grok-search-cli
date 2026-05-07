## Purpose

Define the repository-local agent skill contract used to validate grok-search-cli behavior and supported invocation patterns.
## Requirements
### Requirement: Repository includes a local skill for CLI testing
The repository SHALL include a Claude skill at `.claude/skills/grok-search-cli-test/SKILL.md` for testing and validating the grok-search-cli command.

#### Scenario: Skill discovery
- **WHEN** an agent enumerates repository-local skills
- **THEN** it can discover the grok-search-cli testing skill from its metadata

### Requirement: The skill documents the CLI contract for agents
The testing skill SHALL direct agents to the CLI's self-description for canonical command usage and SHALL keep repository-local guidance focused on repo-specific execution details.

#### Scenario: Contract discovery guidance
- **WHEN** an agent uses the skill to learn the CLI surface
- **THEN** the skill tells it to inspect the CLI's discovery entrypoint before composing commands

#### Scenario: Tool selection guidance
- **WHEN** an agent uses the skill to exercise the CLI search flow
- **THEN** the skill tells it to choose `--tool web|x|both` explicitly

#### Scenario: Output contract guidance
- **WHEN** an agent uses the skill to validate CLI behavior
- **THEN** the skill tells it to expect JSON-only stdout for search invocations and to use the discovery entrypoint for machine-readable usage data

#### Scenario: Input and credential guidance
- **WHEN** an agent uses the skill to prepare an invocation
- **THEN** the skill tells it to provide the query through either stdin or a positional argument, not both
- **THEN** the skill tells it to use `XAI_API_KEY`, `.env`, or the dedicated auth flow according to the supported credential contract

### Requirement: The skill preserves the requested implementation posture
The testing skill SHALL encode the requested implementation posture so agents keep follow-on changes aligned with the repository's intended style.

#### Scenario: Style guidance
- **WHEN** an agent uses the skill while testing or extending the CLI
- **THEN** the skill tells it to prefer let-it-crash behavior, minimal defensive code, concise control flow, and high-performance defaults
- **THEN** the skill tells it to avoid blanket retries or broad recovery logic unless the contract explicitly requires them

