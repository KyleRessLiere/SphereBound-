# Feature: 00 Core Combat Loop

## REQUIREMENTS SPEC

## Goal

Validate the full gameplay loop including turn order, movement, attacking, damage, death, and event flow.

## Feature Interpretation

Core Combat Loop defines the baseline turn-based combat rules for Spherebound. In the initial version, one player-controlled unit and one enemy unit act on a 6x6 grid in sequence, using the same core movement, attack, damage, and death rules, with all state changes flowing through the event system.

## Player Behavior

- The player has a turn before enemies act.
- The player has a fixed number of actions during their turn.
- The player has 2 actions during their turn.
- The player can move 1 tile orthogonally by spending an action.
- The player can attack an adjacent enemy by spending an action.
- The player starts with 5 HP.
- The player cannot continue acting after all available actions are spent.

## Enemy Behavior

- Enemy actions begin only after the player turn ends.
- Each enemy performs one behavior per enemy turn.
- The initial version contains 1 enemy.
- If an enemy is not adjacent to the player, it moves toward the player.
- If an enemy is adjacent to the player, it attacks the player.
- The enemy moves 1 orthogonal tile toward the player when moving.
- If both a vertical move and a horizontal move would reduce distance to the player, the enemy prefers vertical movement first, then horizontal.
- The enemy starts with 3 HP.

## Rules

- Combat takes place on a grid-based board.
- The board size is 6x6.
- The initial version contains 1 player unit and 1 enemy unit.
- Position matters for movement, attacks, and occupancy checks.
- Movement is orthogonal only.
- A move changes position by 1 tile.
- Movement cannot pass through or end on an occupied tile.
- Attacks only target orthogonally adjacent units unless otherwise specified later.
- Attacks deal fixed damage of 1.
- Units die when health reaches 0.
- Dead units are removed from the board.
- Player and enemy follow the same underlying gameplay rules for movement, adjacency, damage, and death handling.

## Edge Cases

- Moving into an occupied tile fails and does not change state.
- Moving outside board bounds fails and does not change state.
- Attacking when no valid adjacent target exists fails and does not change state.
- Acting when a unit has no remaining actions fails and does not change state.
- Attacking a dead unit must not be possible.
- Dead units cannot take further actions.
- If a unit dies from damage, death and board removal must complete before later actions reference that unit.
- Failed actions emit `ActionFailed`.
- Failed movement emits `MoveBlocked`.
- Failed actions do not spend an action unless a later spec explicitly overrides that rule.

## Constraints

- All actions must flow through the event system.
- No direct state mutation may occur outside event resolution.
- The event pattern must follow request -> resolve -> result.
- Core combat logic must live in the pure C# gameplay core.
- Core gameplay logic must not depend on Unity scene objects or MonoBehaviour-driven rules.
- Unity is responsible only for input, presentation, and reacting to core events.
- No effects or items are included in this feature.

## Interactions

- Movement interacts with board occupancy and board bounds.
- Attacks interact with adjacency validation and the damage system.
- Damage interacts with health reduction and death detection.
- Death interacts with board removal and prevention of further actions.
- Turn flow interacts with player action spending and enemy behavior sequencing.
- No effects or item interaction is in scope for this feature.

## Event Hooks

### Request Phase

- `MoveRequested`
- `AttackRequested`
- `DamageRequested`

### Result Phase

- `UnitMoved`
- `UnitDamaged`
- `UnitDying`
- `UnitDeath`
- `UnitRemoved`

### Turn Events

- `TurnStarted`
- `TurnEnded`
- `ActionStarted`
- `ActionSpent`
- `ActionEnded`

### Failure Events

- `ActionFailed`
- `MoveBlocked`

## Acceptance Criteria

- [ ] Player turn starts before enemy turn.
- [ ] Player can spend actions during their turn.
- [ ] Player movement updates unit position correctly on a valid tile.
- [ ] Movement fails when the destination tile is invalid or occupied.
- [ ] Player attack damages an adjacent enemy.
- [ ] Enemy actions occur only after the player turn ends.
- [ ] A non-adjacent enemy moves toward the player.
- [ ] When both vertical and horizontal movement reduce distance, enemy movement prefers vertical first.
- [ ] An adjacent enemy attacks the player.
- [ ] Units die when health reaches 0.
- [ ] Dead units are removed from the board.
- [ ] Failed actions emit `ActionFailed` and do not spend an action.
- [ ] Event output shows the expected order for turn, action, movement, damage, death, and removal events.

## AMBIGUITIES

- Enemy ordering is intentionally unspecified in this requirements version because v1 only contains 1 enemy.
- It is not yet explicit whether non-movement failures should emit feature-specific failure events beyond `ActionFailed`.
