## Context

The current CLI hardcodes the request model to `grok-4-1-fast-reasoning` and only exposes a narrow subset of search-tool configuration. Official xAI tool docs now show `grok-4.3` in basic usage and document additional Web Search and X Search parameters, including image/video understanding toggles and tool-specific validation limits. Implementing this cleanly requires coordinated changes across argument parsing, request models, request shaping tests, and the final output contract.

## Goals / Non-Goals

**Goals:**
- Add an explicit CLI model-selection parameter with `grok-4.3` as the default effective model.
- Expand CLI flags to cover the documented Web Search and X Search parameters that fit the current first-party contract.
- Validate mutually exclusive or tool-specific combinations before dispatching the API request.
- Ensure the final JSON output reports the effective model used for the request.

**Non-Goals:**
- Supporting arbitrary passthrough JSON for future xAI parameters.
- Introducing environment-based model configuration in this change.
- Expanding beyond Web Search and X Search into other xAI tools.

## Decisions

### Use an explicit `--model` flag and keep the default in code
Model selection should be a first-class search flag, not an environment variable or hidden constant. The CLI should use `--model <name>` when provided and otherwise fall back to an updated default of `grok-4.3`.

Alternative considered: keep the model hardcoded and only update the constant. Rejected because the request is specifically to allow model selection at runtime.

### Expose only documented, stable tool parameters as dedicated flags
The CLI should continue the existing pattern of explicit flags rather than raw JSON passthrough. For Web Search, that means domain controls plus image understanding. For X Search, that means handle filters, date filters, image understanding, and video understanding.

Alternative considered: add a generic `--tool-param key=value` escape hatch. Rejected because it weakens validation and destabilizes the contract that OpenSpec currently protects.

### Validate incompatibilities and limits in CLI parsing
The CLI should reject `allowed` plus `excluded` variants for the same tool in a single invocation, enforce the documented list-size limits, and reject X-only parameters when `--tool web` is selected. This keeps failures local and makes request shaping deterministic.

Alternative considered: rely on xAI API validation. Rejected because it produces later, less actionable failures and complicates tests.

### Align request model property names with documented xAI tool fields
The request object should serialize using the official parameter names such as `allowed_x_handles`, `excluded_x_handles`, `enable_image_understanding`, and `enable_video_understanding`. This avoids a contract mismatch between the CLI and the upstream API surface.

Alternative considered: keep the current generalized field names and translate them elsewhere. Rejected because the existing request models are already the serialization boundary.

## Risks / Trade-offs

- More CLI flags increases parsing complexity → Keep the flag surface narrowly scoped to documented parameters and cover it with request-shaping tests.
- Default-model change can alter runtime behavior for existing users → Surface the effective model in output and document the new default in discovery/help text.
- Validation rules may reject combinations some users currently try → Prefer fast local failures over opaque upstream errors.
- Upstream docs may evolve again → Keep the implementation mapped to the currently documented stable parameters rather than speculative future fields.

## Migration Plan

Update the default model constant, add the new flags and request-model fields, extend tests, and refresh any help or docs that mention the old model or incomplete parameter set. Rollback is straightforward: restoring the old default and removing the new flags returns the CLI to its prior narrower contract.

## Open Questions

- Should `--model` be accepted only on search invocations, or should discovery output also surface the default and allowed examples?
- For `--tool both`, should shared flags such as image understanding apply to both tools, matching the upstream behavior noted in the Web Search docs?