# Design: 06 Unity Combat Debug Surface

## Flow

Unity Combat Debug Surface builds a larger debugging layer on top of the existing Unity Listener Bridge and Unity Debug Action Controls.

High-level flow:
- Unity bridge creates or attaches to the observable core combat session.
- A developer triggers debug actions from a visible Unity debugging surface.
- The existing bridge/debug-command path forwards those actions into the pure C# combat core.
- Core gameplay events stream into Unity in the order emitted.
- The Unity debug surface reacts to those observed events by:
  - logging gameplay events immediately
  - updating mirrored unit/debug state
  - printing a full board snapshot after movement resolution
  - printing an attack overlay snapshot after attack resolution
  - updating visible remaining player actions when they change

The Unity debug surface is still observational and trigger-oriented. It does not own gameplay rules or authoritative state.

## Event Flow

### Request Phase

The request phase continues to use the existing Unity debug action controls.

Request responsibilities:
- expose visible Unity-side controls for initialize, start, move, attack, end turn, and restart
- collect developer-entered debug parameters such as acting unit, target tile, or target unit
- forward the request through the existing bridge/session debug-command path

Request-phase rules:
- Unity control code must not validate gameplay legality beyond minimal null/setup guards
- Unity must not invent alternate move, attack, turn, or enemy rules
- the visible control surface is only a trigger mechanism into the core

### Resolution Phase

Resolution remains entirely in the pure C# combat core.

Core responsibilities during resolution:
- validate movement, attack, turn, and enemy behavior
- mutate authoritative combat state when valid
- emit ordered gameplay events and failure events

Unity debug surface responsibilities during resolution:
- receive events in order
- avoid reordering or suppressing those events
- derive board snapshots and action-count output from authoritative post-event state

Resolution rules:
- movement-triggered board output must only occur after a movement has actually resolved through the core
- attack-triggered overlay output must only occur after the attack resolution path has completed enough to identify affected tiles and any hit tile
- failed actions must still log their events, but must not fabricate successful movement/attack board outputs

### Result Phase

The result phase expands Unity debugging output beyond plain event lines.

Expected results:
- gameplay events stream into Unity Console as they occur
- remaining available player actions are printed or refreshed when they change
- after a resolved movement event, a full board snapshot is printed
- after an attack resolution, an attack overlay snapshot is printed
- mirrored unit/debug state remains updated for Inspector inspection

Result rules:
- the event stream remains primary and ordered
- board/overlay outputs are downstream views derived from the observed authoritative state
- the board log must use exactly one symbol per tile
- attack overlay output replaces the normal tile symbol on affected cells rather than layering symbols

## State Changes

Authoritative state remains in the pure C# core combat session.

Core-owned state includes:
- unit positions
- health and life state
- board occupancy
- turn ownership
- remaining player actions
- emitted gameplay events

Unity-owned debug state includes:
- visible control/input parameters
- mirrored session snapshot
- derived board-string or board-cell representation for logging
- derived attack-overlay representation for logging
- visible current remaining-actions display state

State rules:
- Unity debug surface may cache derived display data, but never becomes authoritative
- board output is derived from authoritative core state after event processing
- attack overlay output is derived from the observed attack action/result and authoritative state
- restart clears or replaces old derived display state with a fresh session-derived view

## Effect Interactions

No effect-system changes are introduced by this feature.

For current scope:
- movement board output only reflects current board occupancy and unit placement
- attack overlay output reflects the current combat loop’s attack effect area and hit location only
- later combat effects should extend from the core event stream rather than adding Unity-only combat logic

## Failure Conditions

Gameplay failures remain owned by the core.

Gameplay failure cases:
- invalid movement
- invalid attack target or range
- no remaining actions
- out-of-turn actions

Debug-surface failure cases:
- no active session exists
- visible control surface is present but not yet initialized
- board or overlay output is requested with insufficient observed data
- repeated restart/re-enable causes stale session references

Failure-handling rules:
- gameplay failures must still print normal failure events
- failed movement must not print a fake successful movement board snapshot
- failed attack must not print a fake successful hit overlay
- debug-surface setup failures must be distinguishable from gameplay failures
- stale display data must not survive restart or detach

## Logging

Unity Console remains the main ordered log output surface.

Logging order rules:
- gameplay event logs appear immediately when observed
- when remaining actions change, the updated value is logged after the change is authoritative
- after movement resolution, the board snapshot is logged after the relevant movement event sequence
- after attack resolution, the attack overlay is logged after the relevant attack event sequence

Board snapshot rules:
- uses `P` for player units
- uses `E` for enemy units
- uses `.` or an equivalent neutral marker for empty tiles
- renders the whole board every time a resolved movement snapshot is printed

Attack overlay rules:
- uses one symbol per tile
- uses `X` for affected/path tiles
- uses `O` for the collision/hit tile
- `O` replaces `X` on the hit tile
- overlay symbols replace the underlying board symbol for that printout
- example: if an attack affects two tiles and collides on the second tile, the output pattern is `X O`

## System Impact

This feature turns the current Unity debug tooling into a more usable combat-debugging surface.

It enables:
- immediate readable board inspection after movement
- readable attack-path/hit inspection after attacks
- visible Unity-side controls for driving the combat loop manually
- clearer debugging of remaining player actions and board occupancy

It must not:
- move combat rules into Unity
- alter enemy behavior away from the deterministic move-toward-player rules
- change the core event contracts
- change the out-of-Unity runner’s role or expectations
- become a player-facing runtime UI system

Open design limits for later phases:
- whether the surface should live in a custom inspector, editor window, or mixed UI remains open
- exact board origin/orientation in the rendered text remains open
- whether action-count logging should appear on every event line or only on change remains open
