# Tasks: 12 Scenario Ability And Behavior Validation

## Phase 1 - Suite Contract

[x] Define the dedicated scenario-validation suite contract and its initial scenario list on top of the existing scenario runner and verifier infrastructure.
Acceptance:
- The validation suite has a clear home in the pure C# verifier stack.
- The initial scenario set is named and scoped without introducing gameplay code changes.
- The contract stays compatible with existing scenario-runner reporting and verifier-log output.

[x] Define reusable expectation helpers or conventions for exact event validation versus critical-subsequence validation.
Acceptance:
- Each scenario can declare whether it uses exact ordered matching or critical-subsequence matching.
- The validation rule is explicit rather than inferred ad hoc per test.

## Phase 2 - Scenario Definitions

[x] Add deterministic scenario definitions for:
- basic attack hit
- basic attack miss
- ability shape validation
- multi-turn combat resolution
- behavior plus movement interaction
Acceptance:
- Each scenario defines initial state, behaviors, expected outcomes, and failure conditions.
- Scenarios remain fully executable in the pure C# runner.

[x] Add forced-movement validation scenario support if included for this feature pass.
Acceptance:
- Forced movement uses the existing ability and movement systems only.
- The scenario remains deterministic and validates movement constraints.

## Phase 3 - Verifier Integration

[x] Add a dedicated verifier suite that executes the new scenario validations through the existing runner path.
Acceptance:
- The suite runs from the permanent in-repo runner.
- Pass/fail reporting matches the current verifier style.

[x] Integrate verifier log output so each scenario-validation check emits colocated combat-flow logs.
Acceptance:
- Logs appear in the verifier-specific `.logs` directory.
- Logs preserve board state, ordered events, and final state.

## Phase 4 - Verification And Reflection

[x] Re-run the permanent in-repo core runner and confirm all existing suites still pass with the new validation suite included.
Acceptance:
- Existing verifier suites remain green.
- The new suite passes deterministically.

[x] Update `specDev/current-state.md`, this task file, and `specDev/index.md` after the feature is complete.
Acceptance:
- The holistic current-state document reflects the new validation-suite capability.
- Feature tracking reflects completion accurately.
