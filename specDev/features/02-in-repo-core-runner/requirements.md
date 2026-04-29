# Feature: 02 In-Repo Core Runner

## REQUIREMENTS SPEC

## Goal

Provide a permanent in-repo command-line runner that executes the pure C# core verifiers and scenarios without relying on a temporary external host script.

## Feature Interpretation

In-Repo Core Runner defines a dedicated runnable `.NET` project inside the repository that hosts the existing pure C# combat core, scenario runner, and verifier suites through a stable command such as `dotnet run`.

## Behavior

- The repository contains a permanent runnable project for the pure C# core.
- The runner can execute the existing core combat verifier suite.
- The runner can execute the existing scenario runner verifier suite.
- The runner outputs human-readable results to the console.
- The runner can report success/failure with a process exit code.
- Running the same command against the same code produces deterministic output.

## Rules

- The runner must execute entirely outside Unity.
- The runner must reuse the existing pure C# core and scenario runner code.
- The runner must not duplicate or reimplement gameplay rules.
- The runner must provide a stable in-repo entry point for running current verification logic.
- The runner must support deterministic console output.
- The runner must support at least the current core combat and scenario-runner verification paths.

## Edge Cases

- If verification fails, the runner must report failure clearly and return a non-zero exit code.
- If one verification suite fails, the output must still identify which suite/check failed.
- The runner must not depend on Unity project compilation to execute.
- Repeated executions must not share mutable runtime state between runs.
- Missing or malformed runner inputs must produce clear command-line errors instead of silent failure.

## Constraints

- No Unity runtime types may be required by the runner project.
- The runner must not alter gameplay core behavior just to support command-line hosting.
- The runner must not introduce a second source of truth for scenario execution or verification.
- The runner must remain compatible with the pure C# core architecture defined in `/specDev/decisions`.

## Interactions

- Uses the existing core combat loop implementation.
- Uses the existing core scenario runner implementation.
- Uses the existing verification suites.
- Replaces the temporary ad hoc host script currently documented in `README.md` with a permanent in-repo workflow.

## Event Hooks

- The runner does not define new gameplay events.
- The runner executes and reports on the existing event-driven core systems.
- Console output must remain aligned with the event/log behavior already produced by the core scenario runner and verifiers.

## Logging Requirements

- Console output must be human-readable.
- Output must make it clear which verifier suites and checks ran.
- Output must clearly indicate overall success or failure.
- Output must be deterministic across repeated runs.
- Output must remain useful for debugging when a check fails.

## Acceptance Criteria

- [ ] The repository contains a permanent runnable `.NET` project for the pure C# core.
- [ ] The runner can be executed with a stable in-repo command.
- [ ] The runner executes the current combat core verifier suite.
- [ ] The runner executes the current scenario runner verifier suite.
- [ ] Successful runs produce readable deterministic console output.
- [ ] Failed runs produce readable failure output and a non-zero exit code.
- [ ] No Unity dependencies are required to run the command.
- [ ] The temporary host-script workflow is no longer the primary documented way to run the core.

## AMBIGUITIES

- The exact project location is not yet fixed, for example `Tools/`, `Runner/`, or another repo path.
- The exact command-line interface is not yet fixed beyond requiring a stable in-repo command.
- It is not yet defined whether the runner should support subcommands, filters, or suite selection.
- It is not yet defined whether the runner should reference shared source files directly or copy/include them another way in project structure.
