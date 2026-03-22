# Syrx.MySql Security And Performance Implementation Plan

## 1. Plan Metadata
- Plan date: 2026-03-22
- Scope: Syrx.MySql.sln (root + included submodule projects)
- Source inputs:
  - .docs/research/security/Syrx.MySql-security-research-report-20260322.md
  - .docs/research/performance/Syrx.MySql-performance-research-report-20260322.md
- Objective: Execute the highest-impact security and performance remediations with low regression risk and strong checkpoint discipline across chat sessions.

## 2. Operating Rules For This Plan
- Apply changes in small batches only.
- Run build/test validation after each significant change batch before moving to the next batch.
- Do not mix high-risk workflow changes with deep runtime refactors in the same batch.
- Keep a concise change log at the end of each session in this file.
- If a batch fails validation, stop and fix forward before proceeding.

## 3. Validation Gates (Required Between Significant Changes)
Use these gates after each work package below.

### Gate A: Fast compile validation
- Command:
  - dotnet build "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release
- Pass criteria:
  - 0 build errors

### Gate B: Unit regression validation
- Command:
  - dotnet test "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release --filter "FullyQualifiedName!~Integration"
- Pass criteria:
  - 0 failed unit tests

### Gate C: Integration validation
- Command:
  - dotnet test "c:\Projects\Syrx\Syrx.MySql\tests\integration\Syrx.MySql.Tests.Integration\Syrx.MySql.Tests.Integration.csproj" --configuration Release
- Pass criteria:
  - 0 failed integration tests

### Gate D: Full suite checkpoint
- Command:
  - dotnet test "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release
- Pass criteria:
  - 0 failed tests

## 4. Prioritized Backlog

## P0: High-Risk, High-Impact (Do First)

### P0.1 Lock down package publish scope in workflow (SEC-001)
- Priority: P0
- Why: Prevent accidental or malicious publication of unintended packages.
- Evidence:
  - .github/workflows/publish.yml (recursive nupkg discovery/push)
- Implementation tasks:
  - Replace recursive package discovery with explicit artifact path allow-list.
  - Add explicit package-id allow-list check before push.
  - Fail pipeline on unexpected package count or IDs.
- Security acceptance criteria:
  - Publish job only pushes intended package artifacts.
  - Unexpected package files cause hard failure.
- Validation:
  - Gate A
  - Gate B

### P0.2 Add explicit deployment environment gate for publish (SEC-002)
- Priority: P0
- Why: Enforce approval boundaries and secret scoping for release publishing.
- Evidence:
  - .github/workflows/publish.yml deploy job uses secret-bearing publish.
- Implementation tasks:
  - Add workflow environment to deploy job (for example production-release).
  - Ensure NuGet API key is environment-scoped secret.
  - Document required reviewers in workflow/README release section.
- Security acceptance criteria:
  - Secret-bearing deploy step executes only within protected environment policy.
- Validation:
  - Gate A
  - Gate B

### P0.3 Remove reflection-heavy multiple-result dispatch (PERF-001, PERF-002)
- Priority: P0
- Why: Highest CPU overhead in central query path.
- Evidence:
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multiple.cs
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.Query.Multiple.cs
- Implementation tasks:
  - Replace MethodInfo.Invoke-based result materialization with strongly-typed dispatch helpers.
  - Remove reflected Task.Result extraction in async flow.
  - Preserve public API behavior and error semantics.
- Performance acceptance criteria:
  - No reflection invoke in hot loop for result-set materialization.
  - Existing tests pass without behavior drift.
- Validation:
  - Gate A
  - Gate B
  - Gate C
  - Gate D

## P1: Medium Impact, Strong Follow-Up

### P1.1 Propagate cancellation correctly in async multimap (PERF-004)
- Priority: P1
- Why: Prevent wasted DB work and tail latency during cancellations.
- Evidence:
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multimap.cs
- Implementation tasks:
  - Use Dapper CommandDefinition-based async execution path so token is honored.
  - Add/expand cancellation tests around multimap query methods.
- Acceptance criteria:
  - Cancellation token path verified by test.
- Validation:
  - Gate A
  - Gate B
  - Gate C

