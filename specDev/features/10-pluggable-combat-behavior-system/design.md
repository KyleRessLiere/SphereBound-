# Design: [10] Pluggable Combat Behavior System

## Flow

The behavior system adds a deterministic decision layer inside the pure C# core.

At the beginning of a unit's turn:

1. The core identifies the acting unit.
2. The core asks the unit's assigned behavior to inspect read-only combat state.
3. The behavior returns an action intent or an explicit pass or end-turn intent.
4. The core routes that intent into the existing validation and resolution path.
5. Existing gameplay events continue to drive movement, ability use, damage, death, failure, and turn progression.

The behavior layer decides intent only. The core remains the only system that validates and mutates combat state.

## Behavior Ownership

- Units support an assigned pluggable behavior reference.
- The assigned behavior may come from:
  - the unit's default configuration
  - scenario setup
  - a manual-control adapter
- The first design priority is compatibility with deterministic scenario setup, so behaviors must be easy to assign during unit initialization.
- Manual control should fit the same pluggable behavior model rather than remaining a separate conceptual action source.

## Read-Only Decision Context

Behaviors evaluate against a read-only combat context that exposes only the state needed for deterministic decision-making, including:

- board dimensions and occupancy
- unit positions
- unit health and alive state
- turn side
- remaining actions
- available abilities on the acting unit

This context must not expose write paths to combat state.

## Intent Model

Behavior output is represented as reusable action intents.

The initial intent categories are:

- move intent
- use-ability intent
- pass or end-turn intent

Intent payloads describe desired action data only, such as:

- acting unit id
- target tile
- target unit
- chosen ability id
- optional direction or shape-driving data

Intent payloads do not contain gameplay mutation logic.

## Event Flow

### Behavior Decision Phase

- Beginning of turn triggers behavior evaluation for the acting unit.
- The core emits a behavior-decision log event or equivalent observable record describing:
  - acting unit
  - behavior type
  - selected intent

### Core Resolution Phase

- The selected intent is translated into the existing core action request path.
- Existing request events remain authoritative for gameplay resolution.
- Existing validation determines whether the requested move or ability is legal.

### Result Phase

- Existing result and failure events continue to describe what actually happened.
- If a behavior-selected intent is invalid, normal failure events are emitted.
- If the behavior selects pass or end turn, turn progression continues through the normal turn system.

## Initial Behavior Definitions

### PassTurnBehavior

- Produces a deterministic pass or end-turn intent at the start of the acting unit's turn.
- Contains no gameplay rules beyond selecting that intent.

### MoveTowardTargetBehavior

- Selects a move intent toward a configured target unit.
- Uses deterministic distance reduction rules.
- Uses explicit tie-breaking when more than one move would reduce distance.
- Relies on core validation for blocked, occupied, or invalid tiles.

### SpamAbilityBehavior

- Selects a configured ability each time the unit acts.
- Produces a use-ability intent using deterministic target-selection rules.
- Does not decide combat outcome, only the chosen ability intent.

### ScriptedBehavior

- Returns a predefined sequence of intents in a strict deterministic order.
- Intended for scenario testing and preset combo playback.
- When steps are exhausted, it falls back to a deterministic terminal behavior such as pass or end turn.

### ManualBehavior

- Represents external/manual choice through the same pluggable behavior interface.
- Allows future runtime UI, debug controls, or scenario tools to inject chosen intents without creating a separate execution model.
- Manual behavior still routes through core validation and event resolution.

## Deterministic Combo And Scenario Support

- The system must support initializing units with predefined behaviors during scenario setup.
- This allows future preset combo scenarios where:
  - unit definitions are chosen
  - behaviors are assigned
  - the simulation plays out without live Unity input
- Scripted and manual-pluggable behaviors must fit the same deterministic scenario execution model.

## State Changes

Behaviors do not directly change:

- unit positions
- health
- alive state
- action counts
- board occupancy
- turn state

Only the existing combat resolution layer changes those values after validating an intent.

## Failure Handling

- Invalid behavior-generated intents fail through the same failure path as manual actions.
- Repeated invalid behavior choices remain deterministic.
- Dead units do not execute behavior decisions.
- If a behavior cannot produce a valid intent, the core must fall back to deterministic pass or end-turn handling rather than stalling.
- Scripted behaviors that run out of steps must resolve predictably.

## Logging

Behavior decisions must be visible alongside existing event logs.

The design requires logs to show:

- acting unit
- behavior identity
- selected intent

Downstream logs must still show the normal gameplay event sequence after that decision.

Behavior logging must remain:

- deterministic
- ordered
- compatible with current scenario and runner output

## System Impact

This feature extends the pure core architecture without introducing a second combat engine.

It affects:

- unit initialization and configuration
- automated scenario execution
- future AI expansion
- future manual-control abstraction
- logging and verification coverage

It must not require Unity to own behavior logic or resolve gameplay rules.

## Deferred Items

The following are intentionally out of scope for this phase:

- randomness-driven AI
- utility scoring systems
- behavior trees
- planning systems
- visual behavior editors
- Unity-side behavior ownership
- complex reactive mid-turn replanning beyond the defined beginning-of-turn trigger
