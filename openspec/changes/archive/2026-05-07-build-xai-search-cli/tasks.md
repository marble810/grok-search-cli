## 1. Bootstrap the Native AOT CLI

- [x] 1.1 Create the .NET CLI project structure and Native AOT publish settings for the current LTS SDK.
- [x] 1.2 Add the minimal xAI request and response DTOs plus `System.Text.Json` configuration that stays AOT-friendly.
- [x] 1.3 Add local runtime setup files and verify `.env` is ignored while `.env.example` can remain trackable.

## 2. Implement runtime configuration and API transport

- [x] 2.1 Implement fail-fast credential resolution with `XAI_API_KEY` first and `.env` as the fallback.
- [x] 2.2 Implement the xAI Responses API client with `HttpClient` and fixed `grok-4-1-fast-reasoning` model selection.
- [x] 2.3 Map the agreed CLI flags onto `web_search` and `x_search` tool parameters, including the single-request `both` mode.

## 3. Implement the CLI contract

- [x] 3.1 Implement the search command with explicit `--tool web|x|both` selection and query input from either argv or stdin.
- [x] 3.2 Implement the JSON stdout contract with final answer, citations, and basic metadata plus stderr-only diagnostics and non-zero failures.
- [x] 3.3 Keep the command path concise and high-performance by validating only public contract boundaries and avoiding blanket retries or broad defensive wrappers.

## 4. Verify behavior and agent integration

- [x] 4.1 Add tests for request shaping across `web`, `x`, and `both`, including query-source conflict handling.
- [x] 4.2 Add tests for stdout/stderr separation, missing-key failures, and upstream error propagation.
- [x] 4.3 Update the local `.claude/skills/xai-search-cli-test` skill so its invocation guidance matches the implemented executable and flags.
- [x] 4.4 Publish a Native AOT build and run smoke tests that exercise the CLI the way an agent would call it.