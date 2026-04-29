# Design: 01 Core Scenario Runner

## Flow

Core Scenario Runner executes deterministic gameplay scenarios entirely in the pure C# core and produces a verification-friendly event log.

High-level flow:
- Select a predefined scenario.
- Create a fresh core gameplay state for that scenario.
- Execute the scenario's action sequence step by step through the existing gameplay core.
- Observe every emitted gameplay event in order.
- Convert the observed event stream into a readable log.
- Evaluate scenario expectations against the final gameplay state and emitted events.
- Return a scenario result containing:
  - execution success/failure
  - ordered event log
  - final verification outcome

The runner is not a second gameplay engine. It is a deterministic orchestration and observation layer over the existing core combat loop.

## Event Flow

### Request Phase

The runner drives scenarios by issuing predefined action requests into the existing gameplay core.

Expected request interactions:
- scenario steps describe gameplay intents such as move or attack
- the runner translates those steps into existing core request events
- the runner does not bypass the gameplay core by mutating board or unit state directly

Scenario request rules:
- each scenario step must identify the acting unit and requested action
- each scenario step must run against the current scenario state, not against shared static state
- request order must be stable and deterministic

### Resolution Phase

Resolution remains owned by the existing gameplay core.

Runner responsibilities during resolution:
- call the existing core systems in scenario step order
- capture the full ordered event output from each step
- continue execution after failed actions unless the scenario explicitly ends
- keep scenario state isolated to the current run

Core responsibilities during resolution:
- validate actions
- mutate state only through normal gameplay resolution
- emit success/failure events according to the combat core contracts

### Result Phase

The runner produces a scenario result after all steps complete or the scenario ends early.

Scenario result output includes:
- ordered raw gameplay events
- formatted human-readable log lines
- final gameplay state snapshot or equivalent verification data
- scenario verification result

Result ordering rules:
- logs preserve the exact gameplay event order emitted by the core
- verification happens after scenario execution is complete
- failed steps remain visible in the final event and log output

## State Changes

The runner owns no gameplay rules state of its own beyond scenario execution bookkeeping.

Runner-owned state includes:
- selected scenario definition
- current scenario step index
- captured event history
- formatted log output
- verification result data

Core-owned state includes:
- board state
- unit state
- turn state
- action counts
- damage/death/removal state

State rules:
- each scenario run starts from a fresh initial gameplay state
- scenario runs must not share mutable state
- runner bookkeeping must not alter gameplay results
- scenario verification reads final state but does not rewrite it

## Effect Interactions

No new effect system is introduced by this feature.

The runner simply observes whatever existing core gameplay events occur.

For the current scope:
- the runner targets the core combat loop feature first
- no item/effect-specific behavior is added here
- future effect features may be exercised by scenarios once they exist in the gameplay core

## Failure Conditions

The runner must handle gameplay failures without crashing scenario execution infrastructure.

Failure categories:
- invalid scenario actions that produce gameplay failure events
- verification mismatches between expected and actual outcomes
- malformed scenario definitions
- attempts to execute unsupported scenario step types

Failure handling rules:
- gameplay failures must still be logged in order
- scenario execution infrastructure must produce a readable result even when the scenario fails verification
- malformed scenarios must report why they are invalid
- failure reporting must distinguish between:
  - gameplay failure inside a valid scenario
  - runner/scenario-definition failure

## Logging

Logging is a first-class output of this feature.

Logging requirements in design terms:
- every observed event is transformed into a stable, human-readable log line
- log lines preserve event order exactly
- log lines include key values needed for debugging, such as:
  - acting unit
  - target unit
  - positions
  - damage values
  - health changes
  - failure reasons
- repeated runs of the same scenario produce the same log output

The logging pipeline should support:
- console output
- in-memory log capture for verification
- comparison against expected results or snapshots in a later task

Event naming note:
- the current combat core uses `AttackRequested` rather than `AttackStarted`
- this runner should observe the actual core event contracts unless a later approved change updates the combat spec

## System Impact

This feature adds a reusable verification harness around the pure C# gameplay core.

It enables:
- repeatable out-of-Unity scenario execution
- deterministic regression checking
- human-readable debugging of event order and gameplay outcomes
- validation of future pure-core gameplay features using shared runner infrastructure

It must not:
- replace the gameplay core
- fork gameplay rules
- introduce Unity dependencies
- become a separate source of truth for combat behavior

Open design limits for future phases:
- scenario definition format is still open
- expected-result format is still open
- selection/execution interface is still open
- exact output format for logs is still open
