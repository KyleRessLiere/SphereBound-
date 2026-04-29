# Tasks: 00 Core Combat Loop

## Phase 1 - Core

[x] Define the core combat state model for a 6x6 board, 1 player, 1 enemy, turn ownership, action count, unit positions, unit health, and alive/dead state.
Acceptance:
- Core state can represent the full v1 combat setup from the requirements.
- Core state does not depend on Unity scene types.

[x] Initialize a combat scenario with the player at 5 HP, the enemy at 3 HP, and the player acting first with 2 available actions.
Acceptance:
- Initial state matches the requirements spec exactly.
- Player turn is active at combat start.

[x] Implement turn progression between player turn and enemy turn with explicit turn start and turn end handling.
Acceptance:
- Player turn starts before enemy turn.
- Active side changes only through turn flow.

## Phase 2 - Events

[x] Define request, result, turn, and failure event contracts for the core combat loop.
Acceptance:
- Event contracts include `MoveRequested`, `AttackRequested`, `DamageRequested`, `TurnStarted`, `TurnEnded`, `ActionStarted`, `ActionSpent`, `ActionEnded`, `UnitMoved`, `UnitDamaged`, `UnitDying`, `UnitDeath`, `UnitRemoved`, `ActionFailed`, and `MoveBlocked`.
- Event contracts are usable by the pure C# core without Unity dependencies.

[x] Implement player movement resolution through `MoveRequested` using board bounds, occupancy checks, and 1-tile orthogonal validation.
Acceptance:
- Valid movement updates position correctly.
- Invalid movement emits failure events and does not change state.
- Successful movement spends exactly 1 player action.

[x] Implement attack resolution through `AttackRequested` and `DamageRequested` using adjacency validation and fixed damage of 1.
Acceptance:
- Valid adjacent attacks damage the target by 1.
- Invalid attacks fail without mutating state.
- Successful attacks spend exactly 1 player action.

[x] Implement death resolution so lethal damage emits death events and removes the dead unit from board occupancy before later actions can reference it.
Acceptance:
- Units die at 0 health.
- Dead units emit the expected death/removal events.
- Dead units are removed from the board and cannot act.

## Phase 3 - Edge Cases

[x] Implement failed-action handling so invalid actions emit `ActionFailed`, blocked movement emits `MoveBlocked`, and failed actions do not spend player actions.
Acceptance:
- Every invalid action emits `ActionFailed`.
- Invalid movement also emits `MoveBlocked`.
- Failed actions do not reduce remaining actions.

[x] Implement enemy behavior resolution for the single-enemy v1 loop: attack if adjacent, otherwise move 1 orthogonal tile toward the player with vertical-first tie-breaking.
Acceptance:
- Enemy acts only after the player turn ends.
- Enemy attacks when adjacent.
- Enemy moves toward the player when not adjacent.
- When both axes reduce distance, enemy prefers vertical movement first.

[x] Prevent invalid actors and targets from resolving actions, including dead units, non-adjacent targets, occupied destination tiles, and out-of-turn actions.
Acceptance:
- Dead units cannot act or be attacked as valid live targets.
- Occupied and out-of-bounds movement requests fail.
- Out-of-turn actions fail without state mutation.

## Phase 4 - Verification

[x] Add deterministic verification coverage for turn order, action spending, movement success/failure, attack resolution, enemy behavior, death/removal, and event ordering.
Acceptance:
- Verification proves the full v1 combat loop works from initial state through death/removal.
- Verification covers both success and failure cases.
- Event ordering is validated against the design spec.

[x] Update this task file during execution so only one task is marked in progress at a time and completed tasks are reflected here.
Acceptance:
- Task status remains aligned with actual execution progress.
- The file remains the source of truth for implementation progress.
