# Tasks: 13 Basic 3D Tactical Board View

## Phase 1 - Board Contracts

[x] Define the Unity-side board-view state model and tile highlight categories without changing core gameplay rules.
Acceptance:
- The board view has explicit presentation-only states for idle, selection, movement, preview, and invalid feedback.
- The model stays downstream of the existing bridge and runtime UI path.

## Phase 2 - Board Rendering

[x] Implement primitive-based board rendering for a visible floating `6x6` tactical grid.
Acceptance:
- Grey square tiles render with slight spacing.
- Tiles map deterministically to core board coordinates.
- The board reads as a simple 3D tactical plane.

[x] Implement primitive-based unit rendering for the current player and one enemy.
Acceptance:
- The player renders as a sphere.
- The enemy renders as a cube.
- Units are smaller than their tiles and centered correctly.
- Removed units disappear visually.

## Phase 3 - Highlight And Input Flow

[x] Implement selection and highlight behavior for valid movement and attack preview.
Acceptance:
- Valid movement tiles highlight yellow.
- Attack or ability preview tiles highlight red.
- Selected tile remains visible.
- Highlight state can be reset and recomputed cleanly.

[x] Implement board-click interaction that coexists with the existing runtime UI path.
Acceptance:
- Normal tile clicks can still route movement through the bridge/core path.
- Ability-button activation can enter preview mode.
- Attack preview requires a second click to confirm.
- Clicking off or pressing `Esc` cancels preview mode.

## Phase 4 - Sync And Tooling

[x] Sync board and unit visuals from bridge snapshots and core events.
Acceptance:
- Unit positions update from core-driven state changes.
- Unit removal hides or destroys visuals.
- Existing Unity logs keep working.

[x] Add an editor-side scaffold for creating the tactical board view and its camera/setup objects.
Acceptance:
- The feature can be added to a scene without manual low-level hierarchy setup.
- The scaffold reuses the existing Unity bridge/runtime UI workflow where practical.

## Phase 5 - Verification And Reflection

[x] Re-run the permanent in-repo core runner and confirm all existing verifier suites still pass.
Acceptance:
- Existing pure-core verifier suites remain green.
- No Unity-owned gameplay changes were introduced.

[x] Update `specDev/current-state.md`, this task file, and `specDev/index.md` after the feature is complete.
Acceptance:
- The holistic current-state document reflects the new 3D tactical board capability.
- Feature tracking reflects completion accurately.
