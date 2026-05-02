# Tasks: 11 Verifier Log Output Files

## Phase 1 - Shared Output Infrastructure

[x] Define the shared verifier log contract, log categories, and deterministic per-suite/per-check output path model beside each verifier's code.
Acceptance:
- The verification layer has a clear distinction between combat-flow logs and small component assertion logs.
- A shared path model can resolve one stable overwritten file per suite/check pair in a colocated verifier-specific log directory.
- The contract stays independent from Unity and does not change verifier execution rules.

[x] Define reusable text builders for combat-flow logs and compact assertion logs.
Acceptance:
- Combat-flow verifiers have a shared way to describe initial board, event stream, board snapshots, and final board.
- Small component verifiers have a shared way to describe setup and assertions concisely.

[x] Define a shared verifier log writer that creates verifier-specific colocated log directories and overwrites the latest check file deterministically.
Acceptance:
- The writer creates deterministic colocated suite/check file paths.
- Re-running the same check overwrites the same file.

## Phase 2 - Verifier Integration

[x] Integrate per-check file output into the combat-state progression verifiers.
Acceptance:
- Combat-flow verifiers emit rich board/event logs.
- Output shows initial board, meaningful transitions, and final board.

[x] Integrate per-check file output into the smaller component verifiers using compact assertion logs.
Acceptance:
- Small component verifiers emit concise assertion-oriented logs.
- They do not produce noisy board dumps when no combat-state progression exists.

[x] Integrate the shared output flow into the permanent in-repo runner without creating a separate test execution path.
Acceptance:
- The runner still prints console results normally.
- File output is generated as an additive side effect of the same verifier execution path.

## Phase 3 - Edge Cases

[x] Handle checks with no combat events, failing verifiers, existing output directories, and suites with multiple checks or multiple scenarios.
Acceptance:
- Output remains readable and deterministic across those cases.
- Failures still produce inspectable files where possible.

[x] Keep the output model compatible with scenario-based verifiers that already emit richer event logs.
Acceptance:
- Existing scenario log fidelity is preserved or improved.
- Behavior-decision events and board snapshots remain visible when present.

[x] Preserve out-of-Unity execution and avoid Unity dependencies in the verifier log layer.
Acceptance:
- The log infrastructure works from the permanent runner alone.
- No Unity types are introduced.

## Phase 4 - Verification And Steering

[x] Add verifier coverage for deterministic path resolution, overwrite behavior, and category-specific text generation.
Acceptance:
- The new infrastructure is covered by pure verifier tests.
- File output determinism is explicitly checked.

[x] Re-run the permanent in-repo core runner and confirm all suites still pass while generating verifier log files.
Acceptance:
- `dotnet run --project Tools/CoreRunner/CoreRunner.csproj` still passes.
- The latest file outputs are generated for all relevant checks.

[x] Update `AGENTS.md`, this task file, and `specDev/current-state.md` so future tests follow the same logging rule by default.
Acceptance:
- Steering explicitly says combat-state tests emit rich board/event logs.
- Steering explicitly says small component tests emit compact assertion logs.
- The holistic current-state document reflects the verifier log-file capability.
