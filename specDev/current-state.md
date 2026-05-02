# Current State

This document is the holistic snapshot of Spherebound's current implemented feature state.

It should be updated after each completed feature so the team can see the game's current capabilities without reading every spec file.

## Core Gameplay

- The game has a deterministic core combat loop implemented in pure C#.
- Combat runs on a `6x6` grid.
- The current scenario contains `1` player and `1` enemy.
- The player starts first and has `2` actions per turn.
- The player can move orthogonally by `1` tile and attack an adjacent enemy.
- The enemy acts after the player turn.
- The enemy moves toward the player when not adjacent and attacks when adjacent.
- Damage is fixed and units die at `0` health and are removed from the board.

## Definitions And Abilities

- The player is now created from a reusable core unit-definition or class-definition model instead of a hardcoded attack branch.
- Unit definitions currently provide:
  - base health
  - actions per turn
  - movement capability metadata
  - available ability definitions
- Normal movement remains a definition-backed core capability rather than a normal class ability entry.
- The current basic attack is represented as a reusable ability definition.
- Ability definitions currently support:
  - name and id
  - description metadata
  - action cost
  - targeting mode
  - valid target rules
  - deterministic affected-tile patterns
  - payload definitions for damage, healing representation, and forced movement
- Ability use flows through the event system and still emits the downstream movement and damage events that existing tooling observes.
- Ability-caused forced movement is supported through the same movement validation and movement event path used by normal movement.
- The current player definition now exposes multiple simple test abilities for runtime UI validation, including basic attack, forward-line, and front-cross style examples.

## Behaviors

- Units can now carry a pluggable combat behavior assignment in the pure C# core.
- Behavior assignments currently support:
  - default behavior sources
  - scenario-assigned behavior sources
  - manual behavior-compatible sources
- Behaviors evaluate against a read-only combat-state view and produce action intents instead of directly changing gameplay state.
- Supported intent types currently include:
  - move
  - use ability
  - pass
  - end turn
- Built-in behavior types currently include:
  - pass turn
  - move toward target
  - spam ability
  - scripted sequence
  - manual behavior shell
- Existing enemy chase behavior is now represented through the behavior system instead of a hardcoded enemy branch.
- Behavior decisions are logged through a dedicated event before downstream gameplay events resolve.
- Deterministic scripted and behavior-driven turn cycles can now be initialized in scenario state and played without Unity input.

## Architecture

- Gameplay rules live in a pure C# core.
- Unity is used as presentation, debugging, and input orchestration around that core.
- Gameplay flows through an event-driven request -> resolve -> result pattern.
- Unity is not the authoritative source of board, unit, or turn state.

## Core Verification

- The repo includes a permanent in-repo runner at `Tools/CoreRunner/CoreRunner.csproj`.
- The core runner can execute verifier suites outside Unity.
- The current verification stack covers:
  - combat loop behavior
  - pluggable combat behavior
  - ability and player-definition behavior
  - scenario runner behavior
  - Unity debug action behavior
  - runtime UI-facing ability metadata and effect-tile exposure
  - Unity combat debug surface behavior
  - Unity combat debug file-output behavior
  - verifier log output behavior

## Scenario And Logging Tooling

- The project has a deterministic scenario runner in pure C#.
- Scenarios can execute scripted combat flows and verify results.
- The runner prints ordered scenario logs and verification summaries.
- Scenario steps now support behavior-driven automated turn cycles.
- Scenario logs now include behavior-decision events alongside the normal gameplay event stream.
- The runner now writes stable per-check verifier log files into verifier-specific sibling `.logs` directories beside the verifier code in `Assets/Scripts/CoreCombatLoop/Verification/`.
- Combat-flow verifiers emit board/event-focused log files.
- Small component verifiers emit compact assertion-focused log files.
- Re-running the same verifier check overwrites that check's latest output file.

## Unity Debugging Surface

- Unity can attach to or initialize the combat core through a thin listener bridge.
- Unity logs core gameplay events in order.
- Unity mirrors unit debug state for Inspector visibility.
- Unity has debug controls for:
  - initialize session
  - start observed combat
  - move
  - attack
  - end turn
  - restart combat
- Unity exposes visible Inspector buttons for these debug actions through a custom editor.

## Runtime UI Surface

- Unity now has a basic `uGUI` runtime combat UI controller layer.
- The runtime UI can expose:
  - `Up`, `Down`, `Left`, `Right`
  - `End Turn`
  - one large selected-ability button
  - left/right ability cycling controls
- The runtime ability surface is generated from the player's live definition-backed abilities rather than hardcoded UI names.
- The selected ability button can show:
  - ability name
  - ability description
  - action cost
  - resolved effect-tile coordinates for the current selected ability and player context
- The runtime UI keeps a presentation-only selected ability index and cycles across the player's available abilities with wrap behavior.
- Runtime UI actions route through the existing Unity bridge and pure core gameplay path rather than a separate rules implementation.
- Unity also has an Editor scaffold utility that can create the basic runtime combat UI hierarchy and wire the scene references automatically.

## Board And Attack Debug Output

- Movement now prints full board snapshots in Unity debug output.
- Board output uses bracketed cells such as `[ ][P][ ][ ]`.
- Empty cells render as `[ ]`.
- Player cells render as `[P]`.
- Enemy cells render as `[E]`.
- Successful attack output renders as a bracketed attack overlay.
- Attack overlays use one symbol per tile.
- Affected/path tiles render as `[X]`.
- Hit/collision tiles render as `[O]`.

## File Output

- The Unity debug surface can optionally write the live debug stream to `.txt` files.
- File output is controlled by `CombatDebugOutput/combat-debug-output.config`.
- When enabled, output is written under `CombatDebugOutput/`.
- Output is organized into date-based folders.
- Individual log files use timestamp-based file names.

## Current Limits

- There is no player-facing runtime UI yet.
- There is no animation, VFX, audio, or polished scene presentation layer yet.
- The game is currently focused on core combat behavior and debugging infrastructure rather than production gameplay presentation.
