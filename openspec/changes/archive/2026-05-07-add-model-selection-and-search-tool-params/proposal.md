## Why

The CLI currently hardcodes a retired-looking default model and only exposes a subset of the official Web Search and X Search tool parameters. We need to let callers choose the model explicitly, default to `grok-4.3` when no model is provided, and close the gap between the CLI contract and the current xAI search-tool surface.

## What Changes

- Add a search invocation parameter for model selection, with `grok-4.3` as the default when callers do not pass a model.
- Expand the supported search flags so Web Search and X Search can map the documented tool parameters instead of only domain, handle, and date filters.
- Define validation rules for mutually exclusive or tool-specific parameters so invalid combinations fail before the API request is sent.
- Update result-contract expectations so the reported `model` field reflects the selected or default runtime model.

## Capabilities

### New Capabilities

### Modified Capabilities
- `unified-search-command`: Add explicit model selection, default model behavior, and broader Web Search/X Search parameter coverage.
- `agent-json-output`: Ensure the final JSON output reports the effective model that was used for the request.

## Impact

- Affects CLI argument parsing and request shaping in [src/grok-search-cli/CliLogic.cs](src/grok-search-cli/CliLogic.cs) and [src/grok-search-cli/Program.cs](src/grok-search-cli/Program.cs).
- Affects request payload construction in [src/grok-search-cli/Models/XaiRequest.cs](src/grok-search-cli/Models/XaiRequest.cs) and the related tool model types.
- Affects output-contract and request-shaping coverage in [tests/grok-search-cli.Tests/OutputContractTests.cs](tests/grok-search-cli.Tests/OutputContractTests.cs) and [tests/grok-search-cli.Tests/RequestShapingTests.cs](tests/grok-search-cli.Tests/RequestShapingTests.cs).