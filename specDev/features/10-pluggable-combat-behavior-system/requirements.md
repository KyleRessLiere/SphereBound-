# Requirements: [10] Pluggable Combat Behavior System

## Feature Interpretation

Introduce reusable, deterministic combat behavior definitions that inspect read-only combat state and choose action intents for the core to resolve, without embedding or bypassing gameplay rules.

## Goal

Allow units to select actions through reusable, deterministic behavior definitions that operate on read-only game state and produce action intents for the core to resolve.

## Behavior Definitions

- Units may have an assigned behavior definition.
- A behavior definition is responsible for deciding what action a unit intends to perform.
- Behavior definitions must not directly mutate game state.
- Behavior definitions must not directly apply movement, damage, healing, death, or effects.
- Behavior definitions must only produce action intents that are resolved by the core system.
- Behavior definitions must be reusable across multiple units or scenarios when configured with the same behavior parameters.

## Read-Only Game State Access

- Behavior definitions must have access to relevant combat state needed to decide an intent, including:
  - board layout
  - unit positions
  - unit health
  - alive or dead state
  - turn side
  - remaining available actions
  - available abilities on the acting unit
- This behavior-facing state must be treated as read-only.
- Behavior definitions must not modify board state, unit state, or gameplay state directly.
- All state changes must occur through action intents that later flow through the core event system.

## Determinism

- Behavior decisions must be deterministic given the same input state.
- No randomness should be introduced at this stage unless explicitly controlled and testable in a future feature.
- If multiple valid choices exist, the behavior definition must use documented tie-breaking rules so the same state yields the same chosen intent.

## Action Intents

- Behavior definitions must produce action intents such as:
  - move intent
  - use ability intent
  - end turn or pass intent
- An intent describes the desired action but does not execute gameplay rules.
- Intent execution must continue to use the same validation and resolution path already used by manual/debug/runtime UI actions.

## Supported Initial Behaviors

The first version must define requirements for at least:

### PassTurnBehavior

- Produces no action or explicitly ends turn.
- Must be deterministic and have no side effects outside the intent it returns.

### MoveTowardTargetBehavior

- Moves toward a target unit using deterministic rules.
- Must use deterministic tie-breaking when multiple moves reduce distance.
- Must not bypass occupancy, board bounds, or movement validation.

### SpamAbilityBehavior

- Repeatedly selects and attempts to use a specific configured ability.
- Must not directly apply the ability.
- Must return an ability-use intent that the core resolves normally.

### ScriptedBehavior

- Executes a predefined sequence of intents for deterministic scenario testing.
- Must preserve sequence order exactly.
- Must have deterministic behavior when the script runs out of remaining steps.

## Player And Enemy Use

- Both player units and enemy units must support assigned behaviors.
- Existing enemy behavior must be representable using behavior definitions.
- Manual input should remain conceptually compatible with behavior-generated intents.
- The behavior system must not conflict with existing Unity debug controls or runtime UI controls.
- The behavior system must allow the project to automate turns or actions without requiring Unity input.

## Scenario Testing

- The behavior system must support deterministic scenarios such as:
  - player repeatedly uses an ability while enemy passes
  - enemy moves toward player while player passes
  - ability hits when in range
  - ability fails when out of range
  - multi-turn automated execution without manual input
- Scenario runner coverage must be able to use behaviors as a deterministic action source for automated scenario execution.

## Event Flow

- Behavior-selected intents must flow through the existing gameplay systems and event path.
- Downstream event coverage must still include:
  - turn events
  - action events
  - ability events
  - movement events
  - damage events
  - failure events
- No new combat execution path should be created just for behaviors.
- Behavior selection itself must be observable without becoming a second rules engine.

## Logging

- Behavior decisions must be visible in logs.
- Logs must show which behavior selected which intent.
- Downstream gameplay events must continue to log normally.
- Logs must remain deterministic and ordered.
- Behavior-decision logging must not replace or suppress existing gameplay event logging.

## Constraints

- Do not move gameplay rules into behaviors.
- Do not move gameplay rules into Unity.
- Do not bypass core validation.
- Do not introduce separate combat execution logic.
- Maintain compatibility with:
  - scenario runner
  - verifier suites
  - Unity bridge
  - runtime UI
- Keep this feature inside the pure C# gameplay core unless a Unity-facing observation hook is explicitly required later.

## Interactions

- This feature must integrate with:
  - unit definitions
  - movement capability metadata
  - ability definitions
  - turn system
  - action spending
  - board positioning
  - attack and damage resolution
  - event logging
  - scenario runner automation

## Edge Cases

- Behavior selects invalid move.
- Behavior selects ability without sufficient actions.
- Behavior selects target outside board.
- Behavior selects no valid action.
- Unit is dead when behavior would act.
- Scripted behavior runs out of steps.
- Behavior repeatedly fails actions.
- Behavior tie-breaking must remain deterministic under all cases.
- Behavior attempts to act when it is not the unit's valid action window.
- Behavior selects an ability target or path that resolves to no valid affected units.

## Acceptance Criteria

- [ ] A unit can be assigned a behavior definition.
- [ ] A behavior can produce a move intent.
- [ ] A behavior can produce an ability intent.
- [ ] Pass behavior produces no action or ends turn deterministically.
- [ ] Spam ability behavior repeatedly selects its configured ability.
- [ ] Scripted behavior follows its configured sequence deterministically.
- [ ] Existing enemy move-toward-player behavior can be represented through the behavior system.
- [ ] Invalid behavior intents fail through the normal core validation path.
- [ ] Behavior-driven scenarios can run without Unity input.
- [ ] Logs show behavior decisions and resulting gameplay events in order.
- [ ] Existing verifier suites still pass.

## Ambiguities

- It is not yet finalized where behavior assignment should live first:
  - on unit definitions
  - on unit instances
  - in scenario configuration
  - in a separate controller layer
- It is not yet decided whether manual input should be modeled as a behavior in this feature or only remain conceptually compatible for now.
- The exact target-selection rule for `SpamAbilityBehavior` is not finalized when multiple legal targets exist.
- It is not yet finalized whether `PassTurnBehavior` should explicitly emit an end-turn intent or simply produce no further action intent.
- The exact log format and verbosity for behavior-decision logging is not yet finalized.
- It is not yet finalized whether behavior execution should happen once per unit turn, once per action window, or through another deterministic cadence.
