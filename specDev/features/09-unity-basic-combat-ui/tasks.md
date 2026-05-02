# Tasks: 09 Unity Basic Combat UI

## Phase 1 - Core UI Data

[x] Define the UI-facing player action and ability-button data surface needed for runtime `uGUI`.
Acceptance:
- The Unity layer has a thin model for movement controls, end turn, and ability-button display.
- Ability-button display data can include name, description, cost, and resolved effect-tile coordinates.
- The model stays presentation-focused and does not own gameplay rules.

[x] Expose definition-backed player ability metadata and current resolved effect-tile coordinates through the existing Unity bridge path.
Acceptance:
- Unity can read the player's current abilities from the live session.
- Unity can read resolved effect-tile coordinates for each ability under the current simple targeting assumptions.
- This data comes from the core/bridge path rather than hardcoded UI data.

[x] Add multiple simple test abilities to the current player definition so the UI can prove it is reading live definition-backed abilities.
Acceptance:
- The player definition contains multiple basic test abilities.
- These abilities expose metadata and resolved tiles distinctly enough for runtime UI validation.

## Phase 2 - Runtime UI

[x] Create a basic `uGUI` runtime control surface with directional movement and end-turn buttons.
Acceptance:
- Unity has visible `Up`, `Down`, `Left`, `Right`, and `End Turn` controls.
- Pressing those controls routes through the existing bridge/core path.

[x] Replace the multi-button ability list with one large selected-ability button plus left/right cycle controls.
Acceptance:
- The runtime UI shows one large selected-ability button.
- Left/right controls cycle across the current player ability list.
- The large ability button shows name, description, and resolved effect-tile coordinates on the button itself.

[x] Update the Unity Editor scaffold utility so it creates the single large ability button plus left/right cycle controls instead of the older multi-button ability list layout.
Acceptance:
- Unity can create the runtime Canvas, control buttons, selected-ability button, cycle controls, and scene wiring from an Editor command.
- The generated hierarchy is wired to the existing bridge and runtime UI controller scripts.
- The utility remains a setup convenience and does not add gameplay rules.

[x] Wire the selected ability button, cycle controls, and movement controls into the existing gameplay command path without duplicating gameplay logic.
Acceptance:
- UI-triggered actions use the same gameplay path as current debug controls.
- UI does not implement combat resolution locally.

## Phase 3 - Edge Cases

[x] Handle missing session, restart, dead player, invalid action states, and ability-index clamping without breaking the UI refresh cycle.
Acceptance:
- The UI stays stable across restart and failure cases.
- Invalid actions still fail through normal logs and state remains core-owned.

[x] Preserve existing Inspector debug controls and current Unity debug logging behavior.
Acceptance:
- Inspector controls still work.
- Existing board, attack overlay, and action-count logs still work for UI-triggered actions.

## Phase 4 - Verification

[x] Add verifier coverage for UI-facing selected-ability metadata, cycling behavior, and resolved effect-tile coordinate exposure.
Acceptance:
- Pure verifier coverage proves the UI data source is definition-backed and deterministic.

[x] Re-run the permanent in-repo core runner and any relevant Unity-adjacent verification coverage.
Acceptance:
- The permanent runner still passes.
- No Unity dependency leaks into the out-of-Unity runner path.

[x] Update this task file and the holistic current-state document during execution so the new feature is reflected accurately.
Acceptance:
- Task status matches implementation progress.
- `specDev/current-state.md` reflects the runtime UI surface once complete.
