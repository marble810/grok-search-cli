## Why

The CLI currently stays silent between sending a search request and writing the final JSON result, which makes long-running invocations look stalled to callers and local users. We need an explicit waiting-output layer so callers can distinguish "request in flight" from a hung process without breaking the final machine-readable result.

## What Changes

- Add a request lifecycle capability that emits an explicit waiting state after the CLI has accepted a valid search invocation and before the final model result is available.
- Define how waiting output is surfaced so it does not corrupt the existing final JSON success payload on stdout.
- Specify when the waiting state starts, when it stops, and how failures behave if the upstream request never reaches a completed result.
- Add tests for visible waiting behavior during delayed responses and for preserving the existing final output contract.

## Capabilities

### New Capabilities
- `request-wait-output`: Covers the intermediate waiting signal emitted between request dispatch and completed model output.

### Modified Capabilities
- `agent-json-output`: Clarifies that the final successful JSON result remains a single completed payload even when a waiting signal is emitted during execution.

## Impact

- Affects the runtime search flow in [src/grok-search-cli/Program.cs](src/grok-search-cli/Program.cs) and the xAI request path in [src/grok-search-cli/Clients/XaiClient.cs](src/grok-search-cli/Clients/XaiClient.cs).
- Affects stdout/stderr behavior and output-contract coverage in [tests/grok-search-cli.Tests/OutputContractTests.cs](tests/grok-search-cli.Tests/OutputContractTests.cs) and likely delayed-response integration coverage.
- No external dependency change is required; the main change is contract and runtime behavior around in-flight requests.