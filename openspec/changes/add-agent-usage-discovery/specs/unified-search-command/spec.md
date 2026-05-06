## MODIFIED Requirements

### Requirement: Search command executes explicit search modes
The CLI SHALL expose search as the canonical runtime action, SHALL require an explicit `--tool` value of `web`, `x`, or `both` for search invocations, SHALL call the xAI Responses API with model `grok-4-1-fast-reasoning`, and SHALL reserve explicit discovery entrypoints that do not trigger a search request.

#### Scenario: Web search mode
- **WHEN** an agent invokes the search command with `--tool web` and a valid query
- **THEN** the CLI enables `web_search` in the Responses API request
- **THEN** the CLI does not enable `x_search` in that request

#### Scenario: X search mode
- **WHEN** an agent invokes the search command with `--tool x` and a valid query
- **THEN** the CLI enables `x_search` in the Responses API request
- **THEN** the CLI does not enable `web_search` in that request

#### Scenario: Both search mode
- **WHEN** an agent invokes the search command with `--tool both` and a valid query
- **THEN** the CLI sends one Responses API request that enables both `web_search` and `x_search`
- **THEN** the CLI returns one final answer for that invocation

#### Scenario: Discovery invocation
- **WHEN** a user invokes an explicit discovery entrypoint such as help or machine-readable usage export
- **THEN** the CLI does not require `--tool`
- **THEN** the CLI does not attempt a search request

### Requirement: Search command accepts exactly one query source
For search invocations, the CLI SHALL accept the search query from either a positional argument or stdin, and SHALL reject invocations that provide both or neither.

#### Scenario: Query from argument
- **WHEN** an agent provides a positional query and no stdin input for a search invocation
- **THEN** the CLI uses the positional query text for the request

#### Scenario: Query from stdin
- **WHEN** an agent pipes query text through stdin and omits the positional query for a search invocation
- **THEN** the CLI uses the stdin text for the request

#### Scenario: Ambiguous query input
- **WHEN** an agent provides both stdin input and a positional query for a search invocation
- **THEN** the CLI exits with a configuration error instead of guessing precedence

#### Scenario: Discovery invocation needs no query
- **WHEN** a user invokes a discovery entrypoint instead of a search invocation
- **THEN** the CLI does not require a positional query or stdin input