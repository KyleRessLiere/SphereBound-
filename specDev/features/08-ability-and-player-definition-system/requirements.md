# Feature: 08 Ability And Player Definition System

## REQUIREMENTS SPEC

## Goal

Create a reusable player-definition and ability-definition foundation in the pure C# core so future combat classes and attack patterns can be represented without hardcoding specific attacks into turn logic.

## Feature Interpretation

Ability And Player Definition System replaces the current assumption that the player's attack behavior is a one-off hardcoded combat action with a reusable definition-driven model.

This feature must establish:

- reusable player or class definitions
- reusable ability definitions
- deterministic targeting and affected-tile resolution
- ability execution through the existing event system

The first version must still support the current basic attack behavior while opening the system for future classes and shapes.

## Player Definitions

- A player instance should be created from a reusable player or class definition.
- Player definitions should support base stats such as:
  - starting health
  - actions per turn
- Player definitions should support a list of available abilities.
- The system should allow future player classes to be introduced without rewriting core combat rules.
- Player definitions should remain part of the pure C# gameplay core.

## Ability Definitions

Each ability definition should support:

- name
- action cost
- targeting mode
- affected tile pattern or shape
- payload such as damage, healing, forced movement, or other future gameplay effect data
- valid target rules
- whether it can target:
  - self
  - units
  - enemies
  - empty tiles
  - specific board locations

Ability definitions must be reusable and not depend on Unity-specific types.

## Targeting Modes

The system should support or be clearly designed to support:

- self
- adjacent unit
- specific tile
- directional shape
- line
- area
- all units in the affected shape

The targeting model must remain deterministic and compatible with the current board-based combat rules.

## Ability Shapes

Abilities should be able to describe tiles relative to:

- player position
- facing direction
- selected target tile

The shape model should be capable of representing patterns such as:

- adjacent single target
- two tiles forward
- line of 3
- cross around player
- small area around selected tile

The first feature version does not need to implement every possible future shape in gameplay behavior, but the definition system must be designed so those shapes fit naturally.

## Event Flow

Ability use must flow through the existing event system.

Expected behavior:

- ability use starts
- targeting is validated
- affected tiles are resolved
- movement, damage, healing, or future effects are requested through existing request-style event flow
- an ability may cause movement of the player, an enemy, or another valid affected unit by requesting movement through the normal event system
- results are emitted through normal result events
- action cost is spent only when the ability successfully resolves according to core rules

The feature must not bypass the current request -> resolve -> result architecture.

## Rules

- Do not hardcode player attacks directly into turn logic.
- Do not create one-off gameplay logic for specific future classes.
- Do not move gameplay rules into Unity.
- Existing deterministic combat behavior must remain valid.
- Existing basic attack behavior must be representable as an ability definition.
- Ability targeting and affected-tile resolution must be deterministic.
- The ability system must remain a core gameplay foundation rather than a presentation system.

## Constraints

- Do not implement visuals, UI targeting, animations, VFX, or audio.
- Keep gameplay rules in the pure C# core.
- Unity may observe or trigger the resulting actions only through existing bridge/debug pathways as needed.
- Existing core verifier suites must continue to pass.
- The feature should preserve compatibility with the current combat loop while generalizing player attack behavior.

## Interactions

This feature must integrate with:

- turn system
- action spending
- board positioning
- movement resolution
- attack and damage flow
- event logging
- Unity debug bridge only where observation or triggering already exists

The new system must fit into the current combat architecture instead of replacing it with a Unity-owned alternative.

## Edge Cases

The requirements must account for:

- ability use with insufficient actions
- invalid target selection
- no affected units
- target outside the board
- shape partially outside the board
- occupied or blocked tiles
- ability-caused movement into invalid, blocked, or occupied tiles
- dead unit using an ability
- ability patterns that can later hit multiple units, even if the first version validates only a small subset
- deterministic handling of abilities that affect tiles but do not hit a valid unit

## Acceptance Criteria

- [ ] A player can be initialized from a reusable definition.
- [ ] A player receives abilities from its definition.
- [ ] The current basic attack can be represented as an ability definition.
- [ ] Ability action cost is respected.
- [ ] Invalid ability use fails cleanly.
- [ ] Ability targeting resolves affected tiles deterministically.
- [ ] Ability damage flows through existing damage events.
- [ ] Ability-caused movement flows through existing movement events.
- [ ] Logs show ability use and downstream events in order.
- [ ] Existing movement and attack behavior still works.
- [ ] No Unity gameplay dependency is introduced.

## AMBIGUITIES

- It is not yet defined whether movement should also become an ability or remain a separate core action.
- It is not yet defined whether facing direction is required immediately or only designed for future support.
- It is not yet defined whether ability and player definitions are code-defined first or data-defined first.
- It is not yet defined whether action cost is spent strictly after successful resolution or after a narrower "valid cast" stage.
- It is not yet defined whether missed or no-hit abilities should spend actions.
