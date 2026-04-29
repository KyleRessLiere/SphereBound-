# Feature: 01 Core Scenario Runner

## REQUIREMENTS SPEC

## Goal

Provide a deterministic way to run gameplay scenarios outside Unity and verify behavior using logs.

## Feature Interpretation

Core Scenario Runner defines a pure C# scenario-execution layer that can drive predefined gameplay scenarios, observe the existing combat event stream, produce readable ordered logs, and verify expected scenario outcomes without depending on Unity.

## Behavior

- The system can run predefined gameplay scenarios.
- Each scenario executes a sequence of gameplay actions against core gameplay state.
- The system outputs a readable event log for the scenario run.
- The system can validate expected scenario outcomes after execution.
- The same scenario input must produce the same event order and same final results on repeated runs.
- Multiple scenarios can be executed independently without sharing mutable runtime state.

## Rules

- The runner must execute entirely in pure C# with no Unity dependencies.
- The runner must use the existing gameplay event system rather than bypassing it.
- The runner must observe and log major events in execution order.
- The runner must support more than one scenario definition.
- The runner must support verification of final results such as health, position, alive/dead state, and other scenario-relevant outcome values.
- The runner must be deterministic: the same initial state and action sequence must produce the same log and same final outcome.
- The runner must not introduce new gameplay rules.
- The runner must not directly mutate gameplay state outside the normal core gameplay flow.

## Edge Cases

- A scenario containing invalid actions must log failure events rather than crashing.
- Scenario execution must continue to produce readable logs even when failures occur.
- An invalid scenario step must not corrupt later scenario state.
- Multiple scenarios must not share runtime state between runs.
- A failed scenario verification must produce enough information to diagnose what diverged.
- A scenario that ends in unit death must still log the full death-related sequence in order.

## Constraints

- No Unity types may be used, including `MonoBehaviour`, `GameObject`, `Transform`, `Animator`, scene references, or other Unity runtime objects.
- The runner must not introduce a new gameplay system separate from the existing combat core.
- The runner must reuse the existing event-driven gameplay flow and logging approach.
- The runner must not alter core gameplay logic to make scenarios work.
- The runner must act as an observer/executor over existing core behavior, not as an alternate rules engine.

## Interactions

- The runner uses the core combat loop feature as the gameplay system being exercised.
- The runner uses the existing event system as the source of execution history.
- The runner uses board and unit state as scenario inputs and verification outputs.
- The runner uses combat/event logging as the main debugging and validation surface.

## Event Hooks

The runner must observe and log these events in sequence when they occur:

- `TurnStarted`
- `TurnEnded`
- `ActionStarted`
- `ActionSpent`
- `ActionEnded`
- `MoveRequested`
- `UnitMoved`
- `AttackStarted`
- `DamageRequested`
- `UnitDamaged`
- `UnitDying`
- `UnitDeath`

The runner must also log relevant failure events when scenarios trigger invalid behavior.

## Logging Requirements

- Logs must be human-readable.
- Logs must show event order clearly.
- Logs must include relevant values such as unit ids, positions, damage amounts, health changes, and failure reasons where applicable.
- Logs must be consistent across repeated runs of the same scenario.
- Logs must be usable for debugging and validation.
- Logs must preserve enough information to compare expected and actual behavior.

## Acceptance Criteria

- [ ] A scenario can be defined and executed outside Unity.
- [ ] Running a scenario produces a full event log.
- [ ] Event logs reflect the correct order of gameplay events.
- [ ] Movement, attack, damage, and death events are logged when they occur.
- [ ] Logs are deterministic across repeated runs.
- [ ] Scenario results can be verified, including examples such as enemy death, health totals, and final positions.
- [ ] Multiple scenarios can run independently without shared state.
- [ ] Invalid actions produce failure logging instead of crashing execution.
- [ ] No Unity dependencies are used.

## AMBIGUITIES

- Scenario definition format is not yet fixed: hardcoded code-driven scenarios versus data-driven scenario files.
- Expected-result definition format is not yet fixed.
- It is not yet defined whether assertions are built into the runner or performed by an external verifier layer.
- Scenario selection and execution flow are not yet defined.
- The request references `GameEventSystem`, but that exact named abstraction is not currently defined in the existing specs.
- The requested observed event list includes `AttackStarted`, but the current core combat loop specs define `AttackRequested` and `ActionStarted` instead.
- It is not yet explicit whether logs must include failure events beyond the listed major events in every failing case.
