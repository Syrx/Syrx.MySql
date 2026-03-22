# Syrx.MySql Performance Research Report

## 1. Title And Metadata
- Report: Syrx.MySql performance assessment after dependency updates
- Date: 2026-03-22
- Assessor: GitHub Copilot (GPT-5.3-Codex), performance-researcher mode
- Scope root: c:/Projects/Syrx/Syrx.MySql
- Target solution: Syrx.MySql.sln
- Report type: Research-only (no remediation implemented)

## 2. Scope And Constraints
### In scope
- Root projects in Syrx.MySql.sln
- Included submodule projects referenced by Syrx.MySql.sln, especially:
  - .submodules/Syrx.Commanders.Databases/src/*
  - .submodules/Syrx.Commanders.Databases/.submodules/Syrx/src/*
- Integration and unit test projects included by Syrx.MySql.sln where runtime/test throughput is affected

### Out of scope
- Production code changes
- Benchmark claims without recorded measurements
- Database server-side query-plan tuning without captured plans

### Constraints
- No profiler traces, dotnet-counters output, or BenchmarkDotNet suites were present in this workspace.
- Findings are based on direct source evidence and known runtime behavior of .NET + Dapper execution patterns.

## 3. Methodology And Measurement Sources
- Solution and project graph review from Syrx.MySql.sln and project files.
- Static hotspot inspection for:
  - Connection lifecycle and open/close behavior
  - Async/sync mismatch and cancellation flow
  - Reflection-heavy paths
  - Allocation patterns in hot query APIs
  - Test startup/runtime overhead
- Evidence collection via targeted symbol and pattern search with exact file/line capture.

Measured data available:
- None in workspace (no profiler trace files, no benchmark artifacts).

## 4. Executive Summary
Primary performance risk is concentrated in the shared submodule command layer rather than MySQL-specific connector glue. The largest bottlenecks are reflection-heavy materialization in multi-result query paths and repeated per-call allocations in multimap paths. Cancellation flow is inconsistent in a critical async multimap overload, increasing wasted work under cancellation-heavy scenarios. Test/runtime throughput is additionally impacted by fixture startup blocking and global package-on-build defaults that apply to all projects.

Most impactful remediation targets:
1. Remove reflection invocation from multiple-result query hot paths.
2. Reduce per-call allocations in multimap query APIs.
3. Fix cancellation propagation in async multimap core call.
4. Remove avoidable test harness blocking and redundant service-provider construction.
5. Scope packaging to pack/publish workflows instead of all builds/tests.

## 5. Findings Summary Table
| ID | Priority | Confidence | Category | Component | Short Description |
|---|---|---|---|---|---|
| PERF-001 | High | High | Query hot path / CPU | DatabaseCommander.QueryAsync.Multiple | Reflection invoke and Task.Result reflection used per result-set read |
| PERF-002 | High | High | Query hot path / CPU | DatabaseCommander.Query.Multiple | Reflection invoke used per result-set read in sync path |
| PERF-003 | Medium | High | Allocation pressure | DatabaseCommander.Query(Async).Multimap | Per-call Type[] shaping plus mapper closure allocation |
| PERF-004 | Medium | Medium | Async/cancellation mismatch | DatabaseCommander.QueryAsync.Multimap | CancellationToken accepted but not flowed in multimap core Dapper call |
| PERF-005 | Medium | High | Test runtime throughput | MySqlFixture / DatabaseBuilder | Blocking startup, redundant provider creation, and chatty DB seeding |
| PERF-006 | Medium | High | Build/test throughput | Directory.Build.props (root + submodules) | GeneratePackageOnBuild enabled globally, inflating CI and local test cost |
| PERF-007 | Low | Medium | Async/sync mismatch | DatabaseCommander.ExecuteAsync | Async open helper can fall back to synchronous Open for non-DbConnection providers |

## 6. Detailed Findings

### PERF-001
- Finding ID: PERF-001
- Title: Reflection-heavy async multiple-result materialization on the main query path
- Priority: High
- Confidence: High
- Category: Query hot path / CPU
- Affected files, symbols, or components:
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multiple.cs:492
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multiple.cs:495
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multiple.cs:502
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multiple.cs:504
- Evidence:
  - Uses QueryMultipleAsync(command), then iterates each result-set type.
  - For each result set, invokes generic read via MethodInfo.Invoke.
  - Then uses reflection again to fetch Task.Result property.
  - Type count is recomputed via TakeWhile(...).Count().
- Throughput, latency, allocation, or scalability impact discussion:
  - Reflection invocation per result set adds CPU overhead and prevents JIT devirtualization/inlining.
  - Property reflection for Task.Result adds additional overhead in already latency-sensitive I/O completion path.
  - Cost scales with number of mapped result sets and query volume.
- Recommended remediation:
  - Replace reflective invocation with typed delegates or precompiled generic dispatch paths.
  - Avoid Task.Result reflection by using strongly typed generic helpers.
- Recommended validating agent or skill:
  - csharp-engineering
  - performance-research
- Validation or benchmarking recommendation:
  - Add microbenchmarks for 1/4/8/16 result-set scenarios and compare mean, p95, and allocations.
- Implementation status: Not implemented by performance-researcher

### PERF-002
- Finding ID: PERF-002
- Title: Reflection-heavy sync multiple-result materialization
- Priority: High
- Confidence: High
- Category: Query hot path / CPU
- Affected files, symbols, or components:
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.Query.Multiple.cs:443
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.Query.Multiple.cs:446
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.Query.Multiple.cs:453
- Evidence:
  - Uses QueryMultiple(command) and computes active types with TakeWhile(...).Count().
  - Reads each result set via MethodInfo.Invoke in a loop.
- Throughput, latency, allocation, or scalability impact discussion:
  - Reflection cost is paid on every call and every mapped result set.
  - Under high query rates, CPU burn rises and effective throughput drops.
- Recommended remediation:
  - Same direction as PERF-001 for sync path: typed fast paths and non-reflection dispatch.
- Recommended validating agent or skill:
  - csharp-engineering
  - performance-research
- Validation or benchmarking recommendation:
  - Benchmark sync QueryMultiple variants at increasing result-set counts and compare CPU time.
- Implementation status: Not implemented by performance-researcher

### PERF-003
- Finding ID: PERF-003
- Title: Per-call allocations and repeated type scanning in multimap query core
- Priority: Medium
- Confidence: High
- Category: Allocation pressure
- Affected files, symbols, or components:
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multimap.cs:535
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multimap.cs:541
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.Query.Multimap.cs:479
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.Query.Multimap.cs:485
- Evidence:
  - Each call builds and scans a type array with TakeWhile(...).ToArray().
  - Each call allocates an internal mapper closure Func<object[], TResult>.
- Throughput, latency, allocation, or scalability impact discussion:
  - Creates avoidable Gen0 pressure in high-frequency query workloads.
  - Adds CPU overhead from repeated generic type-shape analysis that is stable per closed generic method.
- Recommended remediation:
  - Cache precomputed type-shape metadata and mapping delegates per generic signature.
  - Eliminate repeated ToArray paths where possible.
- Recommended validating agent or skill:
  - csharp-engineering
  - performance-research
- Validation or benchmarking recommendation:
  - Use BenchmarkDotNet with AllocationDiagnoser on hot multimap overloads.
- Implementation status: Not implemented by performance-researcher

### PERF-004
- Finding ID: PERF-004
- Title: Cancellation token not propagated in async multimap core execution
- Priority: Medium
- Confidence: Medium
- Category: Async/cancellation mismatch
- Affected files, symbols, or components:
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multimap.cs:520
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multimap.cs:524
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multimap.cs:564
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multimap.cs:571
- Evidence:
  - Method accepts CancellationToken and builds a CommandDefinition with token.
  - Final call uses connection.QueryAsync(sql: ..., commandTimeout: ..., commandType: ...), not QueryAsync(CommandDefinition).
  - This path does not visibly pass cancellation token into the executed Dapper call.
- Throughput, latency, allocation, or scalability impact discussion:
  - Under canceled or timed-out requests, work may continue unnecessarily, increasing DB and thread-pool load.
  - In bursty workloads this can amplify tail latency and reduce throughput.
- Recommended remediation:
  - Use Dapper overload that consumes CommandDefinition carrying cancellation token on this path.
- Recommended validating agent or skill:
  - csharp-engineering
  - async-programming guidance
- Validation or benchmarking recommendation:
  - Add cancellation-focused integration tests that assert fast cancellation and no extra command completion.
- Implementation status: Not implemented by performance-researcher

### PERF-005
- Finding ID: PERF-005
- Title: Integration fixture adds avoidable startup and setup overhead
- Priority: Medium
- Confidence: High
- Category: Test runtime throughput
- Affected files, symbols, or components:
  - tests/integration/Syrx.MySql.Tests.Integration/MySqlFixture.cs:55
  - tests/integration/Syrx.MySql.Tests.Integration/MySqlFixture.cs:60
  - tests/integration/Syrx.MySql.Tests.Integration/MySqlFixture.cs:71
  - tests/integration/Syrx.MySql.Tests.Integration/MySqlFixture.cs:74
  - tests/integration/Syrx.MySql.Tests.Integration/MySqlFixture.cs:112
  - tests/integration/Syrx.MySql.Tests.Integration/DatabaseBuilder.cs:87
  - tests/integration/Syrx.MySql.Tests.Integration/DatabaseBuilder.cs:95
- Evidence:
  - Constructor blocks async startup via GetAwaiter().GetResult().
  - DisposeAsync uses Task.Run for a console write.
  - Service provider is built twice in InitializeAsync (unused local provider + Install call).
  - DB readiness poll uses fixed Task.Delay loop.
  - Setup population issues 150 individual Execute calls in loop.
- Throughput, latency, allocation, or scalability impact discussion:
  - Increases local/CI test wall-clock time and variance.
  - Chatty setup loop increases startup round trips before actual tests run.
- Recommended remediation:
  - Make lifecycle fully async without sync blocking.
  - Remove unnecessary Task.Run in dispose.
  - Build provider once and reuse.
  - Batch or set-based seed operations for setup.
- Recommended validating agent or skill:
  - csharp-engineering
  - task-research
- Validation or benchmarking recommendation:
  - Capture end-to-end integration suite time before/after; track p50/p95 runtime and container-ready duration.
- Implementation status: Not implemented by performance-researcher

### PERF-006
- Finding ID: PERF-006
- Title: Global package generation on every build inflates test/build throughput cost
- Priority: Medium
- Confidence: High
- Category: Build/test throughput
- Affected files, symbols, or components:
  - Directory.Build.props:19
  - .submodules/Syrx.Commanders.Databases/Directory.Build.props:19
- Evidence:
  - GeneratePackageOnBuild is enabled in top-level and submodule build props.
  - This setting applies broadly, including scenarios where packaging artifacts are not needed (for example test runs).
- Throughput, latency, allocation, or scalability impact discussion:
  - Increases build I/O and packaging work during routine test/build loops.
  - Scales poorly with solution size and transitive project graph.
- Recommended remediation:
  - Gate package generation to dedicated pack/publish pipelines or explicit pack configurations.
- Recommended validating agent or skill:
  - csharp-engineering
  - github-actions-ci-cd-best-practices
- Validation or benchmarking recommendation:
  - Compare clean and incremental build durations with and without package-on-build for test-focused commands.
- Implementation status: Not implemented by performance-researcher

### PERF-007
- Finding ID: PERF-007
- Title: Async execute path may fall back to synchronous connection open
- Priority: Low
- Confidence: Medium
- Category: Async/sync mismatch
- Affected files, symbols, or components:
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.ExecuteAsync.cs:91
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.ExecuteAsync.cs:98
- Evidence:
  - OpenConnectionAsync uses DbConnection.OpenAsync when possible.
  - Falls back to IDbConnection.Open() for non-DbConnection implementations.
- Throughput, latency, allocation, or scalability impact discussion:
  - If a provider returns only IDbConnection abstraction without DbConnection, open becomes blocking in async flow.
  - Current MySql connector likely returns DbConnection, so this is a portability/scalability risk rather than immediate hotspot.
- Recommended remediation:
  - Constrain connector contracts to async-capable connection types or isolate/provider-adapter fallback with explicit warning.
- Recommended validating agent or skill:
  - csharp-engineering
  - async-programming guidance
- Validation or benchmarking recommendation:
  - Add provider contract tests to ensure returned connections support true async open paths.
- Implementation status: Not implemented by performance-researcher

## 7. Missing Skills, Information, Instrumentation, Or Tooling
- No runtime profiler traces (dotnet-trace, PerfView, ETW) were available.
- No BenchmarkDotNet benchmark project exists in the workspace.
- No explicit query execution plans or database-side performance counters were captured.
- No structured latency/throughput telemetry (p95, p99, req/sec) was provided for production-like workloads.
- No orchestrator tool was available in this environment for formal multi-agent classification routing.

## 8. Cross-Agent Remediation Handoff Recommendations
1. csharp-engineering
   - Owns implementation for PERF-001, PERF-002, PERF-003, PERF-004, PERF-007.
2. architecture-and-ddd
   - Review if 16-overload mapping surface should be simplified or bounded by architectural policy.
3. github-actions-ci-cd-best-practices
   - Owns pipeline/build configuration changes for PERF-006.
4. debug
   - Reproduce and isolate PERF-005 setup overhead in CI and local environments with timing traces.
5. ms-sql-dba (or MySQL DBA equivalent)
   - Validate DB-side impact when cancellation and round-trip reductions are introduced.

## 9. Appendix: Searched Files, Commands, Traces, References, And Assumptions
### Key files inspected
- Syrx.MySql.sln
- Directory.Build.props
- src/Syrx.Commanders.Databases.Connectors.MySql/MySqlDatabaseConnector.cs
- src/Syrx.Commanders.Databases.Connectors.MySql.Extensions/MySqlConnectorExtensions.cs
- tests/integration/Syrx.MySql.Tests.Integration/MySqlFixture.cs
- tests/integration/Syrx.MySql.Tests.Integration/DatabaseBuilder.cs
- .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.ExecuteAsync.cs
- .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multimap.cs
- .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.Query.Multimap.cs
- .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.QueryAsync.Multiple.cs
- .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/DatabaseCommander.Query.Multiple.cs
- .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases.Settings.Readers/DatabaseCommandReader.cs

### Commands and searches executed
- Pattern scans for blocking calls, reflection use, LINQ scans, connection open/close, and cancellation usage.
- Solution and project metadata inspection for dependency and build-property impacts.

### External references used
- None fetched externally during this assessment.

### Assumptions
- Syrx.MySql.sln is the canonical build/test entrypoint for this assessment.
- Hot path designation is based on command APIs used repeatedly by repositories and tests.
- Without runtime metrics, impact magnitude is directional, not numerically benchmarked.
