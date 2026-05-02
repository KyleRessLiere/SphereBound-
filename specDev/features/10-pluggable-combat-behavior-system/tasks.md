# Tasks: 10 Pluggable Combat Behavior System

## Phase 1 - Core Contracts

[x] Define the core behavior-decision contract, read-only behavior context, and reusable action-intent models in the pure C# core.
Acceptance:
- Behaviors have a clear core-owned interface for selecting intents.
- Behavior evaluation uses a read-only view of combat state rather than mutable state objects.
- Intent models can represent move, use-ability, and pass or end-turn behavior outputs.

[x] Define unit-facing behavior assignment models that support default, scripted, and manual-pluggable behavior sources.
Acceptance:
- Units and scenarios have a clear way to reference behavior choice without Unity ownership.
- Manual control can fit the same intent-selection model later.

[x] Keep the new behavior contract layer compatible with the current combat state, unit-definition, and ability-definition system.
Acceptance:
- The contract layer does not require a second rules path.
- Existing movement and ability resolution models remain reusable targets for intent execution.

## Phase 2 - Turn Integration

[x] Integrate beginning-of-turn behavior evaluation into the core turn flow without bypassing existing validation.
Acceptance:
- Behavior selection happens at the beginning of the acting unit's turn.
- Selected intents still resolve through the normal core action path.

[x] Replace the hardcoded enemy behavior branch with behavior-driven intent selection.
Acceptance:
- Existing enemy move-toward-player behavior is representable through the new behavior system.
- No Unity-owned gameplay logic is introduced.

[x] Add support for pass, move, and use-ability intent execution through the existing request/resolution/result flow.
Acceptance:
- Behaviors can drive these intent types without a separate combat engine.
- Invalid intents fail through the normal failure path.

## Phase 3 - Edge Cases

[x] Handle dead actors, missing targets, no valid actions, exhausted scripts, and repeated invalid behavior choices deterministically.
Acceptance:
- These cases do not stall turn progression.
- Deterministic fallback behavior is preserved.

[x] Keep the system compatible with manual/debug/runtime UI control sources.
Acceptance:
- Manual control can remain or become behavior-compatible without conflicting with automated behaviors.
- Existing Unity bridge/debug/runtime UI controls still conceptually fit the same action model.

[x] Preserve scenario-runner automation support for deterministic combo and preset sequence playback.
Acceptance:
- Scenarios can initialize units with behaviors and run without Unity input.
- Scripted behavior supports deterministic step playback.

## Phase 4 - Verification

[x] Add verifier coverage for behavior assignment, deterministic intent selection, pass behavior, scripted behavior, and behavior-driven enemy equivalence.
Acceptance:
- Pure verifier coverage proves the behavior layer is deterministic and core-owned.
- Existing enemy behavior expectations remain valid under the new abstraction.

[x] Re-run the permanent in-repo core runner to confirm all existing suites still pass.
Acceptance:
- `dotnet run --project Tools/CoreRunner/CoreRunner.csproj` still passes.
- No Unity dependency leaks into the out-of-Unity runner path.

[x] Update this task file and the holistic current-state document during execution so the new feature is reflected accurately once complete.
Acceptance:
- Task status matches implementation progress.
- `specDev/current-state.md` reflects the behavior-system foundation when the feature is complete.
