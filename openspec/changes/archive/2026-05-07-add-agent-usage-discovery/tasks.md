## 1. Add shared command-description metadata

- [x] 1.1 Introduce a single in-process description model for supported commands, flags, query rules, credential prerequisites, examples, and output expectations.
- [x] 1.2 Update command dispatch so discovery entrypoints can run without triggering search execution, network access, or credential resolution.

## 2. Implement CLI discovery outputs

- [x] 2.1 Add human-readable help output for the root CLI surface and supported command groups.
- [x] 2.2 Add a machine-readable discovery command that emits one stable JSON document describing the supported search and auth surfaces.
- [x] 2.3 Keep discovery output behavior separate from search-result output so successful search runs remain JSON-only on stdout.

## 3. Align guidance and validation

- [x] 3.1 Update the repository-local skill and checked-in guidance to defer to CLI self-description for canonical usage.
- [x] 3.2 Add tests for discovery output shape, command coverage, and the unchanged search-output contract.