# Requirements: [13] Basic 3D Tactical Board View

## Feature Interpretation

Add a simple Unity-side 3D tactical board presentation that renders the authoritative core combat state as a visible grid with primitive tiles, a sphere for the player, cubes for enemies, and basic tile highlights for selection and targeting.

## Goal

Provide a minimal playable and debuggable 3D board view in Unity that makes the current combat state visible and interactive without moving gameplay ownership out of the pure C# core.

## Board View

- Unity must render a visible `6x6` tactical board.
- The board must be made from individual square tiles.
- Each tile must correspond directly to a core board coordinate.
- Tiles must be arranged on a flat 3D plane.
- Tiles should appear slightly floating for this phase rather than embedded in terrain.
- The board must read as a simple tactical grid in world space, not as a 2D menu.
- The first version must use primitive Unity objects only.

## Tile Appearance

- Tiles must be visually distinct enough that the grid is readable at a glance.
- Tiles must be grey.
- Tiles must be square.
- Tiles must use slight spacing so the grid is readable.
- Tiles must support presentation-only highlight states.
- Highlight state must not affect core rules, targeting, validation, or movement legality.

## Unit Display

- The player must render as a sphere.
- Enemy units must render as cubes.
- Unit primitives must be smaller than the tile footprint so tile boundaries remain visible.
- Units must sit centered on their current tile.
- Unit positions must always be derived from authoritative core state.
- Unit objects must update when core events change state.
- Units that die or are removed from the board must disappear visually.
- The first version only needs to support one enemy visually and interactively.
- The view should still avoid assumptions that make later multi-enemy support unnecessarily difficult.

## Highlights

The board view must support simple highlight states for at least:

- selected tile
- valid movement tiles
- selected ability affected tiles
- attack or ability hit tiles when enough information is available
- invalid or failure feedback when practical

Highlight behavior rules:

- highlights must be visually clear
- highlights must not require advanced shaders
- highlights must be resettable
- highlights must update when selected ability changes
- highlights must update when unit position changes
- highlights must remain separate from gameplay validation
- valid movement tiles must highlight in yellow
- attack or ability affected tiles must highlight in red

## Interaction

- Unity must allow basic tile selection.
- Unity must support player movement through Unity input, the existing debug UI path, or a combination that still routes through the same bridge/core flow.
- Unity must support ability or attack targeting through Unity input, the existing runtime UI path, or a combination that still routes through the same bridge/core flow.
- Attack flow for this phase must support a two-click pattern:
  - first click selects or previews the attack target area and shows red affected-tile highlights
  - second click confirms the attack request
- Unity input must call into the existing Unity bridge and core action path.
- Unity must not directly mutate board state or unit state.
- Unity must not become authoritative for whether an action is valid.

## Event Sync

- Core events must drive visual updates.
- `UnitMoved` must update primitive positions.
- `UnitDeath` and `UnitRemoved` must hide or remove visual primitives.
- ability or attack events may drive temporary highlights when appropriate
- existing Unity event and debug logs must continue working

The board view must act as a synchronized presentation of the existing event-driven core rather than a second gameplay state machine.

## Camera

- Use a simple static camera.
- The camera must be angled downward enough to see the full board.
- No advanced camera controller is required.

## Constraints

- No animations.
- No VFX.
- No audio.
- No polished HUD.
- No inventory.
- No meta progression.
- No character models.
- No pathfinding visualization beyond simple highlights.
- No duplicate authoritative gameplay state in Unity.
- No Unity-owned combat rules.
- The feature must preserve compatibility with the existing Unity bridge, runtime UI, scenario runner, and out-of-Unity verifier flow.

## Logging

- Existing Unity debug and event logs must continue working.
- The board view must not replace logging.
- Logs must still show event order and failures.
- Visual failures or input failures must still be traceable through the existing log path when they map to core events.

## Acceptance Criteria

- [ ] A `6x6` grid displays in Unity.
- [ ] Each tile is visible and visually distinct.
- [ ] The player appears as a sphere smaller than a tile.
- [ ] The enemy appears as a cube smaller than a tile.
- [ ] Player and enemy appear centered on the correct tiles.
- [ ] Unit movement updates visual positions from core events.
- [ ] Dead or removed units disappear visually.
- [ ] Tile highlights can show a selected tile.
- [ ] Valid movement tiles highlight in yellow.
- [ ] Tile highlights can show selected ability affected tiles.
- [ ] Attack or ability preview tiles highlight in red before confirmation.
- [ ] Highlights clear and reset correctly.
- [ ] Attack confirmation uses a second click after preview rather than immediately executing on first selection.
- [ ] Basic movement or ability input still routes through the existing core path.
- [ ] Invalid actions fail through core events, not Unity-owned rules.
- [ ] Existing verifier suites still pass.
- [ ] Unity remains presentation/input only.

## Ambiguities

- The exact tile size and the amount of spacing between tiles are not yet fixed.
- It is not yet finalized whether tiles should use cubes, quads, or planes.
- It is not yet finalized whether highlights should be implemented via color changes, overlay primitives, outlines, or material swaps.
- It is not yet decided how tile selection and second-click attack confirmation should interact with the existing runtime UI ability-selection flow.
- It is not yet decided whether direct tile-click movement must fully replace debug-button-driven movement for the first pass or can coexist with it.
- Multi-enemy support is deferred beyond the first version, but the degree of future-proofing needed now is still open.
- It is not yet decided whether health text or labels should appear above units now or remain out of scope for this phase.
