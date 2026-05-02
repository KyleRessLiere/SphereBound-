# Feature: 07 Unity Combat Debug File Output

## REQUIREMENTS SPEC

## Goal

Extend the Unity combat debug surface so board and combat debug output can be written to timestamped text files in real time, with a config toggle to enable or disable file output.

## Feature Interpretation

Unity Combat Debug File Output builds on the existing Unity combat debug surface by adding two things:

- a revised board text format that renders cells as bracketed slots such as `[ ][P][ ][ ]`
- an optional file-output layer that writes the live debug stream to text files organized by date and timestamp

This feature remains a debugging/output layer. It must not change combat rules, event order, or gameplay ownership.

## Behavior

- The board-state text output must render as bracketed cells.
- Empty tiles must render as `[ ]`.
- Player tiles must render as `[P]`.
- Enemy tiles must render as `[E]`.
- Attack overlay output must continue to use one symbol per tile while following the bracketed format.
- Combat events must continue to appear live in Unity as they are emitted.
- Board snapshots, attack overlays, and action-count updates must continue to be produced by the existing debug surface behavior.
- Unity must optionally write the live debug output stream to text files.
- File output must be controlled by a config setting that can enable or disable writing.
- When file output is enabled, the system must create a dated output folder.
- Files written for a debug session must use timestamp-based file names.

## Rules

- File output must remain downstream of the existing debug surface and core event flow.
- Unity must not change gameplay state or gameplay timing because file writing is enabled.
- The file-output toggle must not affect combat resolution or event ordering.
- The board/file formatter must remain derived from authoritative core state and observed events.
- The file-output layer must not become a second gameplay system.
- Existing Unity Console debug output must continue to work even when file writing is disabled.

## Edge Cases

- If file output is disabled, no debug files should be written.
- If file output is enabled after a session starts, new output must follow the configured behavior without corrupting existing output.
- Repeated restart actions must not overwrite previous debug files unintentionally.
- Multiple output writes within one session must remain ordered in the file.
- File output must remain readable even when multiple gameplay events happen in quick succession.
- Board snapshots and attack overlays must still reflect failed-action behavior correctly and must not log fake successful movement or attack results.
- If the output directory does not yet exist, it must be created safely.

## Constraints

- No gameplay rules may move into Unity file-writing code.
- The feature must build on the existing Unity combat debug surface and existing bridge/debug-action path.
- The feature must remain debug/tooling focused.
- Existing out-of-Unity verifier suites must continue to pass unchanged.
- The file-output layer must be optional and configurable.

## Interactions

- Uses the existing Unity Combat Debug Surface feature.
- Uses the existing Unity Listener Bridge and Unity Debug Action Controls.
- Uses the existing event/logging flow.
- Uses authoritative core board state and observed debug output as the source for file content.
- Must coexist with current in-repo runner verification without changing pure core behavior.

## Event Hooks

This feature does not add a new gameplay event system.

It observes the same existing debug-surface outputs driven by core events, including:

- streamed gameplay event logs
- movement-triggered board snapshots
- attack-triggered overlays
- action-count updates

The file-writing layer must preserve the order these outputs are produced in.

## Logging Requirements

- Board text must use bracketed cells like `[ ][P][ ][ ]`.
- Attack overlays must use the same bracketed-cell style, with overlay symbols replacing the underlying tile symbol for that printed view.
- Live Unity Console output must continue to appear.
- When enabled, file output must write the same debug stream in a readable text form.
- The output directory must contain a date-based folder, such as one folder per day.
- Output files inside that folder must use timestamp-based names so runs are separated cleanly.
- File output must remain chronological and readable for debugging replay of a session.

## Acceptance Criteria

- [ ] The board debug output renders with bracketed cells such as `[ ][P][ ][ ]`.
- [ ] Empty, player, enemy, and attack-overlay tiles each render correctly in the bracketed format.
- [ ] Unity Console output still appears live as before.
- [ ] A config setting can enable or disable file writing.
- [ ] When enabled, debug output is written to a text file in real time.
- [ ] The output path includes a date-based folder.
- [ ] Output files are named with timestamps.
- [ ] Restarts or repeated runs do not overwrite prior logs unintentionally.
- [ ] Existing out-of-Unity runner suites still pass.
- [ ] No gameplay behavior changes are introduced.

## AMBIGUITIES

- It is not yet defined whether one file should be created per session, per restart, or per editor run.
- It is not yet defined whether the config should live in a Unity asset, serialized component fields, or a plain config file.
- It is not yet defined whether file output should append continuously to one open file or reopen/write per entry.
- It is not yet defined exactly which timestamp format should be used in file names.
- It is not yet defined whether the output root directory should live under the project folder, a dedicated logs folder, or another debug-output location.
