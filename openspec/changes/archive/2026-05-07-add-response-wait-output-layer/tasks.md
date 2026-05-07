## 1. Runtime request lifecycle

- [x] 1.1 Add a search-lifecycle seam so the CLI can emit a waiting message after dispatching the xAI request and before the final response is available.
- [x] 1.2 Update the search execution path to write one deterministic waiting message to stderr without changing the final stdout JSON contract.
- [x] 1.3 Ensure discovery, help, and auth command paths do not emit the waiting message.

## 2. Automated verification

- [x] 2.1 Add or update tests that simulate a delayed upstream response and assert the waiting message appears before the final JSON result.
- [x] 2.2 Add or update output-contract tests to verify stdout still contains exactly one completed JSON document and stderr carries the waiting signal.
- [x] 2.3 Add failure-path coverage for an upstream request that fails after the waiting signal has already been emitted.

## 3. Contract and usage updates

- [x] 3.1 Update any user-facing usage or test documentation that currently implies successful searches are silent until completion.
- [x] 3.2 Run the focused grok-search-cli test suite for output and request lifecycle behavior and confirm the new contract passes end to end.