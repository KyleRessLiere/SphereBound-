# Tasks: 03 Runner Console Log Output

## Phase 1 - Core

[x] Define the runner-side reporting contract needed to expose per-scenario run results, log lines, and verification summaries to the in-repo host.
Acceptance:
- The in-repo runner can access structured per-scenario output data without re-running scenario logic.
- No gameplay rules or scenario execution behavior change.

[x] Update the scenario verification/runner reporting path so scenario suite execution returns the data needed for console log printing.
Acceptance:
- Scenario suite execution provides scenario names, ordered log lines, and verification outcomes.
- Existing deterministic scenario execution remains unchanged.

[x] Keep repeated scenario execution deterministic and isolated while exposing report data to the host.
Acceptance:
- Repeated runs still produce identical scenario logs.
- Reporting data does not introduce shared mutable state between runs.

## Phase 2 - Events

[x] Print per-scenario console sections in the in-repo runner using the existing scenario log lines.
Acceptance:
- Each scenario section includes the scenario name.
- Scenario event log lines print in their original order.
- Movement, attack, damage, death, and failure events are visible when present.

[x] Print scenario verification summaries alongside each scenario’s log output.
Acceptance:
- Console output shows whether each scenario verification passed or failed.
- Verification summary information is readable next to the scenario logs.

[x] Preserve suite-level and overall runner output while adding scenario log sections.
Acceptance:
- Existing suite and overall summaries still print.
- Added scenario sections do not make output ambiguous or unreadable.

## Phase 3 - Edge Cases

[x] Preserve readable per-scenario output when gameplay failures or verification failures occur.
Acceptance:
- Failing scenarios still print their log lines.
- Verification failures remain distinguishable from host-level failures.

[x] Ensure multi-scenario output remains clearly separated and deterministic.
Acceptance:
- Multiple scenario sections do not interleave incorrectly.
- Scenario ordering remains stable across runs.

[x] Keep in-repo runner exit-code behavior unchanged after console log expansion.
Acceptance:
- Successful runs still exit `0`.
- Suite/host failures still return non-zero.

## Phase 4 - Verification

[x] Verify the permanent runner command prints per-scenario logs end to end through the existing `dotnet run --project Tools/CoreRunner/CoreRunner.csproj` workflow.
Acceptance:
- The stable runner command prints scenario names and full event logs.
- Output remains deterministic across repeated runs.

[x] Update README so scenario log output is documented as part of the runner workflow.
Acceptance:
- README explains that per-scenario logs print to console.
- Documentation reflects the actual runner output.

[x] Update this task file during execution so only one task is marked in progress at a time and completed tasks are reflected here.
Acceptance:
- Task status remains aligned with actual execution progress.
- The file remains the source of truth for implementation progress.
