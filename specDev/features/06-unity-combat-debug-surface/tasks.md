# Tasks: 06 Unity Combat Debug Surface

## Phase 1 - Core

[x] Define the board-snapshot and attack-overlay formatting model used to turn authoritative combat state into readable debug grid output.
Acceptance:
- There is a clear formatting contract for full board snapshots and attack overlays.
- The formatting model stays derived from authoritative core state rather than becoming a second source of truth.

[x] Define the visible Unity debug-surface contract that exposes basic controls while reusing the existing bridge/debug-action path.
Acceptance:
- Unity has a visible debug-surface contract for initialize, start, move, attack, end turn, and restart.
- The visible control layer does not introduce gameplay rules or a second command path.

[x] Keep the board/overlay output layer and visible control surface debug-focused and replaceable.
Acceptance:
- The feature remains Editor/debug tooling rather than runtime gameplay UI.
- The design does not force future player-facing UI architecture decisions.

## Phase 2 - Events

[x] Implement readable full-board snapshot output that prints after resolved player or enemy movement.
Acceptance:
- Movement-triggered board output shows the whole grid.
- The board output reflects resolved authoritative state after movement.
- Player and enemy positions are readable as `P` and `E`.

[x] Implement attack-overlay output that prints after attack resolution using one symbol per tile.
Acceptance:
- Attack output marks affected/path tiles with `X`.
- The hit or collision tile is marked with `O`.
- `O` replaces `X` on the hit tile and overlay symbols replace underlying board symbols for that printout.

[x] Implement visible logging of remaining available player actions and keep it synchronized with streamed gameplay events.
Acceptance:
- Remaining actions appear in the logs.
- The logged action count updates when actions are spent or reset.
- Event ordering remains readable and deterministic.

## Phase 3 - Edge Cases

[x] Handle failed movement, failed attacks, restart, and rapid event sequences without producing misleading board or overlay output.
Acceptance:
- Failed actions still show normal failure events.
- Failed actions do not print fake successful board/attack results.
- Restart clears stale board/overlay state and returns to fresh-session output.

[x] Prevent duplicate subscriptions, duplicate command execution, or stale display output during repeated debug use.
Acceptance:
- Re-enable/restart does not duplicate logging or board prints.
- Old session state does not remain visible after session replacement.

[x] Preserve deterministic core behavior and keep the existing out-of-Unity runner workflows unchanged.
Acceptance:
- Existing core/scenario/debug-action verifier behavior remains valid.
- The Unity debug surface does not alter core event order or enemy behavior.

## Phase 4 - Verification

[x] Verify Unity can show the board, stream events, show action-count updates, and print attack overlays while using the existing debug controls.
Acceptance:
- The visible Unity debug surface can drive the current combat loop manually.
- Movement board output, attack overlay output, and action-count logging are all observable.

[x] Re-run the permanent in-repo core runner to confirm existing verifier suites still pass after the Unity combat debug-surface additions.
Acceptance:
- `dotnet run --project Tools/CoreRunner/CoreRunner.csproj` still passes.
- No Unity dependency is introduced into the out-of-Unity runner path.

[x] Update this task file during execution so only one task is marked in progress at a time and completed tasks are reflected here.
Acceptance:
- Task status remains aligned with actual execution progress.
- The file remains the source of truth for implementation progress.
