## 1. Align public command identity

- [ ] 1.1 Rename the CLI's visible build metadata so the produced executable and project-facing labels use grok-search-cli instead of xai-search.
- [ ] 1.2 Update solution and test project references so checked-in developer tooling surfaces the same grok-search-cli identity.

## 2. Align agent guidance and checked-in references

- [ ] 2.1 Rename the repository-local testing skill path and metadata to grok-search-cli-test.
- [ ] 2.2 Update checked-in invocation examples and guidance so supported command usage refers to grok-search-cli consistently.

## 3. Validate the rename surface

- [ ] 3.1 Run focused tests/build validation after the rename and confirm the CLI still executes successfully.
- [ ] 3.2 Run a targeted repository search for the old xai-search command name in tracked source and guidance, then resolve or document any remaining intentional references.