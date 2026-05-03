# DESIGN SPEC

## Overview

Feature `15` combines three tightly related changes:

- migrate the authoritative combat board from `4x4` to `5x5`
- add a core-owned enemy intent snapshot surfaced to Unity runtime UI
- add a first persistent multi-turn enemy behavior, `Sniper`, that charges and fires a line attack

The design preserves the existing architecture boundary:

- gameplay rules, board size, behavior state, charge countdown, and attack execution remain in the pure C# core
- Unity remains presentation, input orchestration, and snapshot display only

## Core Board Migration

### Board Authority

- The core `CombatState.Board` becomes `5x5`.
- Default scenario creation and any default board helper paths must produce `5x5`.
- Verifier assumptions, scenario expectations, and board-format log output must be updated to `5x5`.

### Spawn And Layout Migration

- Player and enemy default spawn positions move to valid `5x5` coordinates.
- Existing scenario catalogs and validation setups must be updated to explicit `5x5` placements rather than relying on old `4x4` assumptions.
- Unity tactical board generation reads only the authoritative board dimensions and regenerates the correct tile count.

### Tactical Board Presentation

- Unity continues using primitive tiles and core-driven tile coordinates.
- The existing visual side-gap remains a presentation-only mapping layered onto the `5x5` board.
- The board view must not imply a different board shape than the core actually uses.

## Enemy Intent Snapshot Model

### Purpose

- Provide a reusable, bridge-facing summary of what each living enemy currently intends to do.
- Allow the runtime UI to render deterministic, readable enemy intent without inferring behavior in Unity.

### Required Data Shape

Each enemy intent entry should expose:

- enemy unit id
- enemy display name
- intent type
- action or ability display name
- target unit id and display name when present
- target tile when present
- countdown value when present
- readable summary text

### Ownership

- Intent summary data is derived from core-owned behavior state and decision state.
- Unity does not compute enemy intent from raw board state on its own.
- The bridge exposes the latest intent snapshot for runtime UI consumption.

## Sniper Behavior Design

### Behavior Role

- `Sniper` is a pluggable behavior-driven enemy using the existing combat behavior system.
- The behavior must support persistent state across turns so it can remember charge progress.

### Decision Flow

Per enemy turn, the sniper follows this order:

1. If not aligned with the player, choose a deterministic move that works toward alignment.
2. If aligned and not already charging, enter charging state.
3. If charging and countdown remains above firing threshold, continue charging and reduce countdown.
4. If charging and countdown reaches the firing step, check alignment again.
5. If still aligned, fire through the existing ability system.
6. If no longer aligned, discard the pending shot and return to normal decision-making.

### Alignment

- Alignment means sharing the same row or same column as the player.
- No diagonal alignment support is introduced.

### Charge State

- Charge state is core-owned persistent state.
- It must include, at minimum:
  - whether the sniper is charging
  - remaining countdown
  - intended fire direction or target context sufficient to summarize and resolve the attack
- The design leaves open whether this state lives directly in behavior-owned state, unit-owned transient state, or a shared status-like model, but it must remain in the core.

### Charge Countdown

- Initial countdown is `2` enemy turns.
- Charging produces an intent summary like `Charging Line Shot - 2 turns remaining`.
- The countdown changes only through enemy turn progression in the core.

### Firing

- Firing uses an ordinary definition-backed ability path rather than a custom damage shortcut.
- The sniper attack must generate the normal ability and downstream combat events.
- If alignment is broken at the firing step, the sniper does not fire and instead returns to non-charging behavior selection.

## Sniper Line Attack

### Shape

- The sniper line shot resolves along the row or column toward the player.
- The shot definition remains deterministic.

### Resolution Path

- The attack uses the existing ability request -> resolve -> event pipeline.
- The core remains responsible for:
  - checking alignment at fire time
  - resolving affected tiles
  - applying damage through normal events
  - spending or not spending actions according to the normal core rules for that enemy action

## Enemy Intent UI

### Runtime Panel

- Add a runtime UI text panel titled `Enemy Intent`.
- It lives in the existing runtime HUD layer, not as a world-space UI element.
- The panel renders one line per living enemy.

### Content Strategy

- The panel reads bridge-provided enemy intent snapshot entries.
- Each line uses the snapshot’s readable summary text.
- Ordering should be stable and deterministic, preferably by unit id or current snapshot order.

### Refresh Strategy

The panel refreshes when:

- combat initializes
- bridge runtime state changes
- enemy decisions update
- turn changes
- countdown updates
- sniper fires
- enemies die or are removed
- combat restarts

## Verification Design

### Core Verification

Add or update verifier coverage for:

- `5x5` board default assumptions
- enemy intent snapshot generation
- sniper alignment detection
- sniper countdown progression
- sniper firing when alignment is preserved
- sniper canceling or resetting when alignment is broken

### Runtime UI Verification

- Runtime UI data verification should cover that enemy intent summaries are exposed to Unity through a bridge-facing model.
- No Unity-owned gameplay verification path should be introduced.

### Existing Suite Compatibility

- All existing verifier suites must still pass after `5x5` migration and sniper behavior introduction.

## Non-Goals

- No world-space intent icons
- No tile telegraph overlays for enemy intent
- No animations
- No VFX
- No audio
- No line-of-sight blocker system unless already required by existing mechanics
- No Unity-authored enemy AI logic
