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
  - action cost
  - targeting mode
  - valid target rules
  - deterministic affected-tile patterns
  - payload definitions for damage, healing representation, and forced movement
- Ability use flows through the event system and still emits the downstream movement and damage events that existing tooling observes.
- Ability-caused forced movement is supported through the same movement validation and movement event path used by normal movement.

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
  - ability and player-definition behavior
  - scenario runner behavior
  - Unity debug action behavior
  - Unity combat debug surface behavior
  - Unity combat debug file-output behavior

## Scenario And Logging Tooling

- The project has a deterministic scenario runner in pure C#.
- Scenarios can execute scripted combat flows and verify results.
- The runner prints ordered scenario logs and verification summaries.

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
