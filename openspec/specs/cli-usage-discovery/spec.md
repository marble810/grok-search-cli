# cli-usage-discovery Specification

## Purpose
TBD - created by archiving change add-agent-usage-discovery. Update Purpose after archive.
## Requirements
### Requirement: CLI exposes human-readable help
The CLI SHALL expose a human-readable help entrypoint that describes the supported command surface without requiring credentials or network access.

#### Scenario: Root help
- **WHEN** a user invokes the CLI help entrypoint for the root surface
- **THEN** the CLI prints the supported command groups, the search invocation pattern, and where to look for more detailed usage

#### Scenario: Command-specific help
- **WHEN** a user invokes help for a supported command group such as search or auth
- **THEN** the CLI prints the relevant flags, argument rules, and examples for that command group

### Requirement: CLI exposes machine-readable usage discovery
The CLI SHALL expose a machine-readable discovery entrypoint that writes one JSON document describing the supported command surface for agents.

#### Scenario: Discovery output includes supported command groups
- **WHEN** an agent invokes the machine-readable discovery entrypoint
- **THEN** stdout contains one valid JSON document
- **THEN** the document includes the supported search and auth command groups, their flags, and their argument rules

#### Scenario: Discovery output includes execution guidance
- **WHEN** an agent invokes the machine-readable discovery entrypoint
- **THEN** the document includes credential prerequisites, output-mode notes, and example invocations for supported commands

### Requirement: Discovery does not require runtime credentials
Usage discovery SHALL work without resolving `XAI_API_KEY` or contacting the xAI API.

#### Scenario: Discovery without configured credentials
- **WHEN** an agent invokes a discovery entrypoint on a machine with no configured credentials
- **THEN** the CLI still returns the requested help or discovery output
- **THEN** it does not attempt a search request

