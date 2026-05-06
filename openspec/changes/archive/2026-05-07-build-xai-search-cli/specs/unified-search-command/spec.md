## ADDED Requirements

### Requirement: Search command executes explicit search modes
The CLI SHALL expose a single search command that requires an explicit `--tool` value of `web`, `x`, or `both`, and SHALL call the xAI Responses API with model `grok-4-1-fast-reasoning`.

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

### Requirement: Search command accepts exactly one query source
The CLI SHALL accept the search query from either a positional argument or stdin, and SHALL reject invocations that provide both or neither.

#### Scenario: Query from argument
- **WHEN** an agent provides a positional query and no stdin input
- **THEN** the CLI uses the positional query text for the request

#### Scenario: Query from stdin
- **WHEN** an agent pipes query text through stdin and omits the positional query
- **THEN** the CLI uses the stdin text for the request

#### Scenario: Ambiguous query input
- **WHEN** an agent provides both stdin input and a positional query
- **THEN** the CLI exits with a configuration error instead of guessing precedence

### Requirement: Search command exposes stable filter flags
The CLI SHALL expose explicit flags for the supported search filters instead of accepting raw passthrough JSON.

#### Scenario: Web search domain filters
- **WHEN** an agent invokes web search with allowed or excluded domains
- **THEN** the CLI maps those flags onto the corresponding `web_search` tool parameters

#### Scenario: X search account and date filters
- **WHEN** an agent invokes X search with allowed handles, excluded handles, or date range flags
- **THEN** the CLI maps those flags onto the corresponding `x_search` tool parameters

#### Scenario: Unsupported raw payload injection
- **WHEN** an agent attempts to provide arbitrary raw request JSON
- **THEN** the CLI rejects that input because it is outside the first-release contract