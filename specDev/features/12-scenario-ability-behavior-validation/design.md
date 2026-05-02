# Design: [12] Scenario Ability And Behavior Validation

## Purpose

This feature adds a deterministic scenario-validation suite on top of the existing pure C# combat core.

It does not add new gameplay rules.

It uses the current:

- scenario runner
- ability-definition system
- behavior system
- verifier infrastructure
- ordered event logs

to validate that combat behavior remains correct through realistic multi-step scenarios.

## Validation Role

This suite sits between low-level verifier checks and higher-level manual inspection.

It exists to prove that:

- abilities behave correctly in real scenario flow
- behaviors integrate correctly with movement and attack decisions
- ordered event output remains correct across deterministic runs
- final combat state matches expected outcomes

The suite is validation infrastructure, not a gameplay feature.

## Scenario Structure

Each scenario in this suite is modeled as an existing deterministic scenario-runner definition with explicit validation expectations.

Each scenario must provide:

- a name
- initial board state
- initial unit health values
- initial unit positions
- player behavior assignment
- enemy behavior assignment
- either explicit scenario steps or behavior-driven turn-cycle steps
- expected event output or expected event subsequences
- expected final state
- explicit failure conditions

The scenario runner remains the execution mechanism.

## Scenario Categories

### 1. Basic Attack Hit

This scenario validates the existing basic attack ability in a successful adjacent-target case.

The design expects:

- the player and enemy begin adjacent
- the player uses the basic attack ability
- the ability resolves successfully
- downstream damage events fire
- enemy health is reduced by the expected amount

Validation output must confirm:

- correct event order
- no unexpected failure event
- correct final enemy health

### 2. Basic Attack Miss

This scenario validates the failure path for the same basic attack ability when the target is invalid or out of range.

The design expects:

- the enemy begins in a non-valid target position
- the player attempts the same basic attack ability
- the request fails through existing validation
- no damage is applied

Validation output must confirm:

- failure-oriented events are present
- damage events are absent
- unit positions and health remain unchanged except for allowed action-state behavior

### 3. Ability Shape Validation

This scenario validates a multi-tile ability that already exists in the current definition-backed player kit, such as the front-cross style test ability.

The design expects:

- the player uses a multi-tile ability
- enemy placement and empty tiles are chosen so intended and unintended tiles are distinguishable
- only definition-backed affected tiles are treated as relevant

Validation output must confirm:

- the ability request is emitted correctly
- affected tile interpretation is deterministic
- no unintended diagonal or unrelated tiles are treated as hits

This validation may rely on:

- event output
- final state
- explicit affected-tile expectation data

## 4. Multi-Turn Combat Resolution

This scenario validates repeated successful ability use across multiple turns until the enemy dies.

The design expects:

- enough health on the target to require multiple successful turns
- deterministic turn progression
- repeated use of the intended ability
- correct death and removal chain once health reaches zero

Validation output must confirm:

- expected number of successful resolutions before death
- ordered death-chain events
- final absence of the dead unit from the board

### 5. Behavior + Movement Interaction

This scenario validates behavior-driven enemy action selection in a real combat board state.

The design expects:

- the enemy uses a move-toward-player behavior
- the player either passes or otherwise remains consistent
- the enemy approaches deterministically
- once adjacent, the enemy transitions from movement intent to attack intent

Validation output must confirm:

- `BehaviorIntentSelected` appears before downstream action execution
- movement destinations follow the current deterministic rules
- the attack transition happens only when adjacency becomes valid

### 6. Forced Movement Validation

This scenario is supported by the design and may be required or optional depending on the final requirement decision.

The scenario expects:

- an ability with forced movement payload is used
- the target is moved only through the normal movement-validation path
- blocked, occupied, or out-of-bounds destinations remain constrained by existing rules

Validation output must confirm:

- movement-related events are emitted when valid
- failure-related events are emitted when forced movement cannot resolve
- no direct position mutation bypass occurs

## Execution Model

The suite uses the existing scenario runner and does not create a second simulation path.

Execution flow:

1. Build the scenario with initial state and definitions.
2. Run the scenario through the existing runner.
3. Capture ordered log lines from the scenario run.
4. Validate expected event behavior and final state using existing scenario verification patterns.
5. Emit verifier log files through the existing colocated verifier-log infrastructure.

## Event Validation Strategy

This design supports two event-validation levels:

### Exact Sequence Validation

Used when the entire emitted event flow should be stable and small enough to validate completely.

Good fit for:

- basic attack hit
- basic attack miss
- small deterministic movement transitions

### Critical Subsequence Validation

Used when the important correctness target is the presence and order of a specific event chain rather than every emitted line.

Good fit for:

- behavior-driven multi-turn scenarios
- longer death-resolution chains
- optional forced-movement edge cases

The suite should prefer exact validation when practical, but it must remain explicit about which mode each scenario uses.

## Final-State Validation

Each scenario must validate final state explicitly.

Relevant state checks include:

- unit alive/dead state
- unit position
- unit health
- unit removal from board
- active turn where meaningful
- remaining actions where meaningful

Final-state validation is required even when event validation is strong, because logs alone are not enough to guarantee terminal state correctness.

## Failure Conditions

Each scenario must define explicit failure conditions.

Examples include:

- missing required events
- unexpected damage application
- incorrect affected-tile resolution
- incorrect behavior transition
- incorrect final health or position
- death/removal chain not completing

These failures should integrate into the existing verifier reporting style rather than inventing a separate failure-report system.

## Behavior Assignment Design

Scenarios must reuse the existing pluggable behavior system.

Behavior assignment may include:

- pass behavior
- scripted behavior
- move-toward-target behavior
- spam-ability behavior
- manual-compatible placeholders only when scenario setup needs them, not Unity input

The suite must not add new behavior execution paths.

## Ability Assignment Design

Scenarios must reuse existing definition-backed abilities wherever practical.

This includes:

- current basic attack ability
- current test multi-tile abilities
- current forced-movement test ability when the forced-movement scenario is enabled

The suite must not require bespoke scenario-only combat rules.

## Logging Design

The suite must integrate with the existing verifier log output model.

Because these are combat-state progression scenarios, their verifier logs must be combat-flow logs that include:

- initial board
- ordered event stream
- meaningful progression detail
- final board

Behavior-driven scenarios must preserve `BehaviorIntentSelected` lines in the emitted logs.

Scenario-specific log files should remain easy to inspect in the verifier-specific `.logs` directory.

## Verifier Placement

This feature should integrate into the current out-of-Unity verifier stack as a dedicated validation suite rather than folding everything into unrelated existing verifiers.

The dedicated suite should:

- run from the permanent core runner
- produce pass/fail output like the other verifier suites
- emit colocated verifier log files

This keeps scenario-validation concerns grouped and discoverable.

## Determinism

Determinism requirements for this feature:

- same scenario definition produces the same event order
- same scenario definition produces the same final state
- same verifier run overwrites the same scenario log file
- behavior-driven decisions remain stable given the same initial state

## Out Of Scope

This feature does not include:

- new gameplay abilities
- new behavior types unless already required by existing systems
- Unity-specific scenario execution
- UI or visualization work
- changes to combat rules for validation convenience
- probabilistic or random scenario coverage
