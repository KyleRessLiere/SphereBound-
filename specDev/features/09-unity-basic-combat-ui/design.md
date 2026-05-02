# Design: 09 Unity Basic Combat UI

## Flow

Unity Basic Combat UI adds the first runtime `uGUI` control surface for driving the existing combat core from visible in-scene controls.

High-level flow:
- Unity initializes or attaches to the existing combat session through the current listener bridge.
- A thin runtime UI presenter reads mirrored session and player-definition data from that bridge.
- The UI renders:
  - directional movement buttons: `Up`, `Down`, `Left`, `Right`
  - an `End Turn` button
  - one large ability button for the currently selected player ability
  - left and right cycle controls for changing the selected ability
  - ability metadata text, including name and targeting-shape description
  - simple resolved effect-tile coordinates rendered directly on the large selected-ability button for the current player context
- When the user presses a button:
  - the UI forwards the request into the existing Unity bridge or a thin extension of it
  - the bridge forwards the request into the pure C# core
  - the core resolves the action through the normal event system
  - Unity observes the resulting events, state updates, and logs through the existing observation path

This feature intentionally uses `uGUI` because the current goal is quick functional runtime validation rather than long-term UI framework decisions.

## Event Flow

### Request Phase

The UI layer only creates and forwards player intent.

Request-phase responsibilities:
- expose movement intents for the current player unit in four orthogonal directions
- expose an explicit `End Turn` intent
- expose ability cycling intent across the player's current definition-backed abilities
- expose ability-use intent for the currently selected player ability
- expose ability metadata read from the player's definition-backed ability list
- expose core-resolved effect-tile coordinates for the currently selected ability under the current simple targeting assumptions

Request-phase rules:
- button clicks must not mutate gameplay state directly
- button clicks must not compute damage, targeting, or validation outcomes locally
- ability button labels and descriptions must come from ability definition metadata rather than hardcoded UI-only strings

### Resolution Phase

Resolution remains entirely in the existing core gameplay path.

Core responsibilities during resolution:
- validate move requests
- validate ability use and target assumptions
- spend actions only when allowed by core rules
- run damage, forced movement, death, and turn transitions through the normal event system
- auto-run enemy turn after player end turn through the existing combat flow

Resolution rules:
- the UI must reuse the current bridge and debug-command pathway where appropriate
- the UI may add a thin ability-command path if needed, but that path must still call into the pure core rather than duplicate combat logic
- the player must be initialized with multiple simple test abilities so the runtime UI can prove it is reading live definition-backed data

### Result Phase

The result phase remains the same observable gameplay pipeline already present in the project.

Expected result behavior:
- runtime UI actions produce the same ordered core events as Inspector/debug actions
- Unity Console logging continues to reflect those events
- board and attack overlay logging continue to work
- UI can refresh its selected ability presentation and availability state after gameplay changes or restart

## State Changes

Core-owned state remains authoritative:
- board state
- turn state
- action economy
- player and enemy unit state
- ability definitions and current player ability list

Unity-owned UI state is limited to:
- references to `uGUI` controls
- selected ability index
- rendered labels and descriptions
- current selected or implied target assumptions for the first version
- presentational enabled or disabled state for buttons

Allowed UI state changes:
- rebuild selected ability presentation when a session starts or restarts
- refresh selected ability index, labels, descriptions, coordinates, and interactable states from mirrored authoritative state

Disallowed UI state changes:
- direct health mutation
- direct board mutation
- direct turn mutation
- hardcoded attack resolution or damage logic

## Metadata And Ability List Design

The runtime UI needs a stable definition-backed metadata surface.

Required metadata in the first version:
- ability id
- ability name
- ability description
- action cost
- resolved effect-tile coordinates for the current player context

Description rule for this phase:
- descriptions should communicate targeting or shape behavior in plain functional language
- examples:
  - `adjacent forward`
  - `line of 3`
  - `2x2 in front of player`

Initial ability-list design rule:
- the player definition should expose multiple basic test abilities
- these abilities exist primarily to prove:
  - player initialization from definitions
  - ability metadata flow into UI
  - runtime selected-ability presentation from core definitions
  - ability-use execution through the core path

Effect-tile coordinate rule for this phase:
- the UI should ask the core-owned ability-resolution layer for the currently resolved affected tiles for a player ability under the current simple targeting assumptions
- those tiles should be shown in a simple coordinate list such as `(1,2), (1,3)`
- the coordinate list should appear on the large selected-ability button itself rather than only in a separate detail panel
- these coordinates are a debug-facing presentation of resolved ability output, not a second gameplay authority

Ability cycling rule for this phase:
- the runtime UI should hold a selected ability index in presentation state only
- left and right controls should move that selected index across the current player ability list
- when the underlying ability list changes after restart or state refresh, the selected index should clamp safely into the current list
- cycling does not create gameplay state on its own; only pressing the large ability button issues gameplay intent

## Targeting And Input Assumptions

This feature does not attempt to solve full production targeting UI.

Initial targeting assumptions:
- movement uses direct directional buttons
- end turn uses a direct button
- the large ability button may use currently supported simple targeting assumptions where target selection is limited or inferred
- if an ability cannot be validly resolved under those assumptions, the core should fail the action normally and Unity should show that through existing logs

This keeps the feature focused on:
- visible runtime controls
- metadata display
- resolved effect-tile display
- simple selected-ability cycling
- proof of definition-driven UI population

## Failure Conditions

Gameplay failure cases include:
- movement into blocked or out-of-bounds tiles
- pressing ability buttons with no valid target under current assumptions
- pressing actions with no remaining actions
- using the UI before a session exists
- using the UI after the player dies or combat is no longer active

Failure-handling rules:
- gameplay failures must still be surfaced through the normal core event and log flow
- bridge-level failures such as missing session should surface as Unity-side warnings without changing gameplay state
- UI refresh must remain stable after failures

## Logging

Logging continues to be handled by the existing debug surface and bridge.

Logging rules:
- runtime UI actions must produce the same gameplay logs as other control surfaces
- board snapshots and attack overlays must continue to emit when those gameplay events occur
- UI-specific metadata rendering does not need its own gameplay log format

## System Impact

This feature creates the first thin runtime combat UI without moving gameplay rules into Unity.

It enables:
- basic in-scene manual playtesting
- runtime validation of definition-backed abilities
- visible proof that player abilities are being sourced from core definitions

It must not:
- replace the Inspector debug controls
- create a second gameplay rules path
- overbuild a production HUD
- require a full targeting UX before basic runtime testing works
