# Tasks: 07 Unity Combat Debug File Output

## Phase 1 - Core

[x] Define the bracketed board and overlay formatting contract for the existing combat debug surface.
Acceptance:
- Board snapshots render with bracketed cells such as `[ ][P][ ][ ]`.
- Overlay output preserves one symbol per tile in the bracketed format.

[x] Define the optional file-output config and pathing model for dated folders and timestamped files.
Acceptance:
- There is a clear config model for enabling or disabling file writing.
- Output path rules cover dated directories and timestamp-based file names.

[x] Keep the formatting and file-output layers downstream of the existing debug-surface derivation path.
Acceptance:
- No new gameplay logic is introduced.
- Formatting and persistence remain derived from existing observed debug output.

## Phase 2 - Events

[x] Implement bracketed board formatting for movement board snapshots and attack overlays.
Acceptance:
- Movement snapshots use bracketed cells.
- Attack overlays use bracketed cells with `[X]` and `[O]` rules preserved.

[x] Implement ordered real-time file writing for the existing debug stream when file output is enabled.
Acceptance:
- Event lines, action-count lines, board snapshots, and attack overlays are written in order.
- File output remains aligned with Unity Console output.

[x] Implement config-driven enable/disable behavior and initialize dated/timestamped output paths for debug runs.
Acceptance:
- File output can be toggled through a config file.
- Enabled runs create a dated folder and timestamped text file.

## Phase 3 - Edge Cases

[x] Handle disabled file output, missing directories, restart, and repeated runs without corrupting prior logs.
Acceptance:
- Disabled mode writes no files.
- Missing output directories are created safely when needed.
- Restart and repeated runs do not overwrite previous files unintentionally.

[x] Preserve ordered output and readable failure behavior when gameplay actions fail or events occur rapidly.
Acceptance:
- File output stays ordered during rapid event sequences.
- Failed actions do not produce fake successful board or overlay entries.

[x] Keep existing Unity Console output and deterministic core behavior unchanged while adding persistence.
Acceptance:
- Console logging still works when file output is disabled or enabled.
- Existing core/debug-surface behavior remains deterministic.

## Phase 4 - Verification

[x] Verify bracketed formatting, config-driven file output, and ordered text-file writes through the pure verifier path and Unity-side integration.
Acceptance:
- Bracketed board and overlay formatting are validated.
- File output ordering and dated/timestamped path behavior are validated.

[x] Re-run the permanent in-repo core runner to confirm existing verifier suites still pass after the file-output additions.
Acceptance:
- `dotnet run --project Tools/CoreRunner/CoreRunner.csproj` still passes.
- No Unity dependency is introduced into the out-of-Unity runner path.

[x] Update this task file during execution so only one task is marked in progress at a time and completed tasks are reflected here.
Acceptance:
- Task status remains aligned with actual execution progress.
- The file remains the source of truth for implementation progress.
