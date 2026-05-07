## MODIFIED Requirements

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