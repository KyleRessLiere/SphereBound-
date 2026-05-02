# Design: 07 Unity Combat Debug File Output

## Flow

Unity Combat Debug File Output extends the current Unity combat debug surface with:

- bracketed board and overlay formatting
- optional real-time file output of the existing debug stream

High-level flow:
- the existing Unity bridge/debug surface observes core gameplay events in order
- the existing debug surface derives event lines, action-count lines, board snapshots, and attack overlays
- the new formatting layer converts board and overlay output into bracketed cell format
- the new output layer sends the same ordered debug entries to:
  - Unity Console
  - an optional file writer when enabled
- the file writer creates a dated directory and a timestamped file for the current debug run/session scope

The file-output layer is purely downstream. It does not influence gameplay, event timing, or combat state.

## Event Flow

### Request Phase

This feature does not introduce new gameplay requests.

Request-side responsibilities:
- allow configuration of whether file output is enabled
- allow configuration or determination of the output destination root
- initialize file-output state when a debug run/session begins

Request-phase rules:
- file-output configuration must not change gameplay requests
- enabling or disabling file output must not change which gameplay events occur
- board-format changes must not alter the underlying debug-surface trigger rules

### Resolution Phase

Gameplay resolution remains entirely in the pure C# core.

Core responsibilities during resolution:
- unchanged from current combat/debug features
- validate and resolve movement, attacks, enemy behavior, and turn flow
- emit ordered gameplay events

Unity debug-surface responsibilities during resolution:
- continue observing events in order
- continue deriving board snapshots, overlays, and action counts

File-output responsibilities during resolution:
- none at gameplay-authority level
- file writing only consumes already-derived debug output entries

### Result Phase

The result phase expands how derived debug output is formatted and persisted.

Expected results:
- Unity Console still shows live event output
- board snapshots use bracketed cell formatting such as `[ ][P][ ][ ]`
- attack overlays use the same bracketed style while preserving one-symbol-per-tile rules
- when file output is enabled, the same ordered debug stream is appended to a timestamped text file

Result rules:
- Console and file output remain semantically aligned
- file output order matches the order in which the debug surface produced entries
- board and overlay formatting remain derived views, not new gameplay state

## State Changes

Authoritative gameplay state remains in the pure C# combat core.

Core-owned state includes:
- board occupancy
- unit positions
- health and life state
- turn ownership
- remaining actions
- emitted gameplay events

Unity debug-surface owned state includes:
- derived event lines
- derived action-count lines
- derived board snapshots
- derived attack overlays

File-output owned state includes:
- enabled/disabled setting
- chosen output root
- current dated output directory
- current timestamped output file identity
- append/write lifecycle state for the active debug run

State rules:
- file-output state must not become authoritative gameplay state
- toggling file output only affects persistence behavior
- restart or repeated runs must create clean output separation without mutating prior files

## Effect Interactions

No gameplay effect-system changes are introduced.

For this feature:
- bracketed formatting applies to the existing board snapshot and overlay outputs
- file output persists the existing derived debug stream only
- later gameplay effects, if added, should appear through the same debug-surface derivation path rather than special-cased file logic

## Failure Conditions

Gameplay failures remain owned by the core and are already reflected in the debug surface.

File/debug-output failure cases include:
- file output disabled
- output directory missing before first write
- file path initialization failure
- restart occurring after one file has already been opened or created
- rapid consecutive writes

Failure-handling rules:
- disabled file output is a valid no-write mode, not an error
- directory creation must happen safely before writing when needed
- output-layer failures must be distinguishable from gameplay failures
- file-output failures must not break Console logging
- file-output failures must not mutate gameplay state or suppress the underlying debug stream

## Logging

The debug stream remains ordered and primary.

Logging rules:
- gameplay events still appear live in Unity Console
- action-count updates still appear when they change
- movement board snapshots still appear after movement resolution
- attack overlays still appear after attack resolution

Bracketed formatting rules:
- empty cell renders as `[ ]`
- player cell renders as `[P]`
- enemy cell renders as `[E]`
- overlay cells render with bracketed overlay symbols such as `[X]` and `[O]`
- one symbol appears per tile in a printed board/overlay view

File-output rules:
- when enabled, write entries in the same order they were produced
- place files under a date-based folder
- use timestamp-based file names for separation and traceability
- write plain readable text intended for replay/debug inspection

## System Impact

This feature improves the usefulness and persistence of the current Unity debug surface.

It enables:
- cleaner board readability through bracketed formatting
- persistence of live debug sessions to text files
- easier offline inspection or sharing of debug runs

It must not:
- alter combat logic
- alter event ordering
- alter enemy behavior
- replace Unity Console output
- become a second gameplay system

Open design limits for later phases:
- whether file scope is per session, per restart, or per editor run remains open
- exact config mechanism remains open
- exact timestamp/file naming convention remains open
- exact output root location remains open
