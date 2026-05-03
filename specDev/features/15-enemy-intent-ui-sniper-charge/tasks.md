# TASKS

## Phase 1: Intent Snapshot Contract

- [x] Define the feature task breakdown for the combined `5x5` board migration, enemy intent UI, and sniper charge behavior.
- [x] Add the shared core and bridge-facing enemy intent snapshot contract types.
- [x] Add a reusable intent summary builder path that can represent ordinary enemy behaviors and charging enemies.

## Phase 2: Core 5x5 Migration

- [x] Migrate default board construction and canonical spawn helpers from `4x4` to `5x5`.
- [x] Update scenario setups, verifier assumptions, and board-log expectations to `5x5`.
- [x] Update Unity tactical board generation to mirror the new `5x5` core board size.

## Phase 3: Sniper Behavior

- [x] Add the sniper unit or definition setup and its line-shot ability definition.
- [x] Add persistent sniper charge state handling in the pure core.
- [x] Implement deterministic sniper alignment, charge, countdown, and fire behavior through the existing behavior and ability systems.

## Phase 4: Enemy Intent UI

- [x] Expose enemy intent snapshot data through the Unity bridge runtime state path.
- [x] Add the runtime `Enemy Intent` text panel and bind it to core-generated intent summaries.
- [x] Refresh the panel on initialization, turn changes, decision updates, countdown changes, firing, death, removal, and restart.

## Phase 5: Verification And Reflection

- [x] Add or update verifier coverage for `5x5` board migration, enemy intent summaries, sniper countdown behavior, and alignment-sensitive firing.
- [x] Update verifier log output where scenario expectations change.
- [x] Reflect the completed feature in `specDev/current-state.md` and mark the feature done in `specDev/index.md`.
