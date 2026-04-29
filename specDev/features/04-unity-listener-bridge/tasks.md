# Tasks: 04 Unity Listener Bridge

## Phase 1 - Core

[x] Define the bridge-side contract and mirrored debug-state types needed for Unity to observe a core combat session without owning gameplay logic.
Acceptance:
- Unity-facing bridge contracts can describe mirrored unit state and observed session state.
- The contract/state layer does not introduce gameplay rules or direct gameplay mutation.

[x] Define the core-session observation surface the Unity bridge will use to receive ordered events and read authoritative core state.
Acceptance:
- The bridge has a clear way to access ordered core events and current unit/session state.
- No Unity-authored gameplay logic is required to use the observation surface.

[x] Keep the bridge-side observation model thin and replaceable.
Acceptance:
- The bridge-facing layer is observational only.
- The model does not become a second source of truth for gameplay.

## Phase 2 - Events

[x] Implement Unity-side event subscription and ordered Unity Console logging for the core combat event stream.
Acceptance:
- Unity receives core events.
- Unity logs them in the same order emitted by the core.
- Failure-related events are logged when present.

[x] Implement mirrored player/enemy debug state updates driven by core events and/or authoritative core state reads.
Acceptance:
- Unity can observe unit position, health, and alive/dead state for debugging.
- Mirrored state is derived from the core and not authored independently in Unity.

[x] Add optional placeholder debug-object support without making placeholders authoritative gameplay state.
Acceptance:
- Placeholder objects, if used, remain debugging-only.
- The bridge still works in logging/state-observation mode without placeholders.

## Phase 3 - Edge Cases

[x] Prevent duplicate subscriptions or duplicate bridge ownership when Unity reinitializes or reconnects.
Acceptance:
- Repeated initialization does not duplicate event handling.
- The bridge stays thin and replaceable.

[x] Preserve ordered logging and state observation during failure events and rapid event sequences.
Acceptance:
- Event order remains stable in Unity logging.
- Failure/debug output remains readable.

[x] Ensure the Unity bridge does not bypass the core event system or mutate gameplay state directly.
Acceptance:
- Unity does not inject combat, movement, damage, death, turn, or enemy AI rules.
- Gameplay state remains authoritative in the pure C# core.

## Phase 4 - Verification

[x] Verify the Unity bridge can initialize/connect, receive ordered core events, and expose basic player/enemy debug state without changing gameplay behavior.
Acceptance:
- Core events are visible in Unity.
- Player/enemy debug state is observable in Unity.
- No gameplay behavior changes.

[x] Re-run existing core verifier suites to confirm the Unity bridge did not break out-of-Unity core workflows.
Acceptance:
- Existing core verifier suites still pass.
- No Unity dependency is introduced into the existing out-of-Unity runner path.

[x] Update this task file during execution so only one task is marked in progress at a time and completed tasks are reflected here.
Acceptance:
- Task status remains aligned with actual execution progress.
- The file remains the source of truth for implementation progress.
