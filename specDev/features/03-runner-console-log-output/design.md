# Design: 03 Runner Console Log Output

## Flow

Runner Console Log Output extends the existing in-repo core runner so it prints full scenario event logs during scenario-suite execution.

High-level flow:
- Run the in-repo core runner with the existing stable command.
- Execute the combat verifier suite as before.
- Execute the scenario runner verifier suite.
- For each scenario run in the scenario suite:
  - print a clear scenario header including the scenario name
  - print the ordered scenario event log lines already produced by the scenario runner
  - print scenario verification outcome information
- Continue printing suite and overall success/failure summaries.

The runner remains a host/orchestration layer. It does not create new gameplay behavior or alternate scenario execution paths.

## Event Flow

### Request Phase

The runner does not create new gameplay requests.

Its responsibility is to invoke existing scenario execution and consume the resulting scenario log output.

Request-side rules:
- use the current scenario runner and scenario catalog
- do not reconstruct scenario actions manually in the in-repo runner
- use existing scenario execution outputs as the source for console log sections

### Resolution Phase

Resolution remains owned by the existing combat core and scenario runner.

Runner responsibilities during resolution:
- call existing scenario/verifier entry points
- read scenario run results and their generated log lines
- print those log lines in deterministic order

Scenario/core responsibilities during resolution:
- execute gameplay rules
- capture gameplay events
- produce deterministic log lines
- produce verification results

### Result Phase

The in-repo runner outputs:
- suite headers
- scenario headers
- per-scenario log lines
- scenario verification summaries
- suite pass/fail summaries
- overall pass/fail summary

Result rules:
- scenario logs preserve exact event order from `ScenarioRunResult.LogLines`
- scenario sections stay clearly separated
- suite/overall exit behavior remains unchanged

## State Changes

The feature adds no new gameplay state.

Runner-owned additions include:
- scenario console section formatting
- scenario verification summary formatting
- any top-level reporting structures needed to expose scenario run data to the runner

State rules:
- gameplay state remains owned by the scenario runner/core
- printed logs are derived from existing scenario results
- repeated runs must not share or mutate reporting state across executions

## Effect Interactions

No new effect behavior is introduced.

The feature only exposes existing scenario logs and verification results through console output.

## Failure Conditions

Failure cases the runner must handle:
- scenario execution succeeds but verification fails
- a scenario contains gameplay failures that are already represented in log lines
- a suite fails after some scenario logs have already been printed
- scenario-reporting data is unavailable due to host-level error

Failure handling rules:
- printed scenario logs must still appear for scenarios that ran
- verification failure output must be readable alongside scenario logs
- host-level failure reporting must remain distinguishable from scenario/gameplay failure output
- non-zero exit codes remain unchanged from feature `02`

## Logging

Console output is extended, not replaced.

Logging rules:
- print the scenario name before that scenario’s log lines
- print each scenario log line exactly as produced by the scenario runner/log formatter
- preserve deterministic order of scenarios and log lines
- keep suite/check output readable around the scenario sections
- include verification-related output for each scenario in a readable form

The design assumes:
- scenario logs print by default when the scenario suite runs
- no new flag is required for the initial version

## System Impact

This feature improves the operational visibility of the existing pure C# runner stack.

It enables:
- direct inspection of full scenario event logs from the permanent in-repo runner
- easier debugging of movement, attack, damage, death, failure, and verification behavior
- better alignment between generated scenario logs and command-line output

It must not:
- modify gameplay behavior
- duplicate scenario logic
- introduce Unity dependencies
- replace the scenario runner as the source of log generation

Open design limits for future phases:
- optional filtering/toggling of scenario log output is deferred
- direct scenario-only CLI modes are deferred