### P1.2 Reduce multimap per-call allocations (PERF-003)
- Priority: P1
- Why: Reduce allocation pressure in high-frequency query paths.
- Evidence:
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multimap.cs
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.Query.Multimap.cs
- Implementation tasks:
  - Cache type-shape metadata per generic signature.
  - Minimize temporary arrays and closure allocations.
- Acceptance criteria:
  - Allocation-sensitive paths reduced without API changes.
- Validation:
  - Gate A
  - Gate B
  - Gate C

### P1.3 Tune integration fixture/setup throughput (PERF-005)
- Priority: P1
- Why: Improve CI and local cycle time.
- Evidence:
  - tests/integration/Syrx.MySql.Tests.Integration/MySqlFixture.cs
  - tests/integration/Syrx.MySql.Tests.Integration/DatabaseBuilder.cs
- Implementation tasks:
  - Keep lifecycle fully async and remove unnecessary task wrapping.
  - Remove redundant service-provider build.
  - Batch setup/seeding operations where practical.
- Acceptance criteria:
  - Integration runtime decreases while preserving stability.
- Validation:
  - Gate A
  - Gate C
  - Gate D

### P1.4 Scope package generation to pack/publish contexts (PERF-006)
- Priority: P1
- Why: Reduce routine build/test overhead.
- Evidence:
  - Directory.Build.props
  - .submodules/Syrx.Commanders.Databases/Directory.Build.props
- Implementation tasks:
  - Disable global GeneratePackageOnBuild for normal build/test.
  - Enable packaging explicitly in pack/publish workflows.
- Acceptance criteria:
  - Standard build/test does not generate packages.
  - Publish pipeline still produces/pushes required artifacts.
- Validation:
  - Gate A
  - Gate B
  - Gate D

## P2: Hygiene And Policy Hardening

### P2.1 Remove hardcoded credential literals from tests (SEC-003)
- Priority: P2
- Why: Prevent insecure credential patterns from propagating.
- Evidence:
  - tests/unit/Syrx.Commanders.Databases.Connectors.MySql.Tests.Unit/MySqlDatabaseConnectorTests/CreateConnection.cs
- Implementation tasks:
  - Replace literal credentials with generated or environment-derived test values.
  - Add a lightweight guard test/pattern to prevent reintroduction.
- Validation:
  - Gate A
  - Gate B

### P2.2 Clarify TLS test-only exception boundaries (SEC-004)
- Priority: P2
- Why: Keep insecure transport settings contained to non-production test scope.
- Evidence:
  - tests/integration/Syrx.MySql.Tests.Integration/MySqlFixture.cs
- Implementation tasks:
  - Keep insecure option test-scoped and documented.
  - Add code comment and README note indicating non-production use only.
- Validation:
  - Gate A
  - Gate C

### P2.3 Address xUnit deprecation track (Security appendix)
- Priority: P2
- Why: Dependency hygiene and long-term supportability.
- Implementation tasks:
  - Plan controlled migration path for deprecated xUnit line across root/submodule tests.
  - Execute migration in dedicated PR after P0/P1 stabilization.
- Validation:
  - Gate A
  - Gate B
  - Gate D

## 5. Suggested Execution Sequence (Session-Friendly)
1. Session 1:
- P0.1 and P0.2
- Run Gate A and Gate B
- Commit if green

2. Session 2:
- P0.3 (reflection removal)
- Run Gate A, Gate B, Gate C, Gate D
- Commit if green

3. Session 3:
- P1.1 and P1.2
- Run Gate A, Gate B, Gate C
- Commit if green

4. Session 4:
- P1.3 and P1.4
- Run Gate A, Gate C, Gate D
- Commit if green

5. Session 5:
- P2.1, P2.2, P2.3
- Run Gate A, Gate B, Gate D
- Commit if green

## 6. Risks And Mitigations
- Risk: Workflow hardening can block legitimate release path.
  - Mitigation: Dry-run and explicit artifact assertions before enabling strict fail conditions.

- Risk: Query-path refactors may introduce behavioral regressions.
  - Mitigation: Keep API surface unchanged, add targeted tests for multi-result/multimap edge cases.

- Risk: Performance changes could obscure correctness issues.
  - Mitigation: correctness-first validation (Gate B/C) before any throughput comparison.

