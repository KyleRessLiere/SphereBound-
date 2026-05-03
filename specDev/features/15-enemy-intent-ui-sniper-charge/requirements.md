# REQUIREMENTS SPEC

## Feature Name

Enemy Intent UI + Sniper Charge Behavior + 5x5 Board Migration

## Feature Interpretation

Add a runtime UI panel that shows the current intended actions of all living enemies using core-generated intent data. Add a first specialized enemy type, `Sniper`, whose deterministic behavior moves to align with the player, charges a line attack over multiple enemy turns, exposes that countdown through the intent panel, and then fires through the existing ability and event systems. At the same time, migrate the core combat board from `4x4` to `5x5` and keep Unity presentation aligned with that authoritative board change.

## Goal

Allow the player to see what enemies currently intend to do without moving gameplay authority into Unity, while also introducing a first multi-turn enemy behavior that demonstrates persistent intent state, countdown display, and charged attack execution in the pure C# core. This feature also expands the authoritative combat board to `5x5` so the new behavior and intent presentation operate against the updated board layout.

## Board Size Migration

- The authoritative core combat board must change from `4x4` to `5x5`.
- Default scenario setup, movement validation, ability targeting, behavior decisions, scenario runner expectations, and verifier expectations must all align to the new `5x5` board.
- Unity tactical board presentation must mirror the new `5x5` core board rather than simulating a different size locally.
- The slight visual separation between player-side and enemy-side halves may remain a Unity presentation concern, but it must be applied on top of the authoritative `5x5` board.
- Existing board-state logs, snapshots, and verifier log output must reflect the `5x5` layout after migration.

## Enemy Intent UI

- Unity must expose a runtime text panel titled `Enemy Intent`.
- The panel must list every living enemy currently present in the combat state.
- Each enemy line must show, at minimum:
  - enemy display name or fallback id
  - intended action summary
  - target unit or tile when relevant
  - countdown value when relevant
- Example readable summaries may include:
  - `Sniper: Charging Line Shot - 2 turns remaining`
  - `Sniper: Charging Line Shot - 1 turn remaining`
  - `Sniper: Fire Line Shot at Player next`
  - `Brute: Move toward Player`
  - `Raider: Attack Player`
- The panel is presentation-only and must not become the authority for enemy decision-making.

## Intent Snapshot Data

- The core and bridge must expose enemy intent data in a reusable snapshot form suitable for the runtime UI.
- Each exposed enemy intent entry must contain:
  - enemy unit id
  - enemy display name
  - intent type
  - ability or action name when applicable
  - target unit id or target display name when applicable
  - target tile when applicable
  - countdown value when applicable
  - a readable summary string
- Intent snapshot data must be derived from core-owned state and behavior results rather than reconstructed ad hoc in Unity.

## Sniper Enemy

- Add a first-pass sniper enemy type or definition that uses the pluggable behavior system.
- Sniper behavior requirements:
  - if not aligned with the player, move toward a position that can align with the player
  - once aligned with the player, begin charging a line attack
  - while charging, expose countdown state through the intent system
  - after the countdown completes, fire if still aligned
  - after firing, return to normal decision-making on later turns
- Sniper behavior must remain deterministic for the same combat state.

## Alignment Rule

- Sniper alignment is defined, for this feature, as sharing the same row or the same column as the player.
- Diagonal alignment is out of scope for this feature.

## Charge Rule

- Sniper charging must be deterministic.
- Initial sniper charge duration must be `2` enemy turns unless explicitly overridden later by design.
- Charging state must persist across turns.
- If the sniper is charging, the intent panel must show the charge countdown.
- Charging may become interruptible later, but interruption handling is not required for completion of this feature.

## Sniper Line Attack

- The sniper line attack must target along the row or column toward the player.
- The sniper must only fire if it is still aligned when the countdown completes.
- If alignment is broken before the shot would fire, the sniper must return to normal decision-making rather than firing.
- Damage may remain fixed for the first pass.
- The sniper attack must resolve through the existing ability, damage, and event systems rather than a custom execution path.

## UI Refresh Requirements

The `Enemy Intent` panel must refresh when:

- combat initializes
- enemy behavior decisions are produced
- turn changes
- a charge countdown changes
- a sniper fires
- combat restarts
- an enemy dies or is removed

## Constraints

- The intent display must be text-only for this feature.
- No world-space intent icons are required.
- No intent tile overlays are required.
- No animations are required.
- No VFX are required.
- No audio is required.
- Unity must not own enemy logic.
- Unity must not duplicate authoritative enemy state.
- Gameplay rules and charge logic must remain in the pure C# core.

## Verification Requirements

Add or update verification where practical.

At minimum, verification must cover:

- enemy intent summaries are produced
- every living enemy appears in the intent list
- sniper shows charge countdown text
- countdown decreases deterministically across enemy turns
- sniper fires after charge completes when alignment is maintained
- sniper does not fire if alignment is broken before fire resolution
- the default board, scenarios, and tactical board view all operate on `5x5`
- existing verifier suites still pass

## Acceptance Criteria

- Runtime UI contains an `Enemy Intent` text panel.
- Combat runs on a `5x5` board in the core and Unity mirrors that board size.
- The panel lists all living enemies.
- Sniper intent displays charging text with countdown.
- Sniper begins charging when aligned with the player.
- Sniper fires after countdown reaches `0` or its final firing threshold as defined in design.
- Sniper attack uses the existing core event flow.
- The intent panel updates after decisions, turn changes, countdown changes, firing, restart, and death.
- Unity only displays core-generated intent data.
- Existing verifier suites pass after the feature is implemented.

## AMBIGUITIES

- How the `5x5` default player and enemy spawn positions should change relative to the current `4x4` setup.
- Whether the visual side gap on the Unity tactical board should sit between rows, between columns, or use some other presentation mapping on the `5x5` board.
- Whether countdown text should show turns remaining before the firing turn or turns until the next enemy action window.
- Whether the sniper may move and begin charging on the same turn, or whether alignment only enables charging on the following turn.
- Whether the sniper line attack hits only the player target or every unit occupying the firing line.
- Whether blockers or walls exist yet for line-of-fire purposes.
- Whether charging consumes the sniper's turn each charging turn.
- Whether firing consumes the sniper's turn.
- Whether charge countdown state belongs primarily to behavior state, unit state, or a more general status/effect state.
