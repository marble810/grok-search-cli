## 1. Add shared command-description metadata

- [ ] 1.1 Introduce a single in-process description model for supported commands, flags, query rules, credential prerequisites, examples, and output expectations.
- [ ] 1.2 Update command dispatch so discovery entrypoints can run without triggering search execution, network access, or credential resolution.

## 2. Implement CLI discovery outputs

- [ ] 2.1 Add human-readable help output for the root CLI surface and supported command groups.
- [ ] 2.2 Add a machine-readable discovery command that emits one stable JSON document describing the supported search and auth surfaces.
- [ ] 2.3 Keep discovery output behavior separate from search-result output so successful search runs remain JSON-only on stdout.

## 3. Align guidance and validation

- [ ] 3.1 Update the repository-local skill and checked-in guidance to defer to CLI self-description for canonical usage.
- [ ] 3.2 Add tests for discovery output shape, command coverage, and the unchanged search-output contract.