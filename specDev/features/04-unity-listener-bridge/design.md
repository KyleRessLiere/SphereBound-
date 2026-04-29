# Design: 04 Unity Listener Bridge

## Flow

Unity Listener Bridge provides a minimal Unity-side adapter that can start or attach to a core combat loop session, subscribe to its event output, and reflect core state into Unity for debugging.

High-level flow:
- Unity initializes the listener bridge component.
- The bridge creates or attaches to a core combat loop session.
- The bridge subscribes to ordered core gameplay events.
- As events are received:
  - the bridge logs them to the Unity Console
  - the bridge updates Unity-side mirrored debug state
  - optional placeholder objects are updated if present
- Unity remains an observer/presentation layer and does not own gameplay resolution.

The bridge is thin and replaceable. The gameplay core remains the only source of combat rules and authoritative state transitions.

## Event Flow

### Request Phase

The listener bridge does not introduce new gameplay request logic.

Request-side design rules:
- Unity may initialize or attach to a core session
- Unity does not generate combat rules itself
- any future Unity input would still need to flow into the core, but that is out of scope for this feature

For this feature, Unity’s role is initialization/attachment and observation.

### Resolution Phase

Resolution remains fully owned by the pure C# core.

Bridge responsibilities during resolution:
- receive core events in the order emitted
- mirror relevant state for debugging
- log ordered event information to the Unity Console

Core responsibilities during resolution:
- validate actions
- mutate gameplay state
- emit gameplay events
- preserve deterministic event ordering

Unity-side resolution rules:
- Unity must not mutate combat state directly
- Unity must not inject alternate movement/combat/damage/death rules
- Unity must not become a second source of truth for unit state

### Result Phase

The bridge exposes observable debugging outputs inside Unity:
- Unity Console log entries for core events
- mirrored player/enemy debug state
- optional placeholder object updates for inspection

Result rules:
- logged event order matches core event order
- mirrored state is updated from core outcomes, not from Unity-authored rules
- failure-related events remain visible in Unity logging

## State Changes

Authoritative state remains in the pure C# core.

Core-owned state includes:
- board dimensions
- unit positions
- health
- life state
- turn ownership
- combat event output

Unity bridge-owned state includes:
- references to the current core session
- event subscription wiring
- mirrored debug snapshots of unit state
- optional placeholder GameObject references for visualization/debugging

State rules:
- Unity mirrored state is derived from core state and core events
- Unity mirrored state is for observation/debugging only
- placeholder objects, if present, must not become authoritative gameplay state
- reconnect/reinitialize behavior must avoid duplicate subscriptions and duplicate state ownership

## Effect Interactions

No effect system changes are introduced by this feature.

The bridge simply reflects whatever the current core emits.

For current scope:
- it targets the core combat loop session
- it logs combat/failure events
- it mirrors basic unit state only
- no item/effect-specific presentation is added

## Failure Conditions

Failure cases the bridge must handle:
- core emits failure-related gameplay events
- bridge receives repeated initialization or attachment calls
- placeholder debugging objects are absent
- Unity-side observation/logging encounters setup issues

Failure handling rules:
- gameplay failures must still be logged in Unity in order
- bridge setup failures must be distinguishable from gameplay failures
- missing placeholder objects must not prevent pure logging/state observation from working
- duplicate subscriptions must be prevented or cleaned up

## Logging

Unity Console output is the primary debugging surface for this feature.

Logging rules:
- log existing core event names and relevant data
- preserve core event order exactly
- include enough state context to debug movement, attack, damage, death, failure events, and mirrored unit state
- keep the logging layer observational and lightweight

Mirrored state output may include:
- unit id
- side
- health
- position
- alive/dead state

## System Impact

This feature adds the first Unity-side bridge into the pure C# gameplay core.

It enables:
- Unity-side observation of live core gameplay behavior
- debugging inside the Unity editor/console
- a clear separation between authoritative core logic and Unity presentation/orchestration
- a foundation for later presentation/input features without putting rules into Unity

It must not:
- move gameplay rules into Unity
- alter existing core verifier behavior
- introduce animations, UI, input, VFX, or audio requirements
- create a second gameplay state authority on the Unity side

Open design limits for future phases:
- whether the bridge initializes or attaches by default is still open
- placeholder object strategy is still open
- broader support beyond combat-loop sessions is deferred
