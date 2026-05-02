# Tasks: 05 Unity Debug Action Controls

## Phase 1 - Core

[x] Define the Unity-side debug action input model and bridge-facing command surface for move, attack, end turn, and restart.
Acceptance:
- Unity has a clear debug-facing way to describe the supported actions and any required parameters.
- The command surface stays thin and does not introduce gameplay rules into Unity.

[x] Extend the observable bridge/session surface so Unity debug controls can invoke existing core actions without bypassing current observation/logging behavior.
Acceptance:
- Unity can issue supported debug commands through the existing bridge/session path.
- Existing event observation and mirrored-state behavior remain intact.

[x] Keep the debug action layer developer-focused and replaceable rather than turning it into a general input architecture.
Acceptance:
- The new control surface is clearly scoped to debugging.
- The design does not force future player-input architecture decisions.

## Phase 2 - Events

[x] Implement Unity-triggered move and attack commands that forward through the core combat request/resolution flow.
Acceptance:
- Move requests use the existing core move path.
- Attack requests use the existing core attack path.
- Unity Console logs continue to reflect normal core event ordering.

[x] Implement Unity-triggered end-turn and restart commands through the existing bridge/session lifecycle.
Acceptance:
- End turn uses the existing core turn transition behavior.
- Restart creates a fresh session and reconnects observation without duplicate subscriptions.
- Startup/restart logging and mirrored state refresh remain visible.

[x] Keep mirrored debug state and Unity logging synchronized after successful actions, failed actions, and restart.
Acceptance:
- Mirrored unit state updates after each debug command.
- Failure events and restart transitions remain visible and readable in Unity logs.

## Phase 3 - Edge Cases

[x] Handle invalid debug commands and incomplete debug parameters without creating Unity-authored gameplay mutations.
Acceptance:
- Gameplay-invalid commands fail through normal core failure events.
- Bridge/control-level invalid usage is distinguishable from gameplay failure.
- Later valid commands still work after failed attempts.

[x] Prevent duplicate subscriptions, stale session references, or restart leakage during repeated debug use.
Acceptance:
- Repeated restart/use does not duplicate event handling.
- Old session state does not linger in mirrored debug output.

[x] Preserve deterministic core behavior and keep existing out-of-Unity runner workflows unchanged.
Acceptance:
- Existing core/scenario runner behavior is unchanged.
- The Unity debug layer does not alter core validation or event order.

## Phase 4 - Verification

[x] Verify Unity can trigger move, attack, end turn, and restart through the bridge while preserving ordered logging and mirrored state updates.
Acceptance:
- Each supported debug command can be exercised from Unity-side debugging surfaces.
- Ordered event logging and mirrored state updates are observable after each command.

[x] Re-run the permanent in-repo core runner to confirm existing verifier suites still pass after the Unity debug-control additions.
Acceptance:
- `dotnet run --project Tools/CoreRunner/CoreRunner.csproj` still passes.
- No Unity dependency is introduced into the out-of-Unity runner path.

[x] Update this task file during execution so only one task is marked in progress at a time and completed tasks are reflected here.
Acceptance:
- Task status remains aligned with actual execution progress.
- The file remains the source of truth for implementation progress.
