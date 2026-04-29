# Design: 00 Core Combat Loop

## Flow

Core Combat Loop runs as a deterministic turn sequence inside the pure C# gameplay core.

The initial version contains:
- 1 player unit
- 1 enemy unit
- a 6x6 board

High-level loop:
- Start player turn.
- Give the player 2 actions.
- For each player action attempt, receive a request event and resolve it through core rules.
- End player turn when the player spends both actions or no further valid action is attempted.
- Start enemy turn.
- Allow the enemy to perform exactly 1 behavior.
- If the enemy is adjacent to the player, resolve an attack.
- If the enemy is not adjacent to the player, resolve a 1-tile orthogonal move toward the player.
- End enemy turn.
- Repeat until a unit death causes a combat end condition handled by a later feature.

## Event Flow

### Request Phase

The request phase accepts intents without mutating gameplay state directly.

Expected request events:
- `MoveRequested`
- `AttackRequested`
- `DamageRequested`

Request rules:
- Player input in Unity is translated into request events for the core.
- Enemy behavior selection in the core also produces request events rather than directly mutating state.
- Requests must include enough context to identify the acting unit, target position or target unit, and current turn ownership.

### Resolution Phase

Resolution is the only phase that mutates combat state.

Resolution rules:
- Validate that the acting unit is alive.
- Validate that it is currently that side's turn.
- Validate remaining player actions when the acting unit is the player.
- For movement:
  - check board bounds
  - check occupancy
  - check orthogonal 1-tile movement
- For attacks:
  - check that the target exists
  - check that the target is alive
  - check orthogonal adjacency
- For enemy movement:
  - compare vertical and horizontal moves that reduce distance
  - prefer vertical movement first when both reduce distance
  - if vertical is not valid, evaluate horizontal
- For damage:
  - subtract 1 health from the valid target
  - if health reaches 0, mark the unit as dying and complete death handling before any later action resolves
- On player success:
  - spend exactly 1 player action
- On failure:
  - emit failure events
  - do not spend an action
  - do not change board or health state

### Result Phase

The result phase emits the observable outcomes of successful or failed resolution.

Expected result events:
- `TurnStarted`
- `TurnEnded`
- `ActionStarted`
- `ActionSpent`
- `ActionEnded`
- `UnitMoved`
- `UnitDamaged`
- `UnitDying`
- `UnitDeath`
- `UnitRemoved`
- `ActionFailed`
- `MoveBlocked`

Result ordering principles:
- A turn start event occurs before any action on that turn.
- An action start event occurs before a movement or attack resolution.
- A successful player action emits `ActionSpent` before `ActionEnded`.
- A failed action emits `ActionFailed` and then ends without spending an action.
- A lethal damage sequence emits damage before death-related events.
- `UnitRemoved` occurs after death is finalized.
- A turn end event occurs after all actions or behavior for that turn are complete.

## State Changes

Core-owned state includes:
- board dimensions
- unit positions
- unit health
- alive/dead state
- active side
- remaining player actions

Allowed state changes in this feature:
- player action count decreases by 1 on successful player actions
- unit position changes on successful movement
- unit health decreases on successful attacks
- unit alive/dead state changes when health reaches 0
- dead units are removed from board occupancy
- active side changes when turns end

Disallowed state changes:
- Unity mutating combat state directly
- requests mutating board or health before validation
- failed actions changing health, position, or action count

## Effect Interactions

No effects or items are in scope for this feature.

The design assumes:
- there are no reactive modifiers changing damage
- there are no movement-altering effects
- there are no on-death triggered abilities beyond core death events

## Failure Conditions

The following failures must resolve without state mutation:
- move destination is out of bounds
- move destination is occupied
- requested move is not a 1-tile orthogonal move
- attack target does not exist
- attack target is dead
- attack target is not adjacent
- acting unit has no remaining player actions
- acting unit is dead
- acting unit is not allowed to act during the current turn

Failure event rules:
- every failed action emits `ActionFailed`
- blocked movement also emits `MoveBlocked`
- failed actions do not spend an action

## Logging

The combat loop should produce an event log that is sufficient to verify:
- player turn begins before enemy turn
- player actions are spent only on successful actions
- movement success and movement failure are distinguishable
- attacks and damage are recorded in sequence
- death and removal occur in the correct order
- enemy behavior happens after the player turn

The event log is a primary verification surface for this feature.

## System Impact

This feature establishes the foundational combat contracts for later systems:
- board occupancy rules
- turn ownership
- action economy
- attack and damage flow
- death and removal flow
- enemy baseline behavior

Later combat features should build on these contracts rather than bypass them.

Open design limits for future phases:
- combat win/loss resolution is not finalized here
- multi-enemy ordering is deferred
- specialized failure events for non-movement failures are deferred
