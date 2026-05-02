# Tasks: 08 Ability And Player Definition System

## Phase 1 - Core

[x] Define reusable player-definition, movement-capability, and ability-definition models in the pure C# core.
Acceptance:
- Player units can be created from reusable definitions.
- Movement remains a definition-backed core capability rather than a normal ability entry.
- Ability definitions are reusable and independent from Unity.

[x] Define deterministic targeting, target-rule, and affected-tile data structures for ability resolution.
Acceptance:
- The core has a clear way to represent targeting modes and resolved affected tiles.
- The model supports current single-target behavior and future richer shapes without hardcoded class logic.

[x] Keep the new definition layer compatible with the current combat state and scenario factory flow.
Acceptance:
- Existing unit initialization can be migrated to definitions without changing gameplay behavior.
- The design remains purely core-owned and deterministic.

## Phase 2 - Events

[x] Implement player initialization from definitions and migrate the current basic player attack into an ability definition.
Acceptance:
- The current player is created from a definition.
- The existing basic attack is represented as an ability definition and still behaves the same.

[x] Implement ability-use resolution that validates target input, resolves affected tiles, and routes damage through the existing event flow.
Acceptance:
- Successful ability damage still emits the normal downstream damage events.
- Invalid ability use fails cleanly without mutating authoritative state.

[x] Implement ability-caused forced movement through the existing movement-validation and movement-event path.
Acceptance:
- Ability-caused movement uses the normal movement request/resolution behavior.
- Invalid forced movement fails through the same movement rules.

## Phase 3 - Edge Cases

[x] Handle insufficient actions, dead actors, invalid targets, out-of-bounds targets, and empty/no-hit resolution cases without breaking later actions.
Acceptance:
- These failures are deterministic and emit clean failure results.
- Later valid actions still work after failures.

[x] Preserve current normal movement and enemy move-toward-player behavior while introducing the definition system.
Acceptance:
- Existing movement behavior still works.
- Existing enemy behavior still works without Unity-owned rules.

[x] Keep the system extensible for future classes, shapes, and multi-unit effects without one-off class logic.
Acceptance:
- The implementation does not hardcode one future class or attack pattern into turn logic.
- The structure clearly supports future expansion.

## Phase 4 - Verification

[x] Add verifier coverage for player initialization from definitions, ability cost handling, deterministic affected-tile resolution, damage flow, and forced movement flow.
Acceptance:
- The new definition system is covered by the pure verifier path.
- Existing combat/debug verifier expectations remain valid.

[x] Re-run the permanent in-repo core runner to confirm all existing suites still pass after the definition-system additions.
Acceptance:
- `dotnet run --project Tools/CoreRunner/CoreRunner.csproj` still passes.
- No Unity dependency is introduced into the out-of-Unity runner path.

[x] Update this task file and the holistic current-state document during execution so the new feature is reflected accurately.
Acceptance:
- Task status matches implementation progress.
- `specDev/current-state.md` reflects the new ability/player-definition foundation once the feature is complete.
