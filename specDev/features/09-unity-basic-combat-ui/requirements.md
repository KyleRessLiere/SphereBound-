# Requirements: 09 Unity Basic Combat UI

## Interpreted Feature

Add a thin Unity runtime UI layer that lets a player trigger basic combat actions from visible on-screen controls instead of only Inspector debug controls.

The initial UI should support:
- directional movement buttons: `Up`, `Down`, `Left`, `Right`
- one large visible button for the currently selected player ability
- left and right controls to cycle through the player's available abilities

This UI must remain a presentation and input layer over the existing pure C# combat core.

## Goal

Provide the first playable on-screen combat controls in Unity so the player can:
- move the player unit through simple directional buttons
- trigger the player's currently selected ability through a large visible button
- observe the same event flow and board/debug output already provided by the core and Unity bridge

## Player Behavior

- The player can press on-screen `Up`, `Down`, `Left`, and `Right` buttons.
- Pressing a movement button attempts to move the current player unit by `1` tile in that direction.
- The player can see one currently selected player ability at a time.
- The player can cycle left or right through the player unit's current available abilities.
- The large ability button should include simple effect-tile coordinate output for the currently selected ability in the current player context.
- Pressing the large ability button sends the currently selected ability through the core gameplay system.
- If the selected attack or ability requires a target that is already unambiguous in the current scenario, the UI may use the currently supported debug target assumptions for the initial version.
- Failed actions should still show through the normal logs and debug outputs.

## Core Rules

- Unity UI must not implement gameplay rules.
- Movement, ability use, action spending, targeting validation, damage, death, and turn rules must stay in the pure C# core.
- The UI must use the existing Unity bridge or a thin extension of it rather than creating a second gameplay control path.
- Movement buttons must route through the existing core movement flow.
- The ability UI must be built from the player's current definition-backed available abilities rather than hardcoded attack names.
- The ability UI must be able to show resolved effect-tile coordinates for the currently selected ability in the current player context.
- The UI must update when available player actions or available abilities change.
- The UI must not break the existing Inspector debug controls.

## Constraints

- No production HUD, menus, polish, animation, VFX, or audio yet.
- No gameplay-rule ownership in MonoBehaviours.
- No bypassing the event system.
- No duplicated combat logic in UI code.
- The UI should remain thin and replaceable.
- Existing out-of-Unity verifier suites must continue to pass.

## Interactions

This feature must integrate with:
- the turn system
- player action spending
- the player or class definition system
- ability-definition availability
- the Unity listener bridge
- existing board and event logging
- existing movement and ability resolution

## Edge Cases

- Movement button pressed into an invalid or occupied tile
- Movement button pressed when no player actions remain
- Ability button pressed when the player cannot currently use the selected ability
- Ability button pressed when the selected ability has no valid target
- Selected ability index changes after restart or player-definition changes
- UI pressed before a session is initialized
- UI pressed after combat end or after the player unit dies
- UI remains synchronized after restart combat

## Event Hooks

The UI-triggered actions must still produce normal gameplay event flow, including as applicable:

- `MoveRequested`
- `AbilityRequested`
- `AttackRequested`
- `DamageRequested`
- `ActionStarted`
- `ActionSpent`
- `ActionEnded`
- `ActionFailed`
- `UnitMoved`
- `UnitDamaged`
- `UnitDying`
- `UnitDeath`
- `TurnStarted`
- `TurnEnded`

## Logging Requirements

- Actions triggered from the runtime UI must appear in Unity logs through the normal bridge logging path.
- Board and attack overlay outputs must continue to work after UI-triggered actions.
- UI use must not create a separate logging format for gameplay resolution.

## Acceptance Criteria

- A visible runtime Unity UI exists for movement directions: `Up`, `Down`, `Left`, `Right`.
- Pressing a direction button attempts player movement through the core.
- A visible runtime Unity UI exposes one large ability button plus left/right cycling controls.
- The large ability button is populated from the player unit's current definition-backed selected ability rather than hardcoded UI names.
- The runtime UI displays resolved effect-tile coordinates directly on the large ability button for the currently selected ability.
- Pressing the large ability button triggers core ability resolution through the existing gameplay path.
- Invalid movement or ability use fails cleanly without Unity mutating gameplay state directly.
- Existing Unity event logging, board logging, and attack overlay logging still work when actions come from UI buttons.
- Existing Inspector debug controls still work.
- Existing core runner suites still pass.
- No Unity gameplay dependency is introduced into the pure C# runner path.

## Ambiguities

- Whether the first UI should use Unity UI Toolkit, uGUI, or another Unity UI approach
- Whether ability targeting in the first version is fully interactive or limited to currently supported simple assumptions
- Whether movement and ability controls should be disabled visually when unusable or simply allowed to fail through logs
- Whether the UI should show action cost and remaining actions directly or rely on the existing debug logs
- Whether enemy turns should auto-run immediately after player end-turn conditions as they do now, or whether the UI should expose an explicit end-turn control
- Whether ability cycling should wrap from last to first ability automatically
