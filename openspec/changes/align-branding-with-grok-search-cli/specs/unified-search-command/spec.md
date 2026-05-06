## MODIFIED Requirements

### Requirement: Search command executes explicit search modes
The CLI SHALL expose grok-search-cli as the canonical executable name for the search command, SHALL require an explicit `--tool` value of `web`, `x`, or `both`, and SHALL call the xAI Responses API with model `grok-4-1-fast-reasoning`.

#### Scenario: Web search mode
- **WHEN** an agent invokes grok-search-cli with `--tool web` and a valid query
- **THEN** the CLI enables `web_search` in the Responses API request
- **THEN** the CLI does not enable `x_search` in that request

#### Scenario: X search mode
- **WHEN** an agent invokes grok-search-cli with `--tool x` and a valid query
- **THEN** the CLI enables `x_search` in the Responses API request
- **THEN** the CLI does not enable `web_search` in that request

#### Scenario: Both search mode
- **WHEN** an agent invokes grok-search-cli with `--tool both` and a valid query
- **THEN** the CLI sends one Responses API request that enables both `web_search` and `x_search`
- **THEN** the CLI returns one final answer for that invocation