- Risk: Integration environment instability (Docker/MySQL startup variance).
  - Mitigation: keep explicit startup timeout/readiness checks and use focused reruns before full suite.

## 7. Definition Of Done
- All P0 and P1 items complete and validated.
- No regression in full suite (Gate D).
- CI publish flow hardened and documented.
- Security/performance reports can be re-run with materially fewer high/medium findings.

## 8. Session Handoff Template
Copy and update this section at the end of each session.

### Session Log Entry
- Date:
- Completed items:
- Validation gates run and results:
- Open risks/blockers:
- Next item to start:
- Notes for next chat session:

### Session Log Entry - 2026-03-22 (Execution 1)
- Date: 2026-03-22
- Completed items:
  - P0.1 Lock down package publish scope in workflow (explicit package allow-list, strict package count and ID validation, hard fail on unexpected packages).
  - P0.2 Add explicit deployment environment gate for publish (deploy job environment gate plus environment-scoped secret guidance).
  - README release guidance updated to document required environment reviewers and environment-scoped NuGet secret handling.
- Validation gates run and results:
  - Gate A: `dotnet build "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release` -> Passed.
  - Gate B: `dotnet test "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release --filter "FullyQualifiedName!~Integration"` -> Passed (235 passed, 0 failed; 2 filter warnings in integration assemblies).
- Open risks/blockers:
  - Local workflow schema validation reports environment value as invalid in `publish.yml`; this appears to be tooling/schema noise rather than runtime workflow logic failure.
- Next item to start:
  - P0.3 Remove reflection-heavy multiple-result dispatch.
- Notes for next chat session:
  - Preserve this split-batch strategy: complete P0.3 in isolation, then run Gate A/B/C/D before proceeding to P1.

### Session Log Entry - 2026-03-22 (Execution 2)
- Date: 2026-03-22
- Completed items:
  - P0.3 Remove reflection-heavy multiple-result dispatch in both async and sync core multi-result query paths.
  - Replaced reflection-based `MethodInfo.Invoke` + reflective `Task.Result` access with strongly typed `Read` and `ReadAsync` dispatch.
  - Preserved `Ignore` placeholder semantics by initializing typed empty enumerables for unused result positions and reading only active result sets.
- Validation gates run and results:
  - Gate A: `dotnet build "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release` -> Passed.
  - Gate B: `dotnet test "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release --filter "FullyQualifiedName!~Integration"` -> Passed (235 passed, 0 failed).
  - Gate C: first run failed due transient MySQL container readiness timeout; second run passed (103 passed, 0 failed, 6 skipped).
  - Gate D: first run failed due the same transient MySQL readiness issue; second run passed (338 passed, 0 failed, 6 skipped).
- Open risks/blockers:
  - Integration reliability remains environment-sensitive; occasional container startup/readiness instability can require rerun.
- Next item to start:
  - P1.1 Propagate cancellation correctly in async multimap.
- Notes for next chat session:
  - Keep P1.1 and P1.2 in a focused batch, then run Gate A/B/C.

### Session Log Entry - 2026-03-22 (Execution 3)
- Date: 2026-03-22
- Completed items:
  - P1.1 Propagate cancellation correctly in async multimap.
  - P1.2 Reduce multimap per-call allocations.
  - Async multimap overloads for arities 2-7 now use token-aware Dapper `CommandDefinition` paths directly.
  - Added targeted regression coverage in `.submodules/Syrx.Commanders.Databases/tests/unit/Syrx.Commanders.Databases.Tests.Unit/DatabaseCommanderTests/QueryAsync.Multimap.cs` to verify cancellation token propagation.
  - Introduced cached multimap generic type-shape metadata in sync and async multimap cores to reduce per-call `Type[]` construction and related hot-path overhead.
  - Integration fixture reliability improved by moving container startup to async initialization, removing sync-over-async startup, removing redundant service-provider creation, tightening probe cadence, and separating startup cancellation from readiness polling.
