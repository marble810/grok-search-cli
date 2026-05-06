## MODIFIED Requirements

### Requirement: Successful execution writes one JSON result to stdout
On successful search execution, the CLI SHALL write exactly one JSON document to stdout and SHALL keep stdout free of incidental logs.

#### Scenario: Successful web or X search
- **WHEN** the search request succeeds
- **THEN** stdout contains one valid JSON document
- **THEN** the JSON document includes the selected `tool`, the `model`, the final `answer`, and a `citations` array

#### Scenario: Upstream response includes an identifier
- **WHEN** xAI returns a response identifier for a successful search request
- **THEN** the CLI includes that identifier in the JSON result as response metadata