# Design: 02 In-Repo Core Runner

## Flow

In-Repo Core Runner provides a permanent repository-hosted command-line entry point for executing the pure C# gameplay verification stack.

High-level flow:
- User runs a stable in-repo command such as `dotnet run --project ...`.
- The runner project starts outside Unity.
- The runner invokes the existing verifier suites in a deterministic order.
- Each suite prints human-readable progress/output.
- The runner summarizes overall success or failure.
- The process exits with `0` on success and non-zero on failure.

The runner is only a host. It does not own gameplay rules, scenario definitions, or combat resolution logic.

## Event Flow

### Request Phase

The runner does not create new gameplay request semantics.

Its responsibility is to invoke the existing pure C# verifier and scenario entry points that already drive the gameplay core.

Request-side design rules:
- command-line invocation selects the runner entry point
- the runner chooses which built-in verifier suites to execute
- the runner reuses existing pure C# scenario and verification entry points

### Resolution Phase

Resolution remains owned by the already-implemented core gameplay and scenario systems.

Runner responsibilities during resolution:
- call verifier suites in a stable order
- collect pass/fail results
- catch and report runner-host-level exceptions
- keep suite execution isolated within the current process run

Core responsibilities during resolution:
- execute combat logic
- execute scenario logic
- emit and verify gameplay events
- preserve deterministic behavior

### Result Phase

The runner produces console output and a process exit code.

Result output includes:
- suite names or suite sections
- individual completed checks or failure details
- overall summary status
- process exit code semantics

Result rules:
- successful runs end with exit code `0`
- failed runs end with non-zero exit code
- failure output identifies which suite or check failed
- output order remains deterministic across repeated runs

## State Changes

The in-repo runner owns only host/process-level execution state.

Runner-owned state includes:
- suite execution order
- aggregated pass/fail results
- console output state
- final exit status

The runner does not own:
- combat gameplay state
- scenario state definitions
- event formatting rules inside the scenario runner

State rules:
- each process run starts fresh
- the runner must not persist mutable state between runs
- the runner must not alter gameplay or scenario results to fit hosting needs

## Effect Interactions

This feature introduces no gameplay effects and no effect-specific behavior.

It simply hosts the already-existing pure C# systems.

For current scope:
- it executes the combat core verifier
- it executes the scenario runner verifier
- it may be extended later to host additional pure-core verifier suites

## Failure Conditions

Failure cases the runner must handle:
- a verifier suite throws an exception
- a verifier reports a failing check
- invalid command-line usage
- host startup/configuration issues

Failure handling rules:
- the runner must clearly report host-level failures
- the runner must clearly report verifier-level failures
- failure output must remain readable and deterministic
- a failure in one suite must not make the output ambiguous about which suite failed

## Logging

Console output is the primary logging surface for this feature.

Logging requirements in design terms:
- identify which suite is running
- print completed checks or failure information in a readable order
- print a final overall status line
- preserve deterministic output ordering

The runner should not replace scenario logs.

Instead:
- scenario/core verifiers remain the source of their own detailed check output
- the in-repo runner provides top-level orchestration and summary output

## System Impact

This feature adds a permanent operational entry point for the existing pure C# core tooling.

It enables:
- a stable in-repo `dotnet run` workflow
- easier local validation without temp scripts
- clearer onboarding and documentation
- future extension to more pure-core verifier suites

It must not:
- duplicate gameplay code
- introduce Unity dependencies
- redefine verification logic already owned by existing verifier suites
- create a second execution path for combat rules

Open design limits for future phases:
- exact repo location of the runner project is still open
- exact CLI shape is still open
- optional suite filtering is deferred
