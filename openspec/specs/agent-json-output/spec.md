## Purpose

Define the JSON stdout contract for successful search execution and the stderr/exit-code behavior for diagnostics.
## Requirements
### Requirement: Successful execution writes one JSON result to stdout
On successful search execution, the CLI SHALL write exactly one JSON document to stdout and SHALL keep stdout free of incidental logs.

#### Scenario: Successful web or X search
- **WHEN** the search request succeeds
- **THEN** stdout contains one valid JSON document
- **THEN** the JSON document includes the selected `tool`, the `model`, the final `answer`, and a `citations` array

#### Scenario: Upstream response includes an identifier
- **WHEN** xAI returns a response identifier for a successful search request
- **THEN** the CLI includes that identifier in the JSON result as response metadata

### Requirement: Diagnostics are separated from results
The CLI SHALL write warnings and errors to stderr and SHALL use exit codes to represent failure states.

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
The first release SHALL return a completed JSON result only after the search invocation finishes.

#### Scenario: Standard invocation completes
- **WHEN** an agent runs the search command without any future stream-specific option
- **THEN** the CLI waits for the full xAI response before writing the JSON result to stdout

