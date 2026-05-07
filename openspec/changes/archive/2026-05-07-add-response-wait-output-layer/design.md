## Context

The current search flow parses arguments, builds one xAI Responses API request, waits for the HTTP call to complete, and only then writes the final JSON payload to stdout. That behavior preserves a clean machine-readable contract, but it gives no indication that the process is alive once the request has been dispatched. Because this change touches the main entrypoint, the HTTP client path, and the output contract, it benefits from an explicit design before implementation.

## Goals / Non-Goals

**Goals:**
- Emit a visible waiting signal after a valid search request has been sent and before the final model output is available.
- Preserve the existing success contract of one final JSON document on stdout.
- Keep the waiting signal deterministic enough to test in delayed-response scenarios.
- Limit the change to search execution so discovery and auth commands remain unchanged.

**Non-Goals:**
- Adding token-by-token streaming output.
- Changing the JSON schema of the final successful result.
- Introducing an interactive spinner, terminal control sequences, or TTY-only UI.

## Decisions

### Emit waiting output on stderr, not stdout
The CLI already reserves stdout for the final JSON result and stderr for non-result diagnostics. The waiting layer should therefore be written to stderr so machine consumers can keep parsing stdout as a single JSON document.

Alternative considered: writing an interim JSON envelope to stdout before the final result. Rejected because it would break the current one-document success contract and force every caller to implement a streaming parser.

### Start waiting output only after request dispatch begins
The waiting signal should appear only after the CLI has validated inputs, resolved credentials, and started the outbound API call. This keeps local validation and configuration failures unchanged and ensures the waiting state means "upstream work is in flight," not merely "the process started."

Alternative considered: emit waiting output before validation completes. Rejected because it would create false positives for failures that occur before any network request is attempted.

### Use a simple, stable textual message for the first release
The implementation should emit one stable message such as "waiting for model response..." on stderr while the request is pending. A single deterministic line is easy to test, works in redirected output, and avoids terminal-specific behavior.

Alternative considered: animated spinners or repeated progress ticks. Rejected because they complicate tests, add noisy logs, and do not improve machine readability.

### Scope delayed-response testing around the HTTP client seam
Tests should cover the request lifecycle by simulating a delayed HTTP response and asserting that the waiting message is observable before the final JSON is written. This likely requires a small seam around the current HTTP call so tests can control completion timing without depending on the live xAI service.

Alternative considered: cover the behavior only with high-level smoke tests. Rejected because timing-sensitive behavior needs deterministic tests close to the HTTP boundary.

## Risks / Trade-offs

- More stderr output for successful runs → Keep the waiting message concise, emitted once, and documented as expected success-path behavior.
- Timing-sensitive tests can be flaky → Introduce a controlled delayed-response test seam rather than relying on real network latency.
- Some callers may currently treat any stderr output as failure → Document that stderr can contain non-fatal waiting diagnostics while exit code and stdout remain authoritative for success.
- The simple waiting line does not report granular progress → Accept this limitation for the first release to avoid accidentally designing a full streaming protocol.

## Migration Plan

No data migration is required. Rollout consists of updating the CLI runtime, adding contract tests, and documenting the new stderr behavior for successful delayed searches. If the change needs to be rolled back, reverting the waiting emission restores the prior silent-in-flight behavior without affecting persisted state.

## Open Questions

- Should the waiting message be emitted immediately on request dispatch or only after a short threshold to avoid noise on very fast responses?
- Do we want a future opt-out flag for silent operation, or should the waiting layer become the default runtime behavior for all search invocations?