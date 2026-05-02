# Requirements: [11] Verifier Log Output Files

## Feature Interpretation

Add deterministic file output for verifier runs so each verifier suite produces readable combat-log artifacts in a sibling log folder beside its verifier code for inspection after execution.

## Goal

Allow the project to persist verifier-readable combat logs to files so each verifier suite has an inspectable log artifact showing the combat/event flow for that test or scenario run.

## Output Behavior

- Each verifier suite must be able to write log output to files.
- Log output must be written in a deterministic, readable format.
- Logs must be organized so it is clear which verifier suite produced them.
- The system must support suites that emit:
  - single aggregated logs
  - multiple scenario/test-specific logs
- A verifier run must not require Unity to inspect its logs.

## Directory Structure

- Each verifier suite must have its own output directory or deterministic subdirectory beside its verifier code file.
- Output layout must make it easy to inspect logs by suite and by specific test/scenario case without leaving the verifier code area.
- The structure must avoid mixing unrelated suite logs into a single ambiguous file.
- The file layout must be stable enough that developers can find the same suite output repeatedly in the same folder area as the verifier implementation.

## File Content

- Log files must show the ordered combat/event flow observed by the verifier.
- Log files must remain human-readable.
- Each file must clearly identify:
  - the verifier suite
  - the specific test or scenario name when applicable
- Log content should include the existing formatted combat events, such as:
  - turn events
  - behavior decision events
  - move requests/results
  - ability requests/results
  - damage/death/failure events
- File output must preserve the same deterministic order seen by the verifier.

## Supported Initial Coverage

The first version must define requirements for at least:

- `CombatLoopVerifier`
- `CombatBehaviorVerifier`
- `ScenarioRunnerVerifier`

The design should also remain compatible with the existing other suites, including:

- `AbilityDefinitionVerifier`
- `UnityDebugActionVerifier`
- `CombatRuntimeUiDataVerifier`
- `CombatDebugSurfaceVerifier`
- `CombatDebugFileOutputVerifier`

## Determinism

- Given the same verifier input, the generated log file content must be deterministic.
- File naming and directory layout must follow deterministic rules unless a higher-level run-folder convention is explicitly defined.
- The system must avoid random or unstable log ordering.

## Constraints

- Do not change gameplay behavior.
- Do not move gameplay rules into the verifier file-output layer.
- Do not introduce Unity dependencies into the out-of-Unity runner path.
- Do not duplicate combat execution logic just to create logs.
- Reuse existing event formatting and logging paths where practical.
- File-output failure must not silently alter verifier assertions or gameplay outcomes.

## Interactions

- This feature must integrate with:
  - the permanent in-repo core runner
  - scenario log formatting
  - behavior-decision log formatting
  - existing verifier suites
  - deterministic file-output conventions already present in the repo where appropriate

## Edge Cases

- A verifier suite produces no combat events.
- A verifier suite contains multiple checks and each needs its own output.
- A scenario-based verifier emits multiple scenario logs in one run.
- A verifier fails before a full event stream is produced.
- File output directory already exists.
- Re-running the same verifier should not create ambiguous or conflicting logs without a defined overwrite/segmentation rule.
- A suite contains checks that are not scenario-based but still need readable output.

## Logging Requirements

- The output files must be easy to inspect directly in the repo or output folder.
- Logs must preserve event order exactly as observed by the verifier.
- Logs should make failures diagnosable by showing the final observed event stream even when verification fails.
- If a verifier already has richer scenario log lines, those should be reused rather than replaced with a lower-fidelity format.

## Acceptance Criteria

- [ ] Each targeted verifier suite writes log output to its own deterministic sibling log directory beside the verifier code file.
- [ ] Scenario-based verifier output clearly separates scenario logs by scenario name.
- [ ] Non-scenario verifier output clearly separates logs by check/test name where practical.
- [ ] Log content remains human-readable and ordered.
- [ ] Behavior-decision events appear in file output when present.
- [ ] Existing verifier suites still pass.
- [ ] The out-of-Unity runner still works without Unity dependencies.
- [ ] No gameplay behavior changes are introduced.

## Ambiguities

- It is not yet finalized whether file output should be generated:
  - automatically on every runner invocation
  - only when explicitly enabled
  - or via a separate verifier mode
- It is not yet decided whether files should be:
  - overwritten per suite
  - timestamped per run
  - or grouped under a run folder
- It is not yet finalized whether every individual check inside a suite needs its own file, or whether one suite file is enough for some suites.
- The exact naming convention for colocated verifier log directories is not yet finalized beyond being deterministic and verifier-specific.
- It is not yet finalized how file-output failures should be surfaced:
  - runner warning only
  - verifier failure
  - or separate infrastructure failure reporting