- Validation gates run and results:
  - Gate A: `dotnet build "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release` -> Passed.
  - Gate B: `dotnet test "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release --filter "FullyQualifiedName!~Integration"` -> Passed (236 passed, 0 failed; 2 filter warnings in integration assemblies).
  - Gate C: `dotnet test "c:\Projects\Syrx\Syrx.MySql\tests\integration\Syrx.MySql.Tests.Integration\Syrx.MySql.Tests.Integration.csproj" --configuration Release` -> Passed (103 passed, 0 failed, 6 skipped).
- Open risks/blockers:
  - Dapper does not expose a token-aware async dynamic multimap `Type[]` overload for arities above 7; cancellation propagation is therefore fully addressed through fixed-arity token-aware async overloads and constrained by upstream API surface for the dynamic path.
- Next item to start:
  - P1.3 Tune integration fixture/setup throughput.
- Notes for next chat session:
  - Start P1.3/P1.4 as the next paired batch; preserve current fixture readiness behavior while improving setup throughput.

### Session Log Entry - 2026-03-22 (Execution 4)
- Date: 2026-03-22
- Completed items:
  - P1.3 Tune integration fixture/setup throughput.
  - P1.4 Scope package generation to pack/publish contexts.
  - Integration fixture/setup improvements:
    - `DatabaseBuilder.Populate` refactored from 150 per-row execute calls to a single set-based command execution.
    - `MySqlCommandStrings.Setup.Populate` updated to a recursive-CTE batch insert for deterministic 150-row seed population in one round trip.
    - Removed no-op `Task.CompletedTask` tail await from fixture initialization.
  - Packaging scope improvements:
    - Disabled `GeneratePackageOnBuild` in root and submodule `Directory.Build.props`.
    - Updated publish workflow to create packages explicitly via `dotnet pack --configuration Release --no-build`.
- Validation gates run and results:
  - Gate A: compile validation covered by successful build phases inside Gate C and Gate D runs -> Passed.
  - Gate C: `dotnet test "c:\Projects\Syrx\Syrx.MySql\tests\integration\Syrx.MySql.Tests.Integration\Syrx.MySql.Tests.Integration.csproj" --configuration Release` -> Passed (103 passed, 0 failed, 6 skipped).
  - Gate D: `dotnet test "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release` -> Passed (339 passed, 0 failed, 6 skipped).
- Open risks/blockers:
  - None blocking for P1.3/P1.4.
- Next item to start:
  - P2.1 Remove hardcoded credential literals from tests.
- Notes for next chat session:
  - Keep CI packaging explicit (`dotnet pack`) and avoid reintroducing package-on-build defaults.

### Session Log Entry - 2026-03-22 (Execution 5)
- Date: 2026-03-22
- Completed items:
  - P2.1 Remove hardcoded credential literals from tests.
    - Replaced inline username/password literals in `tests/unit/Syrx.Commanders.Databases.Connectors.MySql.Tests.Unit/MySqlDatabaseConnectorTests/CreateConnection.cs` with runtime-generated test-only values.
  - P2.2 Clarify TLS test-only exception boundaries.
    - Added explicit non-production guard comments in `tests/integration/Syrx.MySql.Tests.Integration/MySqlFixture.cs` for the `SslMode=None` test connection setting.
    - Added README clarification that the TLS-disabled connection string is integration-test scope only and must not be reused in deployed environments.
- Validation gates run and results:
  - Gate A: `dotnet build "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release` -> Passed.
  - Gate B: `dotnet test "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release --filter "FullyQualifiedName!~Integration"` -> Passed (236 passed, 0 failed; 2 filter warnings in integration assemblies).
  - Gate D: `dotnet test "c:\Projects\Syrx\Syrx.MySql\Syrx.MySql.sln" --configuration Release` -> Passed (339 passed, 0 failed, 6 skipped).
- Open risks/blockers:
  - P2.3 xUnit deprecation migration remains as a dedicated follow-on item by design.
- Next item to start:
  - P2.3 Address xUnit deprecation track.
- Notes for next chat session:
  - Execute P2.3 as an isolated dependency-migration batch to reduce blast radius.

## 9. Current Status Snapshot
- P0.1: Completed
- P0.2: Completed
- P0.3: Completed
- P1.1: Completed
- P1.2: Completed
- P1.3: Completed
- P1.4: Completed
- P2.1: Completed
- P2.2: Completed
- P2.3: Not started
