# Tasks: 14 Core 4x4 Board Layout

## Phase 1 - Core Layout Contract

[x] Centralize the default board-dimension and starting-position constants so the migration has one canonical source of truth.
Acceptance:
- The default state no longer hardcodes inline player/enemy start coordinates.
- Future `4x4` migration touches a single core layout definition path first.

[x] Switch the authoritative default board from `6x6` to `4x4` and move default unit placements to valid `4x4` coordinates.
Acceptance:
- The default combat state builds with `4x4` dimensions.
- Player and enemy default positions are valid on that board.

## Phase 2 - Scenario And Behavior Migration

[x] Update scenario catalogs and validation setups to valid `4x4` coordinates.
Acceptance:
- Existing scenario intent is preserved on the smaller board.
- Behavior-driven scenarios still produce deterministic action flow.

[x] Update behavior and movement assumptions that depend on old board spacing.
Acceptance:
- Move-toward and scripted behavior scenarios remain valid.
- No scenario uses out-of-bounds or impossible placements.

## Phase 3 - Verifier And Log Migration

[x] Update verifier expectations and board assertions from `6x6` to `4x4`.
Acceptance:
- Board-size assertions match `4x4`.
- Board snapshot output remains readable and deterministic.

[x] Re-run the permanent verifier stack and ensure the migrated board state passes all suites.
Acceptance:
- Existing suites pass after migration.
- Scenario logs and verifier log files reflect the new board size.

## Phase 4 - Unity Board Presentation

[x] Update the tactical board renderer to present the `4x4` board with a slight visual split between player side and enemy side.
Acceptance:
- Unity renders `4x4` tiles from core state.
- The side gap is presentation-only and does not break input mapping.

[x] Verify board selection, movement highlights, and attack preview still map correctly to the new layout.
Acceptance:
- Click targeting still resolves against core coordinates.
- Highlights and units align with the visual board after the gap offset.

## Phase 5 - Reflection

[x] Update `specDev/current-state.md`, this task file, and `specDev/index.md` after completion.
Acceptance:
- The holistic state document reflects the `4x4` core board and Unity side-gap presentation.
- Feature tracking reflects completion accurately.
