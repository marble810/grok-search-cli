## Requirements

### Requirement: Repository includes a local skill for CLI testing
The repository SHALL include a Claude skill at `.claude/skills/xai-search-cli-test/SKILL.md` for testing and validating the xAI search CLI.

#### Scenario: Skill discovery
- **WHEN** an agent enumerates repository-local skills
- **THEN** it can discover the xAI search CLI testing skill from its metadata

### Requirement: The skill documents the CLI contract for agents
The testing skill SHALL instruct agents to use the planned CLI contract rather than inventing alternate invocation patterns.

#### Scenario: Tool selection guidance
- **WHEN** an agent uses the skill to exercise the CLI
- **THEN** the skill tells it to choose `--tool web|x|both` explicitly

#### Scenario: Output contract guidance
- **WHEN** an agent uses the skill to validate CLI behavior
- **THEN** the skill tells it to expect JSON-only stdout, diagnostic stderr, and meaningful exit codes

#### Scenario: Input and credential guidance
- **WHEN** an agent uses the skill to prepare an invocation
- **THEN** the skill tells it to provide the query through either stdin or a positional argument, not both
- **THEN** the skill tells it to use `XAI_API_KEY` or `.env` for credentials

### Requirement: The skill preserves the requested implementation posture
The testing skill SHALL encode the requested implementation posture so agents keep follow-on changes aligned with the repository's intended style.

#### Scenario: Style guidance
- **WHEN** an agent uses the skill while testing or extending the CLI
- **THEN** the skill tells it to prefer let-it-crash behavior, minimal defensive code, concise control flow, and high-performance defaults
- **THEN** the skill tells it to avoid blanket retries or broad recovery logic unless the contract explicitly requires them