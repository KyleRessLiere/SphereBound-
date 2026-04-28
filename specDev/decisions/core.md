# Core Decisions

## Event System

- All gameplay flows through events
- Request -> Resolve -> Result pattern
- Effects emit events (no direct mutation)

## Turn System

- Player turn first
- Then enemy turn
- Same rules apply to both

## Board

- Grid-based
- Position matters for all interactions


# Architecture Decision: Pure Game Logic Core + Unity Presentation Layer

## Decision

Spherebound will separate gameplay rules from Unity presentation.

Core gameplay logic should live in plain C# wherever possible.

Unity should act primarily as:
- Input layer
- Presentation layer
- Animation/VFX/audio layer
- UI layer
- Scene/prefab orchestration layer

## Core Logic Owns

- Board state
- Unit state
- Turn rules
- Movement rules
- Attack rules
- Damage/healing
- Death/revival
- Effects/items
- Win/loss conditions
- Event resolution

## Unity Owns

- GameObjects
- Transforms
- Animators
- Coroutines
- Prefabs
- Cameras
- UI
- Input
- Visual/audio feedback

## Rule

Core gameplay logic must not depend on Unity-specific types unless unavoidable.

Avoid using:
- MonoBehaviour
- GameObject
- Transform
- Animator
- Coroutine
- Unity scene references

inside core gameplay rules.

## Event Flow

Core emits gameplay events.

Unity listens to those events and translates them into:
- Movement animations
- Hit effects
- Death animations
- UI updates
- Sound effects
- Camera feedback

## Input Flow

Unity receives player input, then asks the core logic to validate and resolve actions.

Unity should not directly mutate gameplay state.

## Reasoning

This keeps gameplay:
- Testable
- Deterministic
- Easier to debug
- Easier to expand
- Less coupled to Unity scenes
- Safer for complex item/effect interactions


