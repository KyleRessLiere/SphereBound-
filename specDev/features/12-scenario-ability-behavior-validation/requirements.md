# Requirements: [12] Scenario Ability And Behavior Validation

## Feature Interpretation

Add a deterministic scenario-validation suite that uses the existing pure C# scenario runner, ability system, behavior system, and verifier/logging infrastructure to prove ability correctness, behavior integration, and ordered event flow through concrete combat scenarios.

## Goal

Provide a reusable scenario-based validation layer that can run deterministic combat setups and confirm:

- ability hit and miss behavior
- affected-tile correctness
- behavior-driven movement and attack transitions
- multi-turn combat resolution
- optional forced-movement behavior when included

This feature is for validation coverage, not new gameplay.

## Scenario Suite Scope

The suite must define deterministic scenarios that validate at least:

1. `Basic Attack Hit`
- An adjacent enemy is targeted by the current basic attack ability.
- Damage is applied correctly.
- Ordered damage-related events fire correctly.

2. `Basic Attack Miss`
- The enemy is not a valid target for the current basic attack ability.
- The failure path emits the expected failure-oriented events.
- No damage or unintended state change is applied.

3. `Ability Shape Validation`
- A multi-tile ability such as the current front-cross style ability is exercised.
- Only the intended tiles are treated as affected.
- No diagonal or unintended tiles are treated as hits unless explicitly part of the definition.

4. `Multi-Turn Combat Resolution`
- Repeated ability use resolves across multiple turns.
- The target enemy dies after the expected number of successful resolutions.
- The death chain fires in the expected order, including removal.

5. `Behavior + Movement Interaction`
- A behavior-driven enemy moves toward the player using the current deterministic behavior rules.
- Once adjacent, that same behavior-driven unit transitions to an attack intent.
- The movement path and attack transition remain deterministic.

6. `Forced Movement Validation` (optional but supported by the suite design)
- An ability that causes forced movement is exercised.
- Forced movement respects board bounds, occupancy, and existing movement constraints.
- Movement-triggered events remain visible in ordered logs.

## Per-Scenario Requirements

Each scenario in this suite must define:

- initial board state
- board dimensions if relevant to readability
- unit positions
- unit health values
- relevant ability definitions or chosen existing abilities
- assigned player behavior
- assigned enemy behavior
- ordered expected event sequence or explicitly defined event-match strategy
- expected final state
- explicit failure conditions

Each scenario must remain self-contained and must not depend on state from any other scenario.

## Scenario Data Expectations

Each validation scenario must be able to express:

- the acting unit or units
- whether actions are driven by scripted steps, behaviors, or mixed validation flow
- the expected number of turns or turn transitions
- whether the scenario is expected to succeed or intentionally trigger a failure path

The scenario suite may use:

- existing basic attack definitions
- existing multi-tile test abilities already present in the definition system
- existing behavior definitions
- scenario-assigned scripted or pass behaviors

The suite must not require new gameplay systems to exist.

## Event And Validation Requirements

Scenario validation must confirm that the existing event system continues to produce ordered gameplay output.

The suite must be able to validate, where relevant:

- `BehaviorIntentSelected`
- turn events
- action start/end events
- ability request events
- attack request events
- damage request events
- unit damage events
- death and removal events
- movement request and movement resolution events
- failure events for invalid actions or invalid targeting

Validation must remain grounded in:

- ordered event logs
- expected final state
- affected-tile correctness where relevant

## Constraints

- Scenarios must run fully in the pure C# core.
- Scenarios must use the existing scenario runner.
- Scenarios must not introduce new gameplay systems.
- Scenarios must not rely on Unity.
- Scenarios must remain deterministic.
- Scenarios must reuse the existing ability and behavior systems.
- Scenarios must validate through event logs and final state rather than ad hoc side channels.
- This feature must not modify existing combat rules, ability rules, behavior rules, or turn rules.

## Logging Requirements

Logs for this suite must:

- include behavior decision events when behaviors participate
- include gameplay events in order
- remain deterministic across repeated runs
- be usable for debugging failures
- integrate with the existing verifier log-file infrastructure

For combat-state scenarios, log output must remain rich enough to confirm:

- initial board state
- meaningful event progression
- final board state

## Verifier Integration

This feature must integrate with the current verifier and runner stack.

The suite must:

- be runnable from the permanent in-repo core runner
- participate in existing verifier reporting conventions
- emit verifier log files into the colocated verifier-specific `.logs` directories
- remain compatible with the current pass/fail and overwrite behavior of verifier log output

## Acceptance Criteria

- [ ] Each defined scenario runs deterministically through the pure C# scenario runner.
- [ ] `Basic Attack Hit` confirms the expected damage and damage-related event flow.
- [ ] `Basic Attack Miss` confirms the expected failure flow and absence of damage.
- [ ] `Ability Shape Validation` confirms only the intended tiles are affected.
- [ ] `Multi-Turn Combat Resolution` confirms the expected number of successful turns and the correct death/removal chain.
- [ ] `Behavior + Movement Interaction` confirms deterministic move-toward-player behavior and attack transition when adjacent.
- [ ] Forced-movement validation is supported when included and respects existing movement constraints.
- [ ] Each scenario defines initial state, assigned behaviors, expected event sequence or match rule, expected final state, and failure conditions.
- [ ] Behavior decisions are visible in logs when behaviors participate.
- [ ] Existing verifier infrastructure can run the suite and emit colocated verifier log files.
- [ ] No Unity dependency is introduced.
- [ ] No gameplay-system changes are introduced.

## Ambiguities

- It is not yet finalized whether event-sequence validation must be fully exact or allow partial matching for some scenarios.
- It is not yet decided how strict log matching should be when multiple valid but currently identical downstream events exist.
- It is not yet finalized whether the new scenarios should live in one scenario catalog grouping or be split into multiple logical groups.
- It is not yet decided whether forced-movement validation is required for feature completion or treated as optional first-pass coverage.
- It is not yet finalized whether all scenarios should validate complete event streams or only the critical subsequences plus final state.
