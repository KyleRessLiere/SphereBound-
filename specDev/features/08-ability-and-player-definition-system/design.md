# Design: 08 Ability And Player Definition System

## Flow

Ability And Player Definition System introduces reusable player-definition and ability-definition models into the pure C# combat core.

High-level flow:
- Combat setup creates a player unit from a reusable player or class definition.
- The player definition provides:
  - base stats
  - action economy values
  - available abilities
  - movement capability metadata
- When the player or another valid unit uses an ability:
  - an ability-use request enters the core
  - targeting is validated
  - affected tiles are resolved deterministically
  - gameplay payloads such as damage, healing, or forced movement are translated into existing request/resolution behavior
  - downstream result events are emitted in the normal event order

Movement remains a core action or capability defined on the unit/player definition side rather than being treated as a normal class ability in the initial design.

## Event Flow

### Request Phase

The request phase accepts definition-backed action intent without directly mutating gameplay state.

Request-phase responsibilities:
- accept ability-use intent for a specific acting unit and ability definition
- accept any needed target input, such as target tile, target unit, or directional context
- identify the acting unit's player/class definition and ability set
- identify movement capability separately from the reusable ability list

Request-phase rules:
- abilities must be referenced through reusable definitions rather than hardcoded attack branches
- movement remains a definition-backed core action/capability, not a normal ability entry in the initial feature version
- Unity may trigger the resulting requests, but request meaning is owned by the core

### Resolution Phase

Resolution remains entirely in the pure C# core.

Core responsibilities during resolution:
- validate the acting unit is alive and allowed to act
- validate available actions and ability cost
- validate targeting input
- resolve affected tiles deterministically from the definition
- resolve payloads such as:
  - damage
  - healing
  - forced movement
- route damage and movement through existing combat resolution/event pathways where possible

Resolution rules:
- ability definitions describe intent and targeting rules
- resolution remains responsible for authoritative state changes
- forced movement caused by an ability must request movement through the existing event-driven movement path
- invalid ability use must fail without mutating board, health, or action state
- action cost is only spent once the ability successfully resolves according to core rules

### Result Phase

The result phase emits the observable ability outcomes in the normal event flow.

Expected result behavior:
- ability use is visible in ordered logs
- damage still appears through existing damage-related events
- movement caused by an ability still appears through existing movement-related events
- existing board and debug surfaces can observe the results without special-case Unity gameplay logic

Result rules:
- the ability system must compose into the existing event pipeline rather than inventing a second gameplay result mechanism
- Unity remains an observer/trigger layer only

## State Changes

Core-owned state includes:
- player/class definitions
- unit instances referencing or being created from definitions
- ability definitions
- movement capability metadata
- targeting and affected-tile resolution data
- authoritative board, turn, health, and action state

Allowed state changes:
- initialize units from reusable definitions
- spend actions for successful ability use
- change health through ability payload resolution
- change positions through ability-caused forced movement

Disallowed state changes:
- Unity-authored gameplay mutation
- hardcoded special-case attacks in turn logic
- bypassing existing event-based movement or damage flow

## Effect Interactions

This feature establishes a foundation for future combat classes and richer ability payloads.

Current scope:
- damage and forced movement must fit naturally
- healing and broader effect payloads must be representable in the definition model even if not all are fully exercised immediately
- future multi-target or area abilities must fit into the same shape and targeting model

Effect-related design rule:
- ability definitions describe what an ability can do
- resolution decides what actually happens under board, turn, occupancy, and target-validity rules

## Failure Conditions

Gameplay failure cases include:
- insufficient actions
- invalid target
- target outside board
- shape partially or fully outside board
- no valid affected units
- blocked or occupied movement results
- dead unit attempting to use an ability

Failure-handling rules:
- failures must emit normal failure-oriented results without mutating authoritative state
- failed movement payloads caused by an ability must fail through the same movement-validation model
- later valid actions must still work after a failed ability attempt

## Logging

Logging remains an important verification surface.

Logging rules:
- logs must identify ability use in order with downstream events
- damage caused by abilities must still appear through existing damage event flow
- ability-caused movement must still appear through existing movement event flow
- existing debug tooling should not need to own special gameplay rules to understand ability outcomes

Movement-specific logging rule:
- movement as a base capability should continue to behave like the current movement flow
- movement does not need to be represented as a normal class ability in logs for this first design

## System Impact

This feature changes the combat foundation from hardcoded player attack assumptions to reusable definitions.

It enables:
- future combat classes
- reusable ability sets
- richer targeting patterns
- future payload expansion without rewriting turn logic

It must not:
- move gameplay rules into Unity
- break current combat behavior
- collapse movement and abilities into a single overgeneralized model prematurely

Open design limits for later phases:
- whether movement should later be unified under a broader `ActionDefinition` hierarchy remains open
- whether facing direction is required immediately remains open
- whether definitions begin code-defined or data-defined remains open
- whether some "valid cast but no hit" cases should spend actions remains open
