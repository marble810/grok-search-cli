## MODIFIED Requirements

### Requirement: Diagnostics are separated from results
The CLI SHALL write lifecycle, warning, and error diagnostics to stderr, SHALL keep stdout reserved for the final successful result document, and SHALL use exit codes to represent failure states.

#### Scenario: Waiting signal during a successful delayed search
- **WHEN** a search invocation is still awaiting the completed upstream model response
- **THEN** stdout remains empty until the final JSON result is ready
- **THEN** stderr may contain the non-fatal waiting signal for that in-flight request

#### Scenario: Missing configuration
- **WHEN** the CLI cannot resolve a required API key
- **THEN** stdout remains empty
- **THEN** stderr contains the configuration error
- **THEN** the process exits with a non-zero exit code

#### Scenario: Upstream request failure
- **WHEN** the Responses API returns an error or an unusable payload
- **THEN** stdout remains empty
- **THEN** stderr contains the failure summary
- **THEN** the process exits with a non-zero exit code

### Requirement: The default result mode is non-streaming
The first release SHALL return a completed JSON result only after the search invocation finishes, even if the CLI emits an intermediate waiting signal on stderr while the request is in flight.

#### Scenario: Standard invocation completes after waiting
- **WHEN** an agent runs the search command without any future stream-specific option and the upstream response is delayed
- **THEN** the CLI may emit a waiting signal on stderr while the request is pending
- **THEN** the CLI writes exactly one completed JSON result to stdout after the upstream response finishes

#### Scenario: Standard invocation completes without delay
- **WHEN** an agent runs the search command without any future stream-specific option and the upstream response completes quickly
- **THEN** the CLI still writes exactly one completed JSON result to stdout
- **THEN** the CLI does not switch to token-by-token or partial-result streaming