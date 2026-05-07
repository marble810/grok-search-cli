## MODIFIED Requirements

### Requirement: Successful execution writes one JSON result to stdout
On successful search execution, the CLI SHALL write exactly one JSON document to stdout, SHALL keep stdout free of incidental logs, and SHALL report the effective request model in the result payload.

#### Scenario: Successful web or X search with default model
- **WHEN** the search request succeeds and the agent did not pass an explicit model
- **THEN** stdout contains one valid JSON document
- **THEN** the JSON document includes the selected `tool`, the effective default `model`, the final `answer`, and a `citations` array

#### Scenario: Successful search with explicit model
- **WHEN** the search request succeeds and the agent provided `--model <name>`
- **THEN** stdout contains one valid JSON document
- **THEN** the JSON document reports that explicit model value

#### Scenario: Upstream response includes an identifier
- **WHEN** xAI returns a response identifier for a successful search request
- **THEN** the CLI includes that identifier in the JSON result as response metadata