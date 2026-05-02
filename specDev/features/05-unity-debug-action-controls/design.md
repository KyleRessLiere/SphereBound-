# Design: 05 Unity Debug Action Controls

## Flow

Unity Debug Action Controls extends the existing Unity Listener Bridge with a thin developer-facing command surface that can issue a small set of manual debug actions into the pure C# combat core.

High-level flow:
- Unity bridge holds or creates an observable core combat session.
- A developer triggers a debug command from a Unity-only debugging surface.
- The Unity bridge translates that command into a call against the existing core-session interface.
- The core validates and resolves the command using the same rules as any other gameplay action.
- The existing event stream is emitted in normal order.
- The Unity bridge logs the resulting events and refreshes mirrored unit state.

The Unity-side control surface is not a gameplay system. It is only a debugging entry point into the existing core.

## Event Flow

### Request Phase

Debug controls are responsible only for packaging developer intent and forwarding it into the core session.

Request-phase responsibilities:
- accept a developer-triggered debug command such as move, attack, end turn, or restart
- forward the command to the current observable combat session
- provide any required parameters, such as a debug destination tile or attack target

Request-phase rules:
- Unity must not perform gameplay validation itself beyond minimal command-shape checks needed to avoid invalid null/session access
- Unity must not decide whether an action is legal based on board, health, or turn rules
- move and attack requests must reuse the core combat request flow rather than inventing Unity-only events
- restart must recreate or reinitialize the session through the bridge/session layer rather than editing mirrored state in place

### Resolution Phase

Resolution remains entirely inside the pure C# combat core.

Core responsibilities during resolution:
- validate turn ownership
- validate remaining actions
- validate movement and attack legality
- mutate authoritative combat state only when valid
- emit ordered gameplay events and failure events

Unity debug control responsibilities during resolution:
- wait for the core result
- avoid issuing alternate mutations while a request is resolving
- preserve the same observation/logging behavior already established by the listener bridge

Resolution rules:
- `Move` must resolve through the existing move path
- `Attack` must resolve through the existing attack path
- `End Turn` must resolve through the existing turn-transition path
- `Restart Combat` must create a clean new session and then reconnect the bridge observation layer without duplicate subscriptions

### Result Phase

The result phase remains the same observable output surface used by the existing Unity Listener Bridge.

Expected result behavior:
- Unity Console logs show the emitted core events in order
- mirrored unit snapshots update after each action or restart
- failure events remain visible and distinguishable from successful actions
- restart results in a fresh startup state and fresh startup event sequence

Result rules:
- Unity-side debug controls do not fabricate results
- Unity-side debug controls only expose what the core actually resolved
- mirrored debug state is refreshed from authoritative core state after result events

## State Changes

Authoritative state remains in the core combat session.

Core-owned state includes:
- board occupancy
- unit positions
- unit health and life state
- turn owner
- remaining actions
- emitted gameplay events

Unity-owned debug state includes:
- current debug input parameters for actions
- current bridge/session reference
- mirrored unit debug state
- optional placeholder object state already supported by the listener bridge

State rules:
- Unity debug controls may update debug input fields, but not authoritative combat state
- successful actions change core state only through existing core resolution
- failed actions leave authoritative state unchanged except for emitted failure events
- restart replaces the active session reference and refreshes mirrored state from the new session
- repeated restart/setup operations must not leak prior subscriptions or stale debug-state references

## Effect Interactions

No effect system changes are introduced by this feature.

Debug controls simply invoke the same core combat actions already used by the current combat loop.

Effect-related design rule:
- if later combat features add effects to movement, attack, damage, or death, Unity debug controls must observe those results through the existing core event flow rather than special-casing them in Unity

## Failure Conditions

Gameplay failures must continue to be owned by the core.

Gameplay failure cases include:
- invalid move destination
- occupied move destination
- non-adjacent attack
- missing or dead attack target
- no remaining actions
- wrong-side or wrong-turn action

Bridge/control failure cases include:
- no active session exists when a debug command is triggered
- restart is requested while a prior session reference is still subscribed
- debug action parameters are incomplete for the requested command

Failure-handling rules:
- gameplay failures must surface through normal core failure events
- bridge/control failures must be distinguishable from gameplay failures
- bridge/control failures must not partially mutate mirrored or authoritative combat state
- repeated invalid debug commands must not break later valid commands

## Logging

Unity Console logging remains the primary debugging surface.

Logging rules:
- each debug-triggered action should be traceable through the normal core event sequence
- event order must match the core emission order exactly
- successful and failed actions must both be visible
- restart must produce a clearly observable transition back to a fresh session state
- Unity log output should remain semantically aligned with the in-repo core runner output

Mirrored debug state should remain inspectable after:
- move attempts
- attack attempts
- end-turn transitions
- restart

## System Impact

This feature turns the Unity bridge from a passive listener into a thin debugging control surface without changing the authoritative gameplay architecture.

It enables:
- in-editor/manual verification of core gameplay actions
- Unity-side validation of event ordering and mirrored state updates
- faster debugging of the combat loop without building a real player input/UI layer

It must not:
- move gameplay rules into Unity
- fork core combat logic
- introduce player-facing UI/input architecture
- change existing core verifier expectations
- weaken deterministic behavior

Open design limits for later phases:
- the exact Unity debugging surface, such as inspector fields, context menus, or custom editor controls, remains open
- support for selecting arbitrary units is deferred
- richer debug tooling, such as step history or replay, is out of scope
