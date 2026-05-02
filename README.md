# Spherebound

Unity project with a pure C# gameplay core and spec-driven workflow.

## Running The Current Core Scripts

The gameplay core under `Assets/Scripts/CoreCombatLoop` is plain C# and now has a permanent in-repo runner project.

The primary workflow is:

### 1. Run the in-repo `.NET` core runner

From the repo root, run:

```bash
dotnet run --project Tools/CoreRunner/CoreRunner.csproj
```

This executes the current verifier suites:
- `CombatLoopVerifier`
- `ScenarioRunnerVerifier`
- `UnityDebugActionVerifier`
- `CombatDebugSurfaceVerifier`

When the scenario suite runs, the runner also prints full per-scenario event logs and a scenario verification summary for each scenario.

Expected output looks like:

```text
[suite] CombatLoopVerifier
  [pass] VerifyInitialScenario
  [pass] VerifyTurnProgression
  [pass] VerifyPlayerMovement
  [pass] VerifyInvalidMovement
  [pass] VerifyAttackAndDamage
  [pass] VerifyEnemyMovement
  [pass] VerifyEnemyAttack
  [pass] VerifyDeathAndRemoval
  [pass] VerifyOutOfTurnFailure
[suite-pass] CombatLoopVerifier
[suite] ScenarioRunnerVerifier
  [scenario] Move Then Kill
    TurnStarted side=Player
    MoveRequested unit=1 destination=(1, 2)
    ActionStarted unit=1 action=Move
    UnitMoved unit=1 from=(1, 1) to=(1, 2)
    ActionSpent unit=1 remainingActions=1
    ActionEnded unit=1 action=Move
    AttackRequested attacker=1 target=2
    ActionStarted unit=1 action=Attack
    DamageRequested source=1 target=2 amount=1
    UnitDamaged unit=2 previous=1 current=0 amount=1
    UnitDying unit=2
    UnitDeath unit=2
    UnitRemoved unit=2 position=(2, 2)
    ActionSpent unit=1 remainingActions=0
    ActionEnded unit=1 action=Attack
  [scenario-pass] verification
  [scenario] Invalid Move
    TurnStarted side=Player
    MoveRequested unit=1 destination=(1, 2)
    ActionStarted unit=1 action=Move
    ActionFailed unit=1 action=Move reason=DestinationOccupied
    MoveBlocked unit=1 destination=(1, 2) reason=DestinationOccupied
    ActionEnded unit=1 action=Move
  [scenario-pass] verification
  [scenario] Enemy Turn Cycle
    TurnStarted side=Player
    TurnEnded side=Player
    TurnStarted side=Enemy
    MoveRequested unit=2 destination=(4, 3)
    ActionStarted unit=2 action=Move
    UnitMoved unit=2 from=(4, 4) to=(4, 3)
    ActionEnded unit=2 action=Move
    TurnEnded side=Enemy
    TurnStarted side=Player
  [scenario-pass] verification
  [pass] VerifyCatalogScenarios
  [pass] VerifyDeterministicRepeatedRun
  [pass] VerifyMultiScenarioIndependence
[suite-pass] ScenarioRunnerVerifier
[suite] UnityDebugActionVerifier
  [pass] VerifyMoveCommand
  [pass] VerifyAttackCommand
  [pass] VerifyEndTurnCommand
  [pass] VerifyRestartCreatesFreshSession
  [pass] VerifyMissingSessionFailure
[suite-pass] UnityDebugActionVerifier
[suite] CombatDebugSurfaceVerifier
  [pass] VerifyBoardFormatting
  [pass] VerifyMovementBoardOutput
  [pass] VerifyAttackOverlayOutput
  [pass] VerifyFailedAttackDoesNotEmitOverlay
  [pass] VerifyActionCountUpdates
[suite-pass] CombatDebugSurfaceVerifier
[overall-pass] All verifier suites passed.
```

If the runner receives unsupported command-line arguments, it exits with a usage error.

### 2. Let Unity compile it

Open the project in Unity `6000.3.10f1`.

This verifies that the scripts compile as part of the Unity project, but it does not run the combat loop by itself.

## What These Scripts Cover

`Assets/Scripts/CoreCombatLoop/Core`
- Pure gameplay state and combat resolution.

`Assets/Scripts/CoreCombatLoop/Scenarios`
- Deterministic scenario definitions, runner logic, logging, and verification contracts.

`Assets/Scripts/CoreCombatLoop/Verification`
- Verifier suites for the core combat loop, scenario runner, Unity debug-command surface, and combat debug-surface formatting/logging.

`Assets/Scripts/CoreCombatLoop/UnityBridge`
- Unity-facing listener bridge plus debug action controls for move, attack, end turn, and restart.
- Board snapshot and attack overlay formatting for Unity-side combat debugging.

## Notes

- These scripts are designed to run without Unity dependencies.
- The primary command-line entry point is `dotnet run --project Tools/CoreRunner/CoreRunner.csproj`.
- The runner project reuses the existing core/scenario/verifier source files directly from `Assets/Scripts/CoreCombatLoop`.
- The runner now prints full per-scenario logs during the scenario suite.
- The Unity bridge exposes visible Inspector buttons through a custom editor on `UnityCombatListenerBridge` for in-editor manual checks.
- Movement logs now print full board snapshots, and successful attacks print single-symbol attack overlays using `X` and `O`.
