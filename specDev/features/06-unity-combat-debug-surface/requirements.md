# Feature: 06 Unity Combat Debug Surface

## REQUIREMENTS SPEC

## Goal

Provide a larger Unity-side debugging surface that makes the current combat loop easier to inspect and exercise by showing the whole board as a readable grid and exposing basic debug controls for running player actions and turn flow.

## Feature Interpretation

Unity Combat Debug Surface combines two debugging needs into one vertical feature:

- a full board-state logger/debug view that shows the current combat grid in a human-readable form
- visible Unity debugging controls for issuing basic player actions and advancing the combat loop

This feature remains a debugging/presentation layer on top of the existing pure C# combat core. It must not change combat rules or move gameplay ownership into Unity.

## Behavior

- Unity must expose a readable board-state representation for the current combat session.
- The board-state representation must show the entire grid, not only mirrored per-unit fields.
- The board must represent:
  - player units as `P`
  - enemy units as `E`
  - empty tiles as a neutral placeholder such as `.`
- Gameplay events must continue to appear in Unity as they are emitted by the core.
- The debug output must show currently available player actions and update that value when it changes.
- Whenever player movement or enemy movement resolves, Unity must print the board-state output so developers can inspect the updated grid immediately.
- Whenever an attack resolves, Unity must print an attack-focused board output that highlights affected tiles.
- Unity must expose basic developer controls for:
  - initializing or restarting combat
  - starting observed combat
  - moving the player
  - attacking with the player
  - ending the player turn
- These controls must run through the existing core combat system and existing Unity bridge/debug-action path.
- Enemy behavior must remain the current deterministic core behavior:
  - enemy acts after the player turn
  - enemy moves toward the player if not adjacent
  - enemy attacks if adjacent

## Rules

- Unity must not implement gameplay rules for movement, attacking, damage, death, turn flow, or enemy AI.
- All debug controls must go through the existing core request/resolution flow.
- Unity must not directly mutate authoritative board or unit state.
- The board-state view must be derived from authoritative core state.
- The board-state view must not become a second source of truth.
- Enemy movement must continue to follow the current approved core combat spec and must not become random in this feature.
- The feature may improve discoverability and readability in Unity, but it must not change gameplay behavior.

## Edge Cases

- Invalid move or attack requests from Unity must still fail through normal core validation and failure events.
- The board view must correctly reflect dead-unit removal from the board.
- Repeated restart actions must reset the board view to a fresh initial state.
- The board view must remain readable even if no actions have been taken yet.
- The debug surface must not duplicate subscriptions or duplicate command execution on re-enable/restart.
- If combat state changes rapidly through a turn transition or enemy action sequence, the board view/log must still reflect the final authoritative state after the relevant movement or attack resolution.
- If multiple gameplay events occur within a turn, those events must still stream in order before any triggered board snapshot or attack overlay is printed.
- If player actions are spent, reset, or otherwise changed, the logged available-action value must update to reflect the current authoritative state.
- Attack-focused board output must remain readable even when an attack affects no valid target tile.

## Constraints

- No player-facing runtime UI is in scope.
- No animation, VFX, audio, HUD, or polished scene presentation is in scope.
- The feature must build on the existing `UnityCombatListenerBridge` and debug action controls.
- The feature must not modify the core combat rules for enemy behavior, movement, attack, damage, death, or turn order.
- The feature must remain Editor/debug focused.
- Existing out-of-Unity verifier suites must continue to pass unchanged.

## Interactions

- Uses the existing core combat loop feature.
- Uses the existing Unity Listener Bridge.
- Uses the existing Unity Debug Action Controls feature.
- Uses authoritative core board state and mirrored session state for display.
- Uses the existing event/logging path so Unity Console output remains aligned with the core runner.

## Event Hooks

The debug surface does not introduce a parallel gameplay event system.

When Unity controls trigger combat actions, the normal core events must still occur as applicable, including:

- `TurnStarted`
- `TurnEnded`
- `MoveRequested`
- `AttackRequested`
- `ActionStarted`
- `ActionSpent`
- `ActionEnded`
- `ActionFailed`
- `MoveBlocked`
- `DamageRequested`
- `UnitMoved`
- `UnitDamaged`
- `UnitDying`
- `UnitDeath`
- `UnitRemoved`

The board-state view/log must refresh from the resulting authoritative core state after these events.
Movement-triggered board output and attack-triggered overlay output must remain downstream of the normal core event flow rather than replacing it.

## Logging Requirements

- Unity must provide a board-state output that shows the full grid clearly.
- The board-state output must be human-readable and useful for debugging movement and board occupancy.
- Unity Console logs for gameplay events must continue to appear when those events occur.
- Log output must show remaining available player actions and update whenever that value changes.
- Movement board output must print after player or enemy movement resolves.
- Attack board output must print after attack resolution.
- Attack board output must show affected tiles with `X`.
- Attack board output must show collision or impact with `O` when an attack connects at a tile.
- Attack board output must replace the normal board symbol on a tile rather than layering multiple symbols in one cell.
- If an attack passes through or targets tiles, those affected tiles should show `X` even if another board symbol would normally be shown there.
- If an attack collides or hits on a tile, that tile should show `O` instead of `X` or the underlying board symbol.
- If board output is logged to the Console, it must remain readable and ordered relative to the streaming gameplay event logs.
- The board-state representation should make it easy to confirm where `P` and `E` are after each action or turn sequence.

## Acceptance Criteria

- [ ] A developer can see the full combat board as a readable grid in Unity.
- [ ] The board view shows player units as `P`, enemy units as `E`, and empty tiles clearly.
- [ ] Gameplay events appear in Unity as they are emitted by the core.
- [ ] Remaining available player actions are visible in the logs and update when they change.
- [ ] The board view prints after player and enemy movement and reflects the resolved board state.
- [ ] Attack execution prints an attack-focused board output with `X` for affected tiles and `O` for collision or impact.
- [ ] Attack overlay output uses one symbol per tile, with `O` replacing `X` on the hit tile.
- [ ] A developer can trigger basic combat debug actions from Unity through a visible debugging surface.
- [ ] Those actions flow through the same core combat path as the existing debug controls.
- [ ] Enemy behavior remains the current deterministic move-toward-player behavior.
- [ ] Invalid actions still fail through normal core validation and failure events.
- [ ] Unity does not directly modify authoritative gameplay state.
- [ ] Existing out-of-Unity runner suites still pass.
- [ ] No gameplay behavior changes are introduced to the core combat system.

## AMBIGUITIES

- It is not yet defined whether the board-state surface should live primarily in the Inspector, the Unity Console, a custom Editor panel, or a combination.
- It is not yet defined whether the board should render with top-left or bottom-left origin from the developer’s point of view.
- It is not yet defined whether the visible control surface should use a custom inspector, editor window, or another Unity Editor mechanism.
- It is not yet defined whether lightweight in-surface status messaging should be shown in addition to the existing Unity Console logs.
