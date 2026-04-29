# Tasks: 01 Core Scenario Runner

## Phase 1 - Core

[x] Define the core scenario runner state and contract types, including scenario definition, scenario step, scenario result, and verification result structures.
Acceptance:
- Scenario contracts can describe predefined scenario input without Unity dependencies.
- Result contracts can represent ordered events, human-readable logs, and verification outcomes.

[x] Implement fresh scenario-state creation so each scenario run starts from an isolated core gameplay state.
Acceptance:
- Each scenario run uses a new gameplay state instance.
- Running one scenario does not mutate the starting state of another scenario.

[x] Implement scenario execution flow that runs predefined steps in deterministic order through the existing core combat loop.
Acceptance:
- Scenario steps execute in a stable, predefined order.
- The runner uses existing core gameplay actions rather than bypassing gameplay resolution.

## Phase 2 - Events

[x] Implement event capture for scenario execution so emitted combat-core events are collected in exact order.
Acceptance:
- The runner captures gameplay events in the same order they are emitted.
- Captured events include turn, action, movement, damage, death, and failure events when they occur.

[x] Implement human-readable log formatting for captured events using stable, deterministic output.
Acceptance:
- Log lines are readable and preserve event order.
- Log lines include relevant values such as unit ids, positions, damage, health changes, and failure reasons.
- Repeated runs of the same scenario produce identical log output.

[x] Align scenario runner event observation with the actual combat core contracts, including use of `AttackRequested` rather than `AttackStarted` unless the core spec changes later.
Acceptance:
- The runner observes the current approved combat-core event names.
- No duplicate or invented gameplay event contracts are introduced by the runner.

## Phase 3 - Edge Cases

[x] Implement failure-safe scenario execution so invalid gameplay actions are logged and returned without crashing the runner.
Acceptance:
- Invalid actions produce readable failure output.
- Scenario execution infrastructure remains stable when gameplay failures occur.
- Failure events are preserved in the ordered log.

[x] Implement malformed-scenario and unsupported-step handling with explicit runner-level error reporting.
Acceptance:
- Invalid scenario definitions are reported clearly.
- Runner-level failures are distinguishable from gameplay-level failures.

[x] Support multiple predefined scenarios that can run independently without shared runtime state or cross-run contamination.
Acceptance:
- More than one scenario can be executed by the runner.
- Running scenarios back-to-back produces independent results and logs.

## Phase 4 - Verification

[x] Implement scenario verification rules for expected results such as final health, position, alive/dead state, and event-log presence/order checks.
Acceptance:
- A scenario can assert expected outcome values.
- Verification can detect mismatches in final state and required event ordering.

[x] Add deterministic verification coverage for successful scenarios, failing gameplay scenarios, and multi-scenario independence.
Acceptance:
- Verification proves repeated runs of the same scenario yield identical results.
- Verification covers success, gameplay failure, and isolation cases.
- No Unity dependencies are required to run verification.

[x] Update this task file during execution so only one task is marked in progress at a time and completed tasks are reflected here.
Acceptance:
- Task status remains aligned with actual execution progress.
- The file remains the source of truth for implementation progress.
