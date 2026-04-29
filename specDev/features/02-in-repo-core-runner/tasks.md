# Tasks: 02 In-Repo Core Runner

## Phase 1 - Core

[x] Create a permanent in-repo `.NET` runner project with a stable repository path and runnable entry point.
Acceptance:
- The repository contains a dedicated runnable project for the pure C# core.
- The project can be invoked with a stable `dotnet run --project ...` command.

[x] Wire the runner project to reuse the existing pure C# combat core, scenario runner, and verifier code without duplicating gameplay rules.
Acceptance:
- The runner project executes existing shared code rather than maintaining a second copy of gameplay logic.
- No Unity runtime dependencies are required by the runner project.

[x] Implement top-level runner flow that executes the built-in verifier suites in deterministic order.
Acceptance:
- The runner executes the combat core verifier suite.
- The runner executes the scenario runner verifier suite.
- Suite order is stable across repeated runs.

## Phase 2 - Events

[x] Implement readable console output that identifies suite execution and completed checks.
Acceptance:
- Output clearly shows which verifier suites ran.
- Output includes readable per-check progress or results.
- Output ordering is deterministic.

[x] Implement overall success/failure summary output for the runner process.
Acceptance:
- Successful runs clearly indicate overall success.
- Failed runs clearly indicate overall failure.
- The summary is readable without inspecting code.

[x] Preserve existing scenario/core logging behavior while keeping the in-repo runner as the orchestration layer only.
Acceptance:
- The runner does not redefine gameplay event logging rules.
- Existing verifier/scenario output remains compatible with the new host project.

## Phase 3 - Edge Cases

[x] Implement non-zero exit code behavior for verifier failure and host-level failure conditions.
Acceptance:
- Verification failures return a non-zero exit code.
- Host-level failures return a non-zero exit code.
- Successful runs return exit code `0`.

[x] Implement failure reporting that distinguishes runner-host failures from verifier/check failures.
Acceptance:
- Output identifies whether failure came from host setup or verifier execution.
- A failing suite or check can be identified from console output.

[x] Ensure repeated executions start cleanly without shared mutable state between runs.
Acceptance:
- Repeated executions produce deterministic output for unchanged code.
- No prior process state leaks into later runs.

## Phase 4 - Verification

[x] Verify the in-repo runner command works end-to-end and executes both current verifier suites successfully.
Acceptance:
- The stable in-repo command runs successfully from the repository.
- Both current verifier suites execute through the permanent runner project.

[x] Update documentation so the permanent in-repo runner becomes the primary documented workflow instead of the temporary host script.
Acceptance:
- `README.md` documents the permanent runner command.
- The temporary host script is no longer presented as the primary path.

[x] Update this task file during execution so only one task is marked in progress at a time and completed tasks are reflected here.
Acceptance:
- Task status remains aligned with actual execution progress.
- The file remains the source of truth for implementation progress.
