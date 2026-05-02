# Feature: 05 Unity Debug Action Controls

## REQUIREMENTS SPEC

## Goal

Allow developers to manually trigger core gameplay actions from Unity for debugging so they can verify core state changes, event order, and Unity bridge logging without introducing real gameplay UI or moving rules into Unity.

## Feature Interpretation

Unity Debug Action Controls adds a thin developer-facing control surface on top of the existing Unity Listener Bridge so Unity can issue a small set of manual debugging actions into the pure C# combat core and observe the normal event-driven results.

## Behavior

- Unity must allow a developer to manually trigger the following player-facing debug actions:
  - `Move`
  - `Attack`
  - `End Turn`
  - `Restart Combat`
- These actions may be triggered through Unity debugging surfaces such as the Inspector or context menu.
- Triggered actions must execute through the existing core combat system rather than through Unity-authored rule logic.
- Triggered actions must produce the same event flow, validation behavior, and logging behavior as equivalent core-driven gameplay actions.
- Unity must continue to mirror core unit state after actions resolve so developers can inspect the updated result.

## Rules

- Unity must not implement gameplay rules for movement, attack validation, damage, death, turn flow, or enemy behavior.
- All debug actions must go through the core gameplay system.
- Unity must not directly mutate unit state, turn state, or board occupancy.
- Triggered actions must emit the same event patterns as the existing core system.
- Invalid actions must fail through normal core validation and must not be silently accepted.
- Restarting combat must recreate or reinitialize the core session through the bridge layer rather than patching Unity-side mirrored state.
- The Unity-side control layer must remain thin, replaceable, and debug-focused.

## Edge Cases

- Attempting to move to an invalid or blocked tile must fail through the normal core failure path.
- Attempting to attack when no valid adjacent target exists must fail through the normal core failure path.
- Attempting to act when the player has no remaining actions must fail through the normal core failure path.
- Restarting during an active turn must reset to a clean new combat session rather than preserving partial in-turn state.
- Multiple debug actions triggered rapidly must not cause Unity to bypass ordered core resolution.
- Repeated restart requests must not create duplicate bridge subscriptions or stale mirrored state.

## Constraints

- No real UI system is in scope yet.
- No animation, VFX, audio, or HUD logic is in scope.
- The feature must use the existing Unity Listener Bridge rather than creating a separate Unity gameplay path.
- The feature must not modify core combat rules to make Unity debugging work.
- The feature must remain developer-focused and not become a player-facing interaction layer.
- Deterministic behavior must be preserved.
- Existing out-of-Unity verifier workflows must continue to pass unchanged.

## Interactions

- Uses the existing core combat loop.
- Uses the existing event system and event ordering rules.
- Uses board state, turn state, and action validation from the core.
- Uses the existing Unity Listener Bridge for logging and mirrored state.
- Uses the existing logging system so Unity output stays comparable to core-runner output.

## Event Hooks

Triggered actions must flow through the normal core event system, including applicable events such as:

- `MoveRequested`
- `AttackRequested`
- `ActionStarted`
- `DamageRequested`
- `UnitMoved`
- `UnitDamaged`
- `UnitDying`
- `UnitDeath`
- `UnitRemoved`
- `ActionSpent`
- `ActionEnded`
- `ActionFailed`
- `MoveBlocked`
- `TurnEnded`
- `TurnStarted`

Restart flow must use the bridge/core session lifecycle and result in a fresh session start that exposes the same observable startup events as a new combat session, including `TurnStarted`. If a broader combat-session start/end event exists later, Unity debug controls must reuse it rather than inventing a parallel lifecycle.

## Logging Requirements

- All triggered debug actions must appear in Unity Console logs.
- Logs must preserve the event order emitted by the core.
- Logged output must remain human-readable and useful for debugging movement, attack, damage, death, failure events, and restart behavior.
- Failure events must be logged and distinguishable from successful actions.
- Unity-side logs should remain aligned with the meaning and ordering of the existing core runner output, even if formatting is not byte-for-byte identical.

## Acceptance Criteria

- [ ] A developer can trigger `Move` from Unity.
- [ ] A developer can trigger `Attack` from Unity.
- [ ] A developer can trigger `End Turn` from Unity.
- [ ] A developer can trigger `Restart Combat` from Unity.
- [ ] Triggered actions flow through the existing core system rather than Unity-authored gameplay rules.
- [ ] Event order remains correct in Unity logging.
- [ ] Invalid actions produce normal failure events.
- [ ] Unity does not directly modify authoritative gameplay state.
- [ ] Mirrored Unity debug state updates after actions and restart.
- [ ] Existing core runner tests still pass.
- [ ] Existing Unity bridge logging behavior continues to work.
- [ ] No gameplay behavior changes are introduced to the core combat system.

## AMBIGUITIES

- It is not yet defined whether debug controls should be exposed primarily through Inspector fields, context menu items, or both.
- It is not yet defined how move destinations should be specified in Unity for debugging, such as direct coordinate entry, relative offsets, or preset actions.
- It is not yet defined how attack target selection should work when the system supports more than one possible target in future features.
- It is not yet defined whether the debug controls should only support the player unit in v1 or be structured for arbitrary unit selection later.
- It is not yet defined how aggressively rapid repeated debug action triggers should be throttled or guarded on the Unity side versus simply relying on the core's ordered validation.
