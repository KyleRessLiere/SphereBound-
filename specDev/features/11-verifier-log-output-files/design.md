# Design: [11] Verifier Log Output Files

## Flow

This feature adds deterministic verifier-owned file output on top of the existing runner and verifier suites.

The file-output layer does not change how gameplay is simulated or how verifiers assert correctness.

The design flow is:

1. A verifier check runs exactly as it does today.
2. The verifier produces a structured log payload for that specific check.
3. A shared verifier-log writer maps the check to a deterministic file path.
4. The latest run overwrites that file for that check.
5. The runner still reports pass/fail normally.

## Output Model

The design distinguishes two verifier categories:

### 1. Combat-Flow Verifiers

These are verifiers that exercise combat-state progression.

Examples include:

- `CombatLoopVerifier`
- `CombatBehaviorVerifier`
- `ScenarioRunnerVerifier`
- `UnityDebugActionVerifier`
- any future verifier that drives `CombatState` through actions and turn flow

These verifiers must emit rich log files containing:

- check name
- pass/fail result
- initial board state
- ordered event stream
- board snapshots after meaningful state changes
- final board state

### 2. Small Component Verifiers

These are verifiers that check focused data or helper logic without meaningful combat-state progression.

Examples may include:

- formatting verifiers
- metadata verifiers
- small deterministic transformation verifiers

These verifiers emit compact assertion-oriented log files containing:

- check name
- pass/fail result
- short setup/input summary
- assertion summary
- actual vs expected details when relevant

These do not require full board snapshots.

## Directory Structure

The output layout is deterministic and test-oriented.

Each verifier suite uses a sibling log directory beside its verifier code file.

For the current verification stack, this means colocated directories in the verification source area such as:

- `Assets/Scripts/CoreCombatLoop/Verification/CombatLoopVerifier.logs/`
- `Assets/Scripts/CoreCombatLoop/Verification/ScenarioRunnerVerifier.logs/`
- `Assets/Scripts/CoreCombatLoop/Verification/CombatRuntimeUiDataVerifier.logs/`

Inside each verifier-specific log directory:

- each check/test has its own stable file
- rerunning that same check overwrites the same file

This makes the latest output for a given check easy to find without browsing dated run folders.

## File Naming

Each verifier check uses a stable file name derived from:

- suite name
- check/test name

The naming must be deterministic and filesystem-safe.

No timestamp is required for the core per-check files in this feature because the goal is latest-state inspection, not archival history.

## Combat-Flow Log Content

Combat-flow logs must be readable as a step-by-step confirmation artifact.

The output structure should include:

### Header

- suite name
- check/test name
- pass/fail result

### Initial State

- initial board snapshot
- optional unit summary if useful

### Step/Event Body

- ordered event lines
- board snapshots after meaningful state changes

Meaningful state changes include at minimum:

- movement resolution
- successful attack/ability resolution
- damage/death/removal
- turn transitions when board context changes materially

### Final State

- final board snapshot
- final outcome summary

## Small Component Log Content

Small component logs should stay concise.

The output structure should include:

- suite name
- check/test name
- pass/fail result
- what is being asserted
- input/setup summary
- expected vs actual values when useful

These logs should be easy to scan and should avoid noisy board dumps when no combat state exists.

## Shared Writer Layer

The design uses a shared verifier-log writer or helper layer rather than bespoke file-writing logic in every verifier.

This shared layer is responsible for:

- mapping suite/check to output path
- colocating that path with the corresponding verifier source area
- overwriting existing files deterministically
- creating directories as needed
- writing plain-text content

Verifier suites remain responsible for supplying the content that best matches their category.

## Integration With Existing Verifiers

The design must support the current suite set, including:

- `CombatLoopVerifier`
- `CombatBehaviorVerifier`
- `ScenarioRunnerVerifier`
- `AbilityDefinitionVerifier`
- `UnityDebugActionVerifier`
- `CombatRuntimeUiDataVerifier`
- `CombatDebugSurfaceVerifier`
- `CombatDebugFileOutputVerifier`

The implementation may classify some of these as combat-flow and others as small component verifiers depending on how much real combat-state progression they exercise.

## Runner Integration

The permanent in-repo runner remains the entry point.

The runner should continue to:

- execute suites
- print pass/fail summaries
- return proper exit codes

The file-output layer is additive.

It must not create a separate execution path for verifier output.

## Failure Handling

- File-output generation must not alter gameplay behavior.
- File-output generation must not alter verifier assertion logic.
- If file generation fails, that failure must be surfaced explicitly rather than silently ignored.
- The design should treat file-output failure as test infrastructure failure, not as gameplay success.

## Determinism

Determinism requirements:

- same verifier input produces the same file path
- same verifier input produces the same text content
- same event sequence produces the same board/event output order

This is especially important for combat-flow logs because they are meant to confirm deterministic behavior directly.

## Steering Update

When this feature is complete, the steering should be updated so future tests follow the same pattern by default:

- combat-state progression tests must emit rich board/event logs
- small component tests must emit compact assertion logs

This makes verifier logging part of the expected test shape rather than an optional follow-up.

## Deferred Items

The following are out of scope for this phase:

- historical archival of every run
- timestamped per-run storage as the primary output mode
- GUI viewers for verifier logs
- Unity-specific log visualization
- changing gameplay simulation for logging convenience
