# Design: 14 Core 4x4 Board Layout

## Overview

This feature migrates the authoritative combat board from `6x6` to `4x4` in the pure C# core, then updates dependent scenarios, verifiers, and Unity presentation to match.

The migration is split so the board contract changes first, scenario and verifier expectations migrate second, and Unity presentation remaps the final `4x4` board with a small visual split between the player-facing half and enemy-facing half last.

## Core Approach

- Keep the existing `BoardDimensions` model.
- Replace current `6x6` assumptions with centralized `4x4` layout constants.
- Update default scenario positions so they remain valid and readable on the smaller board.
- Preserve the existing event-driven combat execution flow.

## Migration Areas

### Board Contract

- `CombatScenarioFactory` becomes the initial canonical source for:
  - board width
  - board height
  - default player start position
  - default enemy start position
- Current default-state construction should consume those canonical values instead of duplicating coordinates inline.

### Scenario And Behavior Migration

- Existing scenario definitions and validation catalogs must move to valid `4x4` coordinates.
- Movement and ability scenarios should keep their intent, but their placements may need to tighten.
- Behavior-driven scenarios must continue producing deterministic results on the smaller board.

### Verifier Migration

- Verifiers that assert `6x6` board output must be updated to `4x4`.
- Board-format snapshots should continue using the same format, just with smaller dimensions.
- Scenario validation output must continue to show ordered events and board states.

### Unity Presentation

- Unity board rendering should derive the final dimensions from the core snapshot.
- The tactical board view keeps primitive tiles and unit objects.
- The visual player-side and enemy-side gap is a rendering offset only; it must not alter core coordinates or introduce fake tiles.

## Input And Interaction

- Tile click routing remains unchanged in architecture:
  - Unity selects/presents
  - bridge forwards requests
  - core validates and mutates
- Highlight behavior remains presentation-only.

## Sequencing

1. Centralize board and default position constants.
2. Switch the core board dimensions and default scenario to `4x4`.
3. Update scenario catalogs and verifier expectations.
4. Update Unity tactical-board placement logic to add the small side gap.
5. Refresh holistic docs after the feature is complete.

## Risks

- Ability and scenario tests that assumed spacious `6x6` geometry may need coordinate redesign, not just numeric replacement.
- Behavior movement assertions may change if units begin closer together.
- Unity side-gap rendering must stay purely visual so click-to-core mapping remains deterministic.
