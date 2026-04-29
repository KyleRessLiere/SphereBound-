# Feature: 04 Unity Listener Bridge

## REQUIREMENTS SPEC

## Goal

Create a thin Unity-side bridge that connects to the pure C# core combat loop, listens to core events, and mirrors/logs core state for debugging without owning gameplay logic.

## Feature Interpretation

Unity Listener Bridge defines a minimal Unity-facing adapter layer that can initialize or connect to the pure C# core combat loop, subscribe to its event output, and reflect basic core state into Unity for observation and debugging while leaving all gameplay rules in the core.

## Behavior

- Unity can initialize or connect to the core combat loop.
- Unity can subscribe to core events emitted by the pure C# gameplay core.
- Unity logs received core events to the Unity Console in the same order they are emitted.
- Unity can mirror basic player and enemy state for debugging, such as position, health, and alive/dead status.
- Unity may create simple placeholder visual objects only if needed to support observation/debugging.
- Unity remains a listener/presentation bridge and does not own combat resolution.

## Rules

- Unity must not mutate gameplay state directly.
- Unity must not contain combat, movement, damage, death, turn, or enemy AI rules.
- Unity must not bypass the core event system.
- Unity listener output must preserve core event order.
- The Unity bridge must remain thin and replaceable.
- The bridge must reuse the existing pure C# core and event flow instead of duplicating gameplay logic.
- No animations, UI, player input, VFX, or audio are in scope for this feature.

## Edge Cases

- If the core emits failure-related events, Unity must still log them in order.
- If the core state changes rapidly within a sequence, Unity must still preserve observed event order.
- If placeholder objects are used, they must remain debugging aids only and not become a second source of gameplay state.
- If Unity reconnects or reinitializes the bridge, it must not create duplicate gameplay-rule ownership.
- Unity observation must not break or alter the existing out-of-Unity verifier workflows.

## Constraints

- Core gameplay logic remains in pure C# and outside Unity-specific rule ownership.
- Unity-side code may use Unity APIs for observation/presentation only.
- No new gameplay system may be introduced on the Unity side.
- Existing core verifier suites must continue to pass unchanged.
- The bridge must not require animations, UI, player input, VFX, or audio to function.

## Interactions

- Uses the existing core combat loop feature.
- Uses the existing core event-driven gameplay flow.
- Uses existing core state as the source of truth.
- May reflect basic unit state into placeholder Unity objects for debugging only.
- Must coexist with the current in-repo core runner and verifier suites without altering their behavior.

## Event Hooks

The Unity bridge must be able to receive and log existing core events, including when they occur:

- `TurnStarted`
- `TurnEnded`
- `ActionStarted`
- `ActionSpent`
- `ActionEnded`
- `MoveRequested`
- `UnitMoved`
- `AttackRequested`
- `DamageRequested`
- `UnitDamaged`
- `UnitDying`
- `UnitDeath`
- `UnitRemoved`
- failure-related events such as `ActionFailed` and `MoveBlocked`

Unity must log these in the order received from the core.

## Logging Requirements

- Unity Console output must be human-readable.
- Logged output must preserve event order.
- Logs must be useful for debugging movement, attack, damage, death, failure events, and mirrored unit state.
- Logs must identify enough context to understand what happened, such as event type and relevant unit/state values.
- Logging must remain thin and observational rather than becoming gameplay logic.

## Acceptance Criteria

- [ ] Unity can initialize or connect to the core combat loop.
- [ ] Core events are received in Unity.
- [ ] Received events appear in Unity in the same order emitted by the core.
- [ ] Player and enemy state can be observed from Unity for debugging.
- [ ] Unity does not bypass the core event system.
- [ ] Unity does not contain gameplay rules for combat, movement, damage, death, turn flow, or enemy AI.
- [ ] Placeholder Unity objects, if used, remain debugging-only and do not become the gameplay source of truth.
- [ ] Existing core verifier suites still pass.
- [ ] No gameplay behavior changes.
- [ ] No animations, UI, player input, VFX, or audio are required for this feature.

## AMBIGUITIES

- It is not yet defined whether Unity should initialize a fresh core combat session itself or attach to an already-created core session.
- It is not yet defined exactly how mirrored unit state should be represented in Unity when no real presentation layer exists yet.
- It is not yet defined whether placeholder visual objects are required or optional for the initial version.
- It is not yet defined how much event/state detail should print to the Unity Console by default.
- It is not yet defined whether the bridge should observe only combat-loop sessions or be structured for broader future core systems as well.
