## ADDED Requirements

### Requirement: Search execution emits a waiting signal for in-flight model work
The CLI SHALL emit a visible waiting signal on stderr after a valid search request has been dispatched and before the final model result is available.

#### Scenario: Delayed upstream response
- **WHEN** a search invocation has started an upstream xAI request and the final model response is not yet complete
- **THEN** stderr contains a waiting message indicating that the CLI is still awaiting the model response
- **THEN** stdout does not contain any partial success payload during that waiting period

#### Scenario: Waiting signal is emitted once per search invocation
- **WHEN** one search invocation remains in-flight while awaiting the completed model response
- **THEN** the CLI emits at most one deterministic waiting message for that invocation
- **THEN** the CLI does not emit spinner frames or repeated progress ticks

#### Scenario: Non-search commands stay quiet
- **WHEN** a user runs a discovery or auth command that does not dispatch a search request
- **THEN** the CLI does not emit the waiting signal

### Requirement: Waiting signal covers both successful and failed in-flight completions
Once the waiting signal has been emitted for a search request, the CLI SHALL still complete with either the final successful result or the final failure diagnostics.

#### Scenario: Upstream request eventually succeeds
- **WHEN** the CLI emitted a waiting signal and the upstream request later completes successfully
- **THEN** the CLI writes the final JSON result after the waiting period
- **THEN** the process exits successfully

#### Scenario: Upstream request eventually fails
- **WHEN** the CLI emitted a waiting signal and the upstream request later fails
- **THEN** stderr includes the final failure summary after the waiting signal
- **THEN** the process exits with the same non-zero failure code used for upstream request failures