## 1. CLI argument surface

- [x] 1.1 Add `--model` parsing for search invocations and default the effective model to `grok-4.3` when the flag is omitted.
- [x] 1.2 Add dedicated flags for documented Web Search and X Search parameters, including image understanding and X video understanding.
- [x] 1.3 Add CLI validation for mutually exclusive include/exclude filters, tool-specific invalid flags, and documented list-size limits.

## 2. Request shaping and output contract

- [x] 2.1 Update the request model types and serialization names to match the documented xAI tool parameters.
- [x] 2.2 Update request construction so the effective model and expanded tool parameters are sent to the Responses API.
- [x] 2.3 Update successful output shaping so the JSON `model` field reflects the explicit or default effective model.

## 3. Verification and docs

- [x] 3.1 Extend request-shaping tests to cover the new flags, default model behavior, and validation failures.
- [x] 3.2 Extend output-contract tests to verify the reported `model` value for default and explicit model selections.
- [x] 3.3 Update help or user-facing docs that describe the old default model or incomplete search parameter support, then run the focused grok-search-cli test suite.