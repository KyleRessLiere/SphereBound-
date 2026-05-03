# Requirements Spec: 14 Core 4x4 Board Layout

## Feature Interpretation

Replace the current `6x6` combat board with a core-owned `4x4` playable board, while updating Unity presentation so the player side and enemy side read as two opposing halves with a slight visual gap between them.

This is a gameplay-board change first and a Unity board-view update second. The pure C# core remains authoritative for board shape, coordinate validity, movement, targeting, behaviors, and event flow.

## Goal

- Change the combat board from `6x6` to `4x4` in the pure C# core.
- Keep all gameplay validation downstream of the new core board shape.
- Update Unity board rendering so the `4x4` board visually reads as a player side and enemy side with a slight presentation gap between those halves.
- Preserve deterministic combat behavior and existing architecture boundaries.

## Core Board Requirements

- The authoritative combat board must become `4` tiles wide by `4` tiles tall.
- Core board validation must treat only `4x4` coordinates as valid.
- Existing movement, targeting, and affected-tile resolution must use the new board dimensions.
- Scenario setup and initial unit placement must be updated to valid `4x4` positions.
- Core board shape must not be faked in Unity while remaining `6x6` internally.

## Coordinate And Layout Requirements

- Core coordinates must remain deterministic and grid-based.
- The board must still support direct mapping from core coordinates to Unity tiles.
- Any previous assumptions tied to `6x6` dimensions must be updated to `4x4`.
- Out-of-bounds validation must reflect the new board dimensions everywhere in the core.

## Scenario And Initial State Requirements

- The default combat scenario must be updated for the `4x4` board.
- The current `1` player and `1` enemy setup must still work on the new board.
- Starting positions must preserve a sensible initial combat lane and not begin in an invalid or overlapping state.
- Existing scenario and verifier setups that depend on board size must be updated to valid `4x4` placements.

## Ability And Targeting Requirements

- Ability affected-tile resolution must remain deterministic on the `4x4` board.
- Ability patterns that partially extend off the smaller board must continue to resolve safely according to existing core rules.
- Invalid target and out-of-board behavior must still fail through normal failure events.
- Existing basic attack, line, cross, and forced-movement validation must continue to function under the new board dimensions.

## Movement Requirements

- Player movement validation must use the `4x4` board limits.
- Enemy behavior movement validation must use the `4x4` board limits.
- Blocked, occupied, and out-of-bounds movement must continue to fail through the existing core path.
- Movement range and action cost rules do not change unless separately specified by a future feature.

## Behavior Requirements

- Existing pluggable behaviors must remain deterministic on the `4x4` board.
- Move-toward-target behavior must still choose valid movement intents on the smaller board.
- Scripted and scenario-assigned behaviors must be updated where previous coordinates no longer fit.
- Behavior logs must continue to reflect the chosen intents and downstream results in order.

## Unity Presentation Requirements

- Unity board rendering must reflect the new `4x4` core board.
- The Unity tactical board must visually read as two opposing sides:
  - player-side half
  - enemy-side half
- A slight visual gap should separate those halves in presentation.
- The gap is presentation-only and must not create non-playable tiles or Unity-owned rules.
- Tile selection, movement highlighting, and attack preview highlighting must continue to map to valid core tiles only.

## Unity Layout Direction

- The board should still use primitive square tiles.
- The player side and enemy side should be distinguishable through spacing/layout first, not through Unity-owned rule partitions.
- The slight side gap must not break core coordinate mapping or click targeting.
- Existing sphere and cube unit presentation should continue to work on the resized board.

## Event And Logging Requirements

- Existing core event flow must remain unchanged in structure.
- Unity debug/event logs must continue to work.
- Board-related debug output and verifier logs must reflect the new `4x4` board state.
- Any board snapshots written by debug or verifier tooling must update to the new dimensions.

## Verification Requirements

- Existing verifier suites must continue to pass after being updated for `4x4` assumptions.
- Scenario-based validation must continue to cover:
  - attack hit
  - attack miss
  - ability shape validation
  - multi-turn combat resolution
  - behavior-driven movement
  - forced movement
- Verifier log output must show the updated `4x4` board states.

## Constraints

- Do not leave the core board as `6x6` while only changing Unity visuals.
- Do not move board validation rules into Unity.
- Do not introduce duplicate board authority in the Unity layer.
- Do not change combat rules unrelated to board size unless required for valid migration.
- Do not introduce animation, VFX, audio, or unrelated presentation scope in this feature.

## Acceptance Criteria

- The authoritative core board is `4x4`.
- Default scenarios and unit placements are valid on the new board.
- Movement and targeting reject coordinates outside the new `4x4` bounds.
- Existing ability resolution still functions deterministically on the smaller board.
- Existing behaviors still produce valid deterministic intents on the smaller board.
- Scenario and verifier suites still pass after board-size migration.
- Unity renders a `4x4` board instead of `6x6`.
- Unity presentation shows a slight visual gap between the player side and enemy side.
- Unity interactions still route through the bridge/core path and do not own validation.
- Logs and board snapshots reflect `4x4` board output consistently.

## Ambiguities

- Which exact starting coordinates should the default player and enemy use on the `4x4` board.
- Whether the visual side gap should split rows, columns, or simply offset the opposing halves while preserving coordinate order.
- Whether any existing ability examples need to change shape definitions for better readability on the smaller board, or only their scenario placements.
- Whether the board formatter/debug snapshot output should remain dense rectangular output or include presentation-aware spacing only in Unity-facing views.
