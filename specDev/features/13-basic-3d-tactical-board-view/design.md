# Design: [13] Basic 3D Tactical Board View

## Purpose

This feature adds a simple Unity-side 3D tactical board presentation for the existing combat core.

It does not add gameplay rules.

It renders:

- a primitive `6x6` board
- one player unit as a sphere
- one enemy unit as a cube
- presentation-only highlights for movement, targeting, and preview states

The core remains authoritative for all combat validation and state changes.

## Presentation Role

The board view is a visualization and input surface over the existing Unity bridge and runtime control flow.

Unity responsibilities in this feature are:

- render board tiles
- render unit primitives
- map tile clicks to bridge/core requests
- show temporary highlights and preview state
- stay synchronized with core events

Unity must not:

- decide whether movement is legal
- decide whether attacks are valid
- decide damage, death, turn flow, or behavior logic
- become a second board-state authority

## Board Model

The board is a flat world-space grid built from one primitive object per tile.

First-pass presentation rules:

- `6x6`
- square grey tiles
- slight spacing between tiles
- slight floating visual feel above a neutral plane or empty space
- readable from an angled static camera

Each tile maps directly to a core `GridPosition`.

The tile visual model should be deterministic and should not require authored meshes.

## Tile Primitive Strategy

Tiles remain primitive-based for this feature.

The implementation may use:

- cubes with small height
- quads or planes with a consistent highlight layer

The important design constraint is that:

- tiles are individually addressable
- highlights can be changed per tile
- unit placement remains centered and readable

## Unit Primitive Strategy

The board view renders current authoritative unit state using primitive objects.

Rules:

- player = sphere
- enemy = cube
- unit primitive footprint is smaller than the tile
- unit object is centered on tile coordinates
- removed or dead units disappear

The first version assumes one enemy in the scene flow, while keeping the mapping logic clean enough that future multi-enemy support is not blocked.

## Coordinate Mapping

A deterministic mapping layer converts core `GridPosition` values into Unity world positions.

That mapping must be shared by:

- tile placement
- unit placement
- highlight placement
- tile click hit interpretation

This avoids subtle desync between what the player sees and what the core state means.

## Visual State Categories

The board view maintains presentation-only visual states such as:

- idle tile
- selected tile
- valid movement tile
- attack or ability preview tile
- optional invalid feedback tile

These states are derived from:

- current selected unit context
- current preview mode
- current selected ability context
- current core state snapshot

They do not alter gameplay state.

## Highlight Rules

Required highlight behavior for the first pass:

- valid movement tiles highlight yellow
- attack or ability preview tiles highlight red
- selected tile remains visibly distinguishable

Highlights must:

- be resettable
- recompute when the acting unit changes position
- recompute when selected ability context changes
- clear when preview is canceled

## Input States

The board interaction design uses a small presentation-state machine.

### 1. Idle Selection State

Default state.

In this state:

- tile clicks may select a tile
- tile clicks on valid movement targets may submit a move request
- no pending attack confirmation exists

### 2. Attack Preview State

Entered when the player chooses an attack-capable action through the existing UI or interaction path.

In this state:

- a first tile click previews affected tiles
- preview tiles highlight red
- no gameplay mutation happens yet
- a second click confirms the attack request

### 3. Preview Canceled State

Preview returns to idle when canceled by:

- clicking off
- pressing `Esc`
- selecting a different mode that replaces the preview

On cancel:

- red preview highlights clear
- pending confirmation state clears
- movement input becomes active again through the normal board-click flow

## Movement Interaction Design

Movement must coexist with attack preview rather than being replaced by it.

Rules:

- when not in attack preview, clicking a valid movement tile should submit movement through the existing core path
- movement availability is shown through yellow highlights
- movement requests still go through the bridge and core validation
- invalid movement still fails through core events, not Unity-side rule checks

## Attack Interaction Design

The attack flow for this phase is explicitly two-step:

1. Enter preview mode through the existing UI/action-selection path.
2. Click a tile to preview the affected attack area in red.
3. Click again to confirm the request.

Cancel rules:

- click off to cancel
- press `Esc` to cancel

Unity may reject obviously malformed presentation states, but core validation still decides whether the confirmed request succeeds.

## Integration With Existing Runtime UI

This board view does not replace the current runtime UI.

The expected coexistence is:

- runtime UI still chooses or cycles abilities
- board view shows the current board and tile interactions
- attack preview uses the currently selected ability context where relevant

This keeps the existing UI useful while making the board spatially interactive.

## Event Sync Design

Core events remain the source for visual updates.

Examples:

- `UnitMoved` updates the visual unit position
- `UnitDeath` or `UnitRemoved` hides or destroys the corresponding unit object
- attack or ability events may drive temporary preview or impact-highlight behavior

The board view should also refresh from a current bridge snapshot after meaningful changes so presentation stays robust even if multiple events occur in quick succession.

## Board Builder And Runtime Bridge Split

The design should separate:

- board construction and object ownership
- runtime state synchronization
- input interpretation

A practical split is:

- board-view builder or presenter:
  - creates tiles and unit primitives
- runtime sync controller:
  - listens to bridge/session state and updates transforms/highlights
- input handler:
  - interprets raycast clicks and routes requests through the existing bridge path

This keeps the board feature replaceable and avoids making one MonoBehaviour do everything.

## Camera Design

The first version uses a simple static camera.

Requirements:

- see the full board at once
- downward angle
- no free-look or advanced controls

The camera is a presentation detail only and should not be entangled with gameplay state.

## Logging

Existing Unity debug logging continues unchanged.

This feature may add presentation-oriented debug logs if needed, but:

- gameplay event logs remain primary
- board rendering must not replace ordered core event visibility
- invalid actions should still be understood through existing event/failure logs

## Constraints

This feature remains intentionally minimal.

Out of scope:

- animation
- VFX
- audio
- polished HUD
- health bars or labels unless explicitly added later
- pathfinding visual polish
- character models
- terrain systems
- inventory or progression systems
- Unity-owned combat rules

## Verification Direction

Verification for this feature should preserve the current split:

- pure core/verifier suites must still pass unchanged
- Unity-side validation should focus on:
  - board generation correctness
  - coordinate mapping correctness
  - highlight state transitions
  - event-driven visual sync behavior

Because this feature is mostly Unity-side presentation, full verification will likely be a mix of:

- existing out-of-Unity regression checks
- targeted Unity-side validation or manual editor checks
