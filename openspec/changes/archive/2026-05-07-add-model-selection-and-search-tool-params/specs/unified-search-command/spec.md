## MODIFIED Requirements

### Requirement: Search command executes explicit search modes
The CLI SHALL expose search as the canonical runtime action, SHALL require an explicit `--tool` value of `web`, `x`, or `both` for search invocations, SHALL accept an optional `--model` parameter for search invocations, SHALL default to model `grok-4.3` when no model is provided, and SHALL reserve explicit discovery entrypoints that do not trigger a search request.

#### Scenario: Web search mode with default model
- **WHEN** an agent invokes the search command with `--tool web`, omits `--model`, and provides a valid query
- **THEN** the CLI enables `web_search` in the Responses API request
- **THEN** the CLI does not enable `x_search` in that request
- **THEN** the CLI sends `grok-4.3` as the request model

#### Scenario: X search mode with explicit model
- **WHEN** an agent invokes the search command with `--tool x`, provides `--model <name>`, and provides a valid query
- **THEN** the CLI enables `x_search` in the Responses API request
- **THEN** the CLI does not enable `web_search` in that request
- **THEN** the CLI sends the provided model name in the request

#### Scenario: Both search mode
- **WHEN** an agent invokes the search command with `--tool both` and a valid query
- **THEN** the CLI sends one Responses API request that enables both `web_search` and `x_search`
- **THEN** the CLI returns one final answer for that invocation

#### Scenario: Discovery invocation
- **WHEN** a user invokes an explicit discovery entrypoint such as help or machine-readable usage export
- **THEN** the CLI does not require `--tool`
- **THEN** the CLI does not attempt a search request

### Requirement: Search command exposes stable filter flags
The CLI SHALL expose explicit flags for the supported search filters instead of accepting raw passthrough JSON, SHALL support the documented Web Search and X Search parameter set covered by this contract, and SHALL reject invalid flag combinations before sending the API request.

#### Scenario: Web search domain filters
- **WHEN** an agent invokes web search with allowed or excluded domains
- **THEN** the CLI maps those flags onto the corresponding `web_search` tool parameters

#### Scenario: Web search image understanding
- **WHEN** an agent invokes web search with image understanding enabled
- **THEN** the CLI maps that flag onto the corresponding `web_search` tool parameter

#### Scenario: X search account and date filters
- **WHEN** an agent invokes X search with allowed handles, excluded handles, or date range flags
- **THEN** the CLI maps those flags onto the corresponding `x_search` tool parameters

#### Scenario: X search media understanding
- **WHEN** an agent invokes X search with image understanding or video understanding enabled
- **THEN** the CLI maps those flags onto the corresponding `x_search` tool parameters

#### Scenario: Mutually exclusive inclusion and exclusion filters
- **WHEN** an agent provides both allowed and excluded filters for the same search tool in one invocation
- **THEN** the CLI exits with a configuration error before sending the API request

#### Scenario: Unsupported tool-specific flag combination
- **WHEN** an agent provides an X-only parameter while invoking `--tool web`, or a Web-only parameter that is outside the supported contract
- **THEN** the CLI exits with a configuration error instead of silently ignoring the flag

#### Scenario: Unsupported raw payload injection
- **WHEN** an agent attempts to provide arbitrary raw request JSON
- **THEN** the CLI rejects that input because it is outside the first-release contract