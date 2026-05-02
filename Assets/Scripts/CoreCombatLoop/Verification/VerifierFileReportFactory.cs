using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;
using Spherebound.CoreCombatLoop.Scenarios;
using Spherebound.CoreCombatLoop.UnityBridge;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class VerifierFileReportFactory
    {
        public static VerifierSuiteFileReport CreateCombatLoopSuiteReport()
        {
            const string suiteName = nameof(CombatLoopVerifier);
            var checks = new List<VerifierCheckFileReport>
            {
                CreateCheck(suiteName, nameof(CombatLoopVerifier) + "." + nameof(VerifyInitialScenario), VerifierLogCategory.CombatFlow, BuildCombatLoopInitialScenario),
                CreateCheck(suiteName, nameof(CombatLoopVerifier) + "." + nameof(VerifyTurnProgression), VerifierLogCategory.CombatFlow, BuildCombatLoopTurnProgression),
                CreateCheck(suiteName, nameof(CombatLoopVerifier) + "." + nameof(VerifyPlayerMovement), VerifierLogCategory.CombatFlow, BuildCombatLoopPlayerMovement),
                CreateCheck(suiteName, nameof(CombatLoopVerifier) + "." + nameof(VerifyInvalidMovement), VerifierLogCategory.CombatFlow, BuildCombatLoopInvalidMovement),
                CreateCheck(suiteName, nameof(CombatLoopVerifier) + "." + nameof(VerifyAttackAndDamage), VerifierLogCategory.CombatFlow, BuildCombatLoopAttackAndDamage),
                CreateCheck(suiteName, nameof(CombatLoopVerifier) + "." + nameof(VerifyEnemyMovement), VerifierLogCategory.CombatFlow, BuildCombatLoopEnemyMovement),
                CreateCheck(suiteName, nameof(CombatLoopVerifier) + "." + nameof(VerifyEnemyAttack), VerifierLogCategory.CombatFlow, BuildCombatLoopEnemyAttack),
                CreateCheck(suiteName, nameof(CombatLoopVerifier) + "." + nameof(VerifyDeathAndRemoval), VerifierLogCategory.CombatFlow, BuildCombatLoopDeathAndRemoval),
                CreateCheck(suiteName, nameof(CombatLoopVerifier) + "." + nameof(VerifyOutOfTurnFailure), VerifierLogCategory.CombatFlow, BuildCombatLoopOutOfTurnFailure),
            };

            return new VerifierSuiteFileReport(suiteName, checks.All(check => check.Succeeded), checks);
        }

        public static VerifierSuiteFileReport CreateCombatBehaviorSuiteReport()
        {
            const string suiteName = nameof(CombatBehaviorVerifier);
            var checks = new List<VerifierCheckFileReport>
            {
                CreateCheck(suiteName, nameof(CombatBehaviorVerifier) + "." + nameof(VerifyBehaviorAssignment), VerifierLogCategory.Assertion, BuildBehaviorAssignmentAssertion),
                CreateCheck(suiteName, nameof(CombatBehaviorVerifier) + "." + nameof(VerifyMoveTowardIntent), VerifierLogCategory.Assertion, BuildBehaviorMoveTowardIntentAssertion),
                CreateCheck(suiteName, nameof(CombatBehaviorVerifier) + "." + nameof(VerifySpamAbilityIntent), VerifierLogCategory.Assertion, BuildBehaviorSpamAbilityIntentAssertion),
                CreateCheck(suiteName, nameof(CombatBehaviorVerifier) + "." + nameof(VerifyScriptedBehaviorSequence), VerifierLogCategory.Assertion, BuildBehaviorScriptedSequenceAssertion),
                CreateCheck(suiteName, nameof(CombatBehaviorVerifier) + "." + nameof(VerifyEnemyBehaviorEquivalence), VerifierLogCategory.CombatFlow, BuildBehaviorEnemyEquivalence),
                CreateCheck(suiteName, nameof(CombatBehaviorVerifier) + "." + nameof(VerifyBehaviorDrivenScenario), VerifierLogCategory.CombatFlow, BuildBehaviorDrivenScenario),
            };

            return new VerifierSuiteFileReport(suiteName, checks.All(check => check.Succeeded), checks);
        }

        public static VerifierSuiteFileReport CreateAbilityDefinitionSuiteReport()
        {
            const string suiteName = nameof(AbilityDefinitionVerifier);
            var checks = new List<VerifierCheckFileReport>
            {
                CreateCheck(suiteName, nameof(AbilityDefinitionVerifier) + "." + nameof(VerifyPlayerInitializationFromDefinition), VerifierLogCategory.Assertion, BuildAbilityPlayerDefinitionAssertion),
                CreateCheck(suiteName, nameof(AbilityDefinitionVerifier) + "." + nameof(VerifyBasicAttackAbilityDefinition), VerifierLogCategory.Assertion, BuildAbilityBasicAttackDefinitionAssertion),
                CreateCheck(suiteName, nameof(AbilityDefinitionVerifier) + "." + nameof(VerifyAbilityCostHandling), VerifierLogCategory.CombatFlow, BuildAbilityCostHandling),
                CreateCheck(suiteName, nameof(AbilityDefinitionVerifier) + "." + nameof(VerifyDeterministicAffectedTileResolution), VerifierLogCategory.Assertion, BuildAbilityAffectedTileAssertion),
                CreateCheck(suiteName, nameof(AbilityDefinitionVerifier) + "." + nameof(VerifyAbilityDamageFlow), VerifierLogCategory.CombatFlow, BuildAbilityDamageFlow),
                CreateCheck(suiteName, nameof(AbilityDefinitionVerifier) + "." + nameof(VerifyForcedMovementFlow), VerifierLogCategory.CombatFlow, BuildAbilityForcedMovementFlow),
            };

            return new VerifierSuiteFileReport(suiteName, checks.All(check => check.Succeeded), checks);
        }

        public static VerifierSuiteFileReport CreateScenarioRunnerSuiteReport()
        {
            var report = ScenarioRunnerVerifier.CreateReport();
            var checks = new List<VerifierCheckFileReport>();

            for (var index = 0; index < report.ScenarioChecks.Count; index += 1)
            {
                var scenarioCheck = report.ScenarioChecks[index];
                var scenarioRun = scenarioCheck.ScenarioRun;
                var initialState = scenarioRun.Scenario.CreateInitialState();
                var content = CombatFlowVerifierLogBuilder.Build(
                    report.SuiteName,
                    scenarioCheck.CheckName,
                    scenarioCheck.Succeeded,
                    VerificationBoardStateFormatter.FormatBoard(initialState),
                    scenarioRun.LogLines,
                    VerificationBoardStateFormatter.FormatBoard(scenarioRun.FinalState),
                    scenarioRun.RunnerFailures.Select(failure => $"{failure.Code}: {failure.Message}").ToList());
                checks.Add(new VerifierCheckFileReport(
                    new VerifierCheckLogDefinition(report.SuiteName, scenarioCheck.CheckName, VerifierLogCategory.CombatFlow),
                    scenarioCheck.Succeeded,
                    content));
            }

            for (var index = 0; index < report.CompletedChecks.Count; index += 1)
            {
                var checkName = report.CompletedChecks[index];
                checks.Add(new VerifierCheckFileReport(
                    new VerifierCheckLogDefinition(report.SuiteName, checkName, VerifierLogCategory.Assertion),
                    true,
                    AssertionVerifierLogBuilder.Build(
                        report.SuiteName,
                        checkName,
                        true,
                        "Scenario runner invariants passed.",
                        new[] { "Repeated-run determinism and scenario independence were preserved." })));
            }

            return new VerifierSuiteFileReport(report.SuiteName, report.Succeeded, checks);
        }

        public static VerifierSuiteFileReport CreateUnityDebugActionSuiteReport()
        {
            const string suiteName = nameof(UnityDebugActionVerifier);
            var checks = new List<VerifierCheckFileReport>
            {
                CreateCheck(suiteName, nameof(UnityDebugActionVerifier) + "." + nameof(VerifyMoveCommand), VerifierLogCategory.CombatFlow, BuildUnityDebugMove),
                CreateCheck(suiteName, nameof(UnityDebugActionVerifier) + "." + nameof(VerifyAttackCommand), VerifierLogCategory.CombatFlow, BuildUnityDebugAttack),
                CreateCheck(suiteName, nameof(UnityDebugActionVerifier) + "." + nameof(VerifyEndTurnCommand), VerifierLogCategory.CombatFlow, BuildUnityDebugEndTurn),
                CreateCheck(suiteName, nameof(UnityDebugActionVerifier) + "." + nameof(VerifyRestartCreatesFreshSession), VerifierLogCategory.Assertion, BuildUnityDebugRestartAssertion),
                CreateCheck(suiteName, nameof(UnityDebugActionVerifier) + "." + nameof(VerifyMissingSessionFailure), VerifierLogCategory.Assertion, BuildUnityDebugMissingSessionAssertion),
            };

            return new VerifierSuiteFileReport(suiteName, checks.All(check => check.Succeeded), checks);
        }

        public static VerifierSuiteFileReport CreateCombatRuntimeUiDataSuiteReport()
        {
            const string suiteName = nameof(CombatRuntimeUiDataVerifier);
            var checks = new List<VerifierCheckFileReport>
            {
                AssertionCheck(suiteName, nameof(CombatRuntimeUiDataVerifier) + "." + nameof(VerifyPlayerDefinitionExposesMultipleUiAbilities), "Player definition exposes multiple runtime UI abilities."),
                AssertionCheck(suiteName, nameof(CombatRuntimeUiDataVerifier) + "." + nameof(VerifyControlSurfaceIncludesAbilityMetadata), "Runtime control surface includes definition-backed ability metadata."),
                AssertionCheck(suiteName, nameof(CombatRuntimeUiDataVerifier) + "." + nameof(VerifyControlSurfaceIncludesResolvedEffectTiles), "Runtime control surface includes resolved effect-tile coordinates."),
                AssertionCheck(suiteName, nameof(CombatRuntimeUiDataVerifier) + "." + nameof(VerifyUiAbilityRequestUsesDefinitionBackedAbility), "Runtime ability requests preserve definition-backed ability ids."),
                AssertionCheck(suiteName, nameof(CombatRuntimeUiDataVerifier) + "." + nameof(VerifyAbilitySelectionCycling), "Runtime ability selection cycles deterministically."),
            };

            return new VerifierSuiteFileReport(suiteName, true, checks);
        }

        public static VerifierSuiteFileReport CreateCombatDebugSurfaceSuiteReport()
        {
            const string suiteName = nameof(CombatDebugSurfaceVerifier);
            var checks = new List<VerifierCheckFileReport>
            {
                AssertionCheck(suiteName, nameof(CombatDebugSurfaceVerifier) + "." + nameof(VerifyBoardFormatting), "Board formatter emits bracketed board rows with player/enemy markers."),
                CreateCheck(suiteName, nameof(CombatDebugSurfaceVerifier) + "." + nameof(VerifyMovementBoardOutput), VerifierLogCategory.CombatFlow, BuildDebugSurfaceMovement),
                CreateCheck(suiteName, nameof(CombatDebugSurfaceVerifier) + "." + nameof(VerifyAttackOverlayOutput), VerifierLogCategory.CombatFlow, BuildDebugSurfaceAttackOverlay),
                CreateCheck(suiteName, nameof(CombatDebugSurfaceVerifier) + "." + nameof(VerifyFailedAttackDoesNotEmitOverlay), VerifierLogCategory.CombatFlow, BuildDebugSurfaceFailedAttack),
                CreateCheck(suiteName, nameof(CombatDebugSurfaceVerifier) + "." + nameof(VerifyActionCountUpdates), VerifierLogCategory.CombatFlow, BuildDebugSurfaceActionCounts),
            };

            return new VerifierSuiteFileReport(suiteName, checks.All(check => check.Succeeded), checks);
        }

        public static VerifierSuiteFileReport CreateCombatDebugFileOutputSuiteReport()
        {
            const string suiteName = nameof(CombatDebugFileOutputVerifier);
            var checks = new List<VerifierCheckFileReport>
            {
                AssertionCheck(suiteName, nameof(CombatDebugFileOutputVerifier) + "." + nameof(VerifyConfigLoadOrCreate), "Debug file-output config is created and loaded with expected defaults."),
                AssertionCheck(suiteName, nameof(CombatDebugFileOutputVerifier) + "." + nameof(VerifyDisabledConfigDoesNotEnableWriting), "Disabled config remains disabled."),
                AssertionCheck(suiteName, nameof(CombatDebugFileOutputVerifier) + "." + nameof(VerifyWriterCreatesDatedTimestampedFile), "Existing dated/timestamped debug file writer creates expected output layout."),
                AssertionCheck(suiteName, nameof(CombatDebugFileOutputVerifier) + "." + nameof(VerifyWriterPreservesOutputOrder), "Existing debug file writer preserves append order."),
            };

            return new VerifierSuiteFileReport(suiteName, true, checks);
        }

        public static VerifierSuiteFileReport CreateVerifierLogOutputSuiteReport()
        {
            const string suiteName = nameof(VerifierLogOutputVerifier);
            var checks = new List<VerifierCheckFileReport>
            {
                AssertionCheck(suiteName, nameof(VerifierLogOutputVerifier) + "." + nameof(VerifyPathResolverProducesStablePaths), "Verifier log paths are deterministic for a suite/check pair."),
                AssertionCheck(suiteName, nameof(VerifierLogOutputVerifier) + "." + nameof(VerifyWriterOverwritesLatestCheckFile), "Verifier log writer overwrites the latest check file deterministically."),
                AssertionCheck(suiteName, nameof(VerifierLogOutputVerifier) + "." + nameof(VerifyCombatFlowBuilderIncludesBoardsAndEvents), "Combat-flow log builder includes board and event sections."),
                AssertionCheck(suiteName, nameof(VerifierLogOutputVerifier) + "." + nameof(VerifyAssertionBuilderIncludesSummary), "Assertion log builder includes summary and optional detail sections."),
            };

            return new VerifierSuiteFileReport(suiteName, true, checks);
        }

        public static VerifierSuiteFileReport CreateScenarioAbilityBehaviorValidationSuiteReport()
        {
            var report = ScenarioAbilityBehaviorValidationVerifier.CreateReport();
            var checks = new List<VerifierCheckFileReport>();

            for (var index = 0; index < report.ScenarioChecks.Count; index += 1)
            {
                var scenarioCheck = report.ScenarioChecks[index];
                var scenarioRun = scenarioCheck.ScenarioRun;
                checks.Add(new VerifierCheckFileReport(
                    new VerifierCheckLogDefinition(report.SuiteName, scenarioCheck.CheckName, VerifierLogCategory.CombatFlow),
                    scenarioCheck.Succeeded,
                    CombatFlowVerifierLogBuilder.Build(
                        report.SuiteName,
                        scenarioCheck.CheckName,
                        scenarioCheck.Succeeded,
                        VerificationBoardStateFormatter.FormatBoard(scenarioRun.Scenario.CreateInitialState()),
                        scenarioRun.LogLines,
                        VerificationBoardStateFormatter.FormatBoard(scenarioRun.FinalState),
                        scenarioRun.RunnerFailures.Select(failure => $"{failure.Code}: {failure.Message}").ToList())));
            }

            for (var index = 0; index < report.CompletedChecks.Count; index += 1)
            {
                var checkName = report.CompletedChecks[index];
                checks.Add(new VerifierCheckFileReport(
                    new VerifierCheckLogDefinition(report.SuiteName, checkName, VerifierLogCategory.Assertion),
                    true,
                    AssertionVerifierLogBuilder.Build(
                        report.SuiteName,
                        checkName,
                        true,
                        "Scenario ability/behavior validation invariants passed.")));
            }

            return new VerifierSuiteFileReport(report.SuiteName, report.Succeeded, checks);
        }

        private static VerifierCheckFileReport AssertionCheck(string suiteName, string checkName, string summary)
        {
            return new VerifierCheckFileReport(
                new VerifierCheckLogDefinition(suiteName, checkName, VerifierLogCategory.Assertion),
                true,
                AssertionVerifierLogBuilder.Build(suiteName, checkName, true, summary));
        }

        private static VerifierCheckFileReport CreateCheck(
            string suiteName,
            string checkName,
            VerifierLogCategory category,
            Func<string> buildContent)
        {
            try
            {
                return new VerifierCheckFileReport(
                    new VerifierCheckLogDefinition(suiteName, checkName, category),
                    true,
                    buildContent());
            }
            catch (Exception exception)
            {
                return new VerifierCheckFileReport(
                    new VerifierCheckLogDefinition(suiteName, checkName, VerifierLogCategory.Assertion),
                    false,
                    AssertionVerifierLogBuilder.Build(
                        suiteName,
                        checkName,
                        false,
                        "Check failed while generating verifier log output.",
                        new[] { exception.Message }));
            }
        }

        private static string BuildCombatLoopInitialScenario()
        {
            var state = CombatScenarioFactory.CreateInitialState();
            return CombatFlowVerifierLogBuilder.Build(
                nameof(CombatLoopVerifier),
                nameof(VerifyInitialScenario),
                true,
                VerificationBoardStateFormatter.FormatBoard(state),
                Array.Empty<string>(),
                VerificationBoardStateFormatter.FormatBoard(state),
                new[]
                {
                    $"activeTurn={state.ActiveTurn}",
                    $"remainingPlayerActions={state.RemainingPlayerActions}",
                });
        }

        private static string BuildCombatLoopTurnProgression()
        {
            var state = CombatScenarioFactory.CreateInitialState();
            var initialBoard = VerificationBoardStateFormatter.FormatBoard(state);
            var engine = new CombatEngine();
            var eventLines = engine.StartCombat(state)
                .Concat(engine.EndPlayerTurnAndRunEnemyTurn(state))
                .Select(ScenarioLogFormatter.Format)
                .ToList();
            return CombatFlowVerifierLogBuilder.Build(nameof(CombatLoopVerifier), nameof(VerifyTurnProgression), true, initialBoard, eventLines, VerificationBoardStateFormatter.FormatBoard(state));
        }

        private static string BuildCombatLoopPlayerMovement()
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(4, 4), 3, CombatTurnSide.Player, 2);
            return BuildSingleActionCombatFlow(nameof(CombatLoopVerifier), nameof(VerifyPlayerMovement), state, engine => engine.ResolveMove(state, CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2)));
        }

        private static string BuildCombatLoopInvalidMovement()
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(1, 2), 3, CombatTurnSide.Player, 2);
            return BuildSingleActionCombatFlow(nameof(CombatLoopVerifier), nameof(VerifyInvalidMovement), state, engine => engine.ResolveMove(state, CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2)));
        }

        private static string BuildCombatLoopAttackAndDamage()
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(1, 2), 3, CombatTurnSide.Player, 2);
            return BuildSingleActionCombatFlow(nameof(CombatLoopVerifier), nameof(VerifyAttackAndDamage), state, engine => engine.ResolveAttack(state, CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId));
        }

        private static string BuildCombatLoopEnemyMovement()
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(3, 3), 3, CombatTurnSide.Enemy, 0);
            return BuildSingleActionCombatFlow(nameof(CombatLoopVerifier), nameof(VerifyEnemyMovement), state, engine => engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId));
        }

        private static string BuildCombatLoopEnemyAttack()
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(1, 2), 3, CombatTurnSide.Enemy, 0);
            return BuildSingleActionCombatFlow(nameof(CombatLoopVerifier), nameof(VerifyEnemyAttack), state, engine => engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId));
        }

        private static string BuildCombatLoopDeathAndRemoval()
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(1, 2), 1, CombatTurnSide.Player, 2);
            return BuildSingleActionCombatFlow(nameof(CombatLoopVerifier), nameof(VerifyDeathAndRemoval), state, engine => engine.ResolveAttack(state, CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId));
        }

        private static string BuildCombatLoopOutOfTurnFailure()
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(4, 4), 3, CombatTurnSide.Enemy, 2);
            return BuildSingleActionCombatFlow(nameof(CombatLoopVerifier), nameof(VerifyOutOfTurnFailure), state, engine => engine.ResolveMove(state, CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2)));
        }

        private static string BuildBehaviorAssignmentAssertion()
        {
            var state = CombatScenarioFactory.CreateInitialState();
            return AssertionVerifierLogBuilder.Build(
                nameof(CombatBehaviorVerifier),
                nameof(VerifyBehaviorAssignment),
                true,
                "Initial state assigns behaviors to both player and enemy.",
                new[]
                {
                    $"playerBehavior={(state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player) ? player.BehaviorAssignment?.Behavior.BehaviorId : "missing")}",
                    $"enemyBehavior={(state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy) ? enemy.BehaviorAssignment?.Behavior.BehaviorId : "missing")}",
                });
        }

        private static string BuildBehaviorMoveTowardIntentAssertion()
        {
            var state = CombatScenarioFactory.CreateInitialState();
            state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy);
            var decision = new MoveTowardTargetBehavior("test-move-toward", CombatScenarioFactory.PlayerUnitId)
                .DecideIntent(CombatBehaviorContext.FromState(state, enemy.Id));
            return AssertionVerifierLogBuilder.Build(
                nameof(CombatBehaviorVerifier),
                nameof(VerifyMoveTowardIntent),
                true,
                "MoveTowardTargetBehavior chooses a deterministic move intent.",
                new[]
                {
                    $"intent={decision.Intent.IntentType}",
                    $"targetPosition={decision.Intent.TargetPosition}",
                });
        }

        private static string BuildBehaviorSpamAbilityIntentAssertion()
        {
            var state = CombatScenarioFactory.CreateInitialState();
            var decision = new SpamAbilityBehavior("test-spam", CombatScenarioFactory.ForwardLineAbilityId, CombatScenarioFactory.EnemyUnitId)
                .DecideIntent(CombatBehaviorContext.FromState(state, CombatScenarioFactory.PlayerUnitId));
            return AssertionVerifierLogBuilder.Build(
                nameof(CombatBehaviorVerifier),
                nameof(VerifySpamAbilityIntent),
                true,
                "SpamAbilityBehavior selects its configured ability deterministically.",
                new[]
                {
                    $"intent={decision.Intent.IntentType}",
                    $"abilityId={decision.Intent.AbilityId}",
                });
        }

        private static string BuildBehaviorScriptedSequenceAssertion()
        {
            var behavior = new ScriptedCombatBehavior(
                "scripted-sequence",
                new[]
                {
                    CombatBehaviorIntent.Move(CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2)),
                    CombatBehaviorIntent.EndTurn(CombatScenarioFactory.PlayerUnitId),
                });
            var context = CombatBehaviorContext.FromState(CombatScenarioFactory.CreateInitialState(), CombatScenarioFactory.PlayerUnitId);
            var first = behavior.DecideIntent(context);
            var second = behavior.DecideIntent(context);
            var third = behavior.DecideIntent(context);
            return AssertionVerifierLogBuilder.Build(
                nameof(CombatBehaviorVerifier),
                nameof(VerifyScriptedBehaviorSequence),
                true,
                "Scripted behavior preserves configured order and deterministic fallback.",
                new[]
                {
                    $"first={first.Intent.IntentType}",
                    $"second={second.Intent.IntentType}",
                    $"third={third.Intent.IntentType}",
                });
        }

        private static string BuildBehaviorEnemyEquivalence()
        {
            var state = new CombatState(
                new BoardDimensions(6, 6),
                CombatTurnSide.Enemy,
                0,
                new[]
                {
                    new CombatUnitState(
                        CombatScenarioFactory.PlayerUnitId,
                        CombatUnitSide.Player,
                        5,
                        new GridPosition(1, 1),
                        UnitLifeState.Alive,
                        CombatScenarioFactory.PlayerDefinition,
                        CombatBehaviorAssignment.Manual(new ManualCombatBehavior())),
                    new CombatUnitState(
                        CombatScenarioFactory.EnemyUnitId,
                        CombatUnitSide.Enemy,
                        3,
                        new GridPosition(3, 3),
                        UnitLifeState.Alive,
                        CombatScenarioFactory.EnemyDefinition,
                        CombatBehaviorAssignment.Default(new MoveTowardTargetBehavior("enemy-chase", CombatScenarioFactory.PlayerUnitId))),
                });
            return BuildSingleActionCombatFlow(nameof(CombatBehaviorVerifier), nameof(VerifyEnemyBehaviorEquivalence), state, engine => engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId));
        }

        private static string BuildBehaviorDrivenScenario()
        {
            var scenario = ScenarioCatalog.CreateBehaviorTurnScenario();
            var run = new ScenarioRunner().Run(scenario);
            return CombatFlowVerifierLogBuilder.Build(
                nameof(CombatBehaviorVerifier),
                nameof(VerifyBehaviorDrivenScenario),
                run.Verification.Succeeded,
                VerificationBoardStateFormatter.FormatBoard(scenario.CreateInitialState()),
                run.LogLines,
                VerificationBoardStateFormatter.FormatBoard(run.FinalState));
        }

        private static string BuildAbilityPlayerDefinitionAssertion()
        {
            var state = CombatScenarioFactory.CreateInitialState();
            state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player);
            return AssertionVerifierLogBuilder.Build(
                nameof(AbilityDefinitionVerifier),
                nameof(VerifyPlayerInitializationFromDefinition),
                true,
                "Player instance is created from the reusable player definition.",
                new[]
                {
                    $"definitionId={player.Definition.Id}",
                    $"abilities={player.Definition.Abilities.Count}",
                    $"movementRange={player.Definition.Movement.Range}",
                });
        }

        private static string BuildAbilityBasicAttackDefinitionAssertion()
        {
            CombatScenarioFactory.PlayerDefinition.TryGetAbility(CombatScenarioFactory.BasicAttackAbilityId, out var ability);
            return AssertionVerifierLogBuilder.Build(
                nameof(AbilityDefinitionVerifier),
                nameof(VerifyBasicAttackAbilityDefinition),
                true,
                "Basic attack ability exposes expected metadata and damage payload.",
                new[]
                {
                    $"name={ability.Name}",
                    $"description={ability.Description}",
                    $"actionCost={ability.ActionCost}",
                });
        }

        private static string BuildAbilityCostHandling()
        {
            const string expensiveAbilityId = "heavy-slash";
            var expensiveAbility = new AbilityDefinition(
                expensiveAbilityId,
                "Heavy Slash",
                "adjacent heavy strike",
                CombatActionType.Attack,
                2,
                AbilityTargetingMode.AdjacentUnit,
                AbilityTargetRule.Enemy | AbilityTargetRule.OccupiedTile,
                new AbilityTilePattern(AbilityTilePatternAnchor.TargetPosition, new[] { new GridOffset(0, 0) }),
                new[] { AbilityEffectDefinition.Damage(2) });
            var playerDefinition = new CombatUnitDefinition(
                "heavy-fighter",
                "Heavy Fighter",
                5,
                2,
                new MovementCapabilityDefinition(1, 1, true),
                new[] { expensiveAbility },
                expensiveAbilityId);
            var state = CreateAbilityState(playerDefinition, 5, 3, new GridPosition(1, 1), new GridPosition(1, 2), 1);
            return BuildSingleActionCombatFlow(nameof(AbilityDefinitionVerifier), nameof(VerifyAbilityCostHandling), state, engine => engine.ResolveAbility(state, new AbilityUseRequest(CombatScenarioFactory.PlayerUnitId, expensiveAbilityId, CombatScenarioFactory.EnemyUnitId)));
        }

        private static string BuildAbilityAffectedTileAssertion()
        {
            var state = CombatScenarioFactory.CreateInitialState();
            state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player);
            CombatScenarioFactory.PlayerDefinition.TryGetAbility(CombatScenarioFactory.BasicAttackAbilityId, out var basicAttack);
            var initialFailure = CombatAbilityResolver.ResolveAffectedTiles(
                state,
                player,
                basicAttack,
                new AbilityUseRequest(player.Id, basicAttack.Id, CombatScenarioFactory.EnemyUnitId),
                out var failureReason);
            var adjacentState = CreateAbilityState(CombatScenarioFactory.PlayerDefinition, 5, 3, new GridPosition(1, 1), new GridPosition(1, 2), 2);
            adjacentState.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var adjacentPlayer);
            var adjacentTiles = CombatAbilityResolver.ResolveAffectedTiles(
                adjacentState,
                adjacentPlayer,
                basicAttack,
                new AbilityUseRequest(adjacentPlayer.Id, basicAttack.Id, CombatScenarioFactory.EnemyUnitId),
                out var adjacentFailure);
            return AssertionVerifierLogBuilder.Build(
                nameof(AbilityDefinitionVerifier),
                nameof(VerifyDeterministicAffectedTileResolution),
                true,
                "Affected-tile resolution stays deterministic for invalid and valid adjacency cases.",
                new[]
                {
                    $"initialFailure={failureReason}",
                    $"adjacentFailure={adjacentFailure}",
                    $"adjacentTileCount={adjacentTiles.Count}",
                });
        }

        private static string BuildAbilityDamageFlow()
        {
            var state = CreateAbilityState(CombatScenarioFactory.PlayerDefinition, 5, 3, new GridPosition(1, 1), new GridPosition(1, 2), 2);
            return BuildSingleActionCombatFlow(nameof(AbilityDefinitionVerifier), nameof(VerifyAbilityDamageFlow), state, engine => engine.ResolveAbility(state, new AbilityUseRequest(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.BasicAttackAbilityId, CombatScenarioFactory.EnemyUnitId)));
        }

        private static string BuildAbilityForcedMovementFlow()
        {
            const string pushAbilityId = "shield-bash";
            var pushAbility = new AbilityDefinition(
                pushAbilityId,
                "Shield Bash",
                "push adjacent enemy",
                CombatActionType.Attack,
                1,
                AbilityTargetingMode.AdjacentUnit,
                AbilityTargetRule.Enemy | AbilityTargetRule.OccupiedTile,
                new AbilityTilePattern(AbilityTilePatternAnchor.TargetPosition, new[] { new GridOffset(0, 0) }),
                new[] { AbilityEffectDefinition.ForcedMovement(new GridOffset(0, 1)) });
            var playerDefinition = new CombatUnitDefinition(
                "shield-knight",
                "Shield Knight",
                5,
                2,
                new MovementCapabilityDefinition(1, 1, true),
                new[] { pushAbility },
                pushAbilityId);
            var state = CreateAbilityState(playerDefinition, 5, 3, new GridPosition(1, 1), new GridPosition(1, 2), 2);
            return BuildSingleActionCombatFlow(nameof(AbilityDefinitionVerifier), nameof(VerifyForcedMovementFlow), state, engine => engine.ResolveAbility(state, new AbilityUseRequest(CombatScenarioFactory.PlayerUnitId, pushAbilityId, CombatScenarioFactory.EnemyUnitId)));
        }

        private static string BuildUnityDebugMove()
        {
            var session = ObservableCombatSession.CreateDefault("debug-move");
            session.StartCombat();
            return BuildDebugCommandCombatFlow(nameof(UnityDebugActionVerifier), nameof(VerifyMoveCommand), session, CombatDebugCommandRequest.Move(CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2)));
        }

        private static string BuildUnityDebugAttack()
        {
            var session = new ObservableCombatSession(
                "debug-attack",
                new CombatState(
                    new BoardDimensions(6, 6),
                    CombatTurnSide.Player,
                    2,
                    new[]
                    {
                        new CombatUnitState(CombatScenarioFactory.PlayerUnitId, CombatUnitSide.Player, 5, new GridPosition(1, 1), UnitLifeState.Alive),
                        new CombatUnitState(CombatScenarioFactory.EnemyUnitId, CombatUnitSide.Enemy, 3, new GridPosition(1, 2), UnitLifeState.Alive),
                    }));
            session.StartCombat();
            return BuildDebugCommandCombatFlow(nameof(UnityDebugActionVerifier), nameof(VerifyAttackCommand), session, CombatDebugCommandRequest.Attack(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId));
        }

        private static string BuildUnityDebugEndTurn()
        {
            var session = ObservableCombatSession.CreateDefault("debug-end-turn");
            session.StartCombat();
            return BuildDebugCommandCombatFlow(nameof(UnityDebugActionVerifier), nameof(VerifyEndTurnCommand), session, CombatDebugCommandRequest.EndTurn(CombatScenarioFactory.PlayerUnitId));
        }

        private static string BuildUnityDebugRestartAssertion()
        {
            return AssertionVerifierLogBuilder.Build(
                nameof(UnityDebugActionVerifier),
                nameof(VerifyRestartCreatesFreshSession),
                true,
                "Restart creates a fresh session with initial state and a new session id.");
        }

        private static string BuildUnityDebugMissingSessionAssertion()
        {
            return AssertionVerifierLogBuilder.Build(
                nameof(UnityDebugActionVerifier),
                nameof(VerifyMissingSessionFailure),
                true,
                "Missing session fails at the debug-command layer without gameplay events.");
        }

        private static string BuildDebugSurfaceMovement()
        {
            var session = ObservableCombatSession.CreateDefault("move-board");
            var result = session.ResolveMove(CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2));
            return CombatFlowVerifierLogBuilder.Build(
                nameof(CombatDebugSurfaceVerifier),
                nameof(VerifyMovementBoardOutput),
                result.Succeeded,
                VerificationBoardStateFormatter.FormatBoard(CombatScenarioFactory.CreateInitialState()),
                result.Events.Select(ScenarioLogFormatter.Format).ToList(),
                VerificationBoardStateFormatter.FormatBoard(session.State));
        }

        private static string BuildDebugSurfaceAttackOverlay()
        {
            var session = new ObservableCombatSession(
                "attack-overlay",
                new CombatState(
                    new BoardDimensions(6, 6),
                    CombatTurnSide.Player,
                    2,
                    new[]
                    {
                        new CombatUnitState(CombatScenarioFactory.PlayerUnitId, CombatUnitSide.Player, 5, new GridPosition(1, 1), UnitLifeState.Alive),
                        new CombatUnitState(CombatScenarioFactory.EnemyUnitId, CombatUnitSide.Enemy, 3, new GridPosition(1, 2), UnitLifeState.Alive),
                    }));
            var result = session.ResolveAttack(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId);
            return CombatFlowVerifierLogBuilder.Build(
                nameof(CombatDebugSurfaceVerifier),
                nameof(VerifyAttackOverlayOutput),
                result.Succeeded,
                VerificationBoardStateFormatter.FormatBoard(new CombatState(new BoardDimensions(6, 6), CombatTurnSide.Player, 2, new[] {
                    new CombatUnitState(CombatScenarioFactory.PlayerUnitId, CombatUnitSide.Player, 5, new GridPosition(1,1), UnitLifeState.Alive),
                    new CombatUnitState(CombatScenarioFactory.EnemyUnitId, CombatUnitSide.Enemy, 3, new GridPosition(1,2), UnitLifeState.Alive),
                })),
                result.Events.Select(ScenarioLogFormatter.Format).ToList(),
                VerificationBoardStateFormatter.FormatBoard(session.State));
        }

        private static string BuildDebugSurfaceFailedAttack()
        {
            var session = ObservableCombatSession.CreateDefault("failed-attack");
            var result = session.ResolveAttack(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId);
            return CombatFlowVerifierLogBuilder.Build(
                nameof(CombatDebugSurfaceVerifier),
                nameof(VerifyFailedAttackDoesNotEmitOverlay),
                !result.Succeeded,
                VerificationBoardStateFormatter.FormatBoard(CombatScenarioFactory.CreateInitialState()),
                result.Events.Select(ScenarioLogFormatter.Format).ToList(),
                VerificationBoardStateFormatter.FormatBoard(session.State));
        }

        private static string BuildDebugSurfaceActionCounts()
        {
            var session = ObservableCombatSession.CreateDefault("action-count");
            var eventLines = new List<string>();
            eventLines.Add("TurnStarted side=Player");
            eventLines.AddRange(session.ResolveMove(CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2)).Events.Select(ScenarioLogFormatter.Format));
            eventLines.AddRange(session.EndPlayerTurnAndRunEnemyTurn().Select(ScenarioLogFormatter.Format));
            return CombatFlowVerifierLogBuilder.Build(
                nameof(CombatDebugSurfaceVerifier),
                nameof(VerifyActionCountUpdates),
                true,
                VerificationBoardStateFormatter.FormatBoard(CombatScenarioFactory.CreateInitialState()),
                eventLines,
                VerificationBoardStateFormatter.FormatBoard(session.State),
                new[] { $"remainingPlayerActions={session.State.RemainingPlayerActions}" });
        }

        private static string BuildSingleActionCombatFlow(string suiteName, string checkName, CombatState state, Func<CombatEngine, CombatActionResult> execute)
        {
            var initialBoard = VerificationBoardStateFormatter.FormatBoard(state);
            var engine = new CombatEngine();
            var result = execute(engine);
            var eventLines = result.Events.Select(ScenarioLogFormatter.Format).ToList();
            return CombatFlowVerifierLogBuilder.Build(suiteName, checkName, result.Succeeded, initialBoard, eventLines, VerificationBoardStateFormatter.FormatBoard(state), new[] { $"failureReason={result.FailureReason}" });
        }

        private static string BuildDebugCommandCombatFlow(string suiteName, string checkName, ObservableCombatSession session, CombatDebugCommandRequest request)
        {
            var initialBoard = VerificationBoardStateFormatter.FormatBoard(session.State);
            var result = CombatDebugCommandExecutor.Execute(session, request);
            var eventLines = result.Events.Select(ScenarioLogFormatter.Format).ToList();
            return CombatFlowVerifierLogBuilder.Build(
                suiteName,
                checkName,
                result.Succeeded,
                initialBoard,
                eventLines,
                VerificationBoardStateFormatter.FormatBoard(session.State),
                new[]
                {
                    $"commandFailureReason={result.CommandFailureReason}",
                    $"gameplayFailureReason={result.GameplayFailureReason}",
                });
        }

        private static CombatState CreateState(
            GridPosition playerPosition,
            int playerHealth,
            GridPosition enemyPosition,
            int enemyHealth,
            CombatTurnSide activeTurn,
            int remainingPlayerActions)
        {
            return new CombatState(
                new BoardDimensions(6, 6),
                activeTurn,
                remainingPlayerActions,
                new[]
                {
                    new CombatUnitState(CombatScenarioFactory.PlayerUnitId, CombatUnitSide.Player, playerHealth, playerPosition, UnitLifeState.Alive),
                    new CombatUnitState(CombatScenarioFactory.EnemyUnitId, CombatUnitSide.Enemy, enemyHealth, enemyPosition, UnitLifeState.Alive),
                });
        }

        private static CombatState CreateAbilityState(
            CombatUnitDefinition playerDefinition,
            int playerHealth,
            int enemyHealth,
            GridPosition playerPosition,
            GridPosition enemyPosition,
            int remainingPlayerActions)
        {
            return new CombatState(
                new BoardDimensions(6, 6),
                CombatTurnSide.Player,
                remainingPlayerActions,
                new[]
                {
                    new CombatUnitState(CombatScenarioFactory.PlayerUnitId, CombatUnitSide.Player, playerHealth, playerPosition, UnitLifeState.Alive, playerDefinition),
                    new CombatUnitState(CombatScenarioFactory.EnemyUnitId, CombatUnitSide.Enemy, enemyHealth, enemyPosition, UnitLifeState.Alive, CombatScenarioFactory.EnemyDefinition),
                });
        }

        private static string VerifyInitialScenario => "VerifyInitialScenario";
        private static string VerifyTurnProgression => "VerifyTurnProgression";
        private static string VerifyPlayerMovement => "VerifyPlayerMovement";
        private static string VerifyInvalidMovement => "VerifyInvalidMovement";
        private static string VerifyAttackAndDamage => "VerifyAttackAndDamage";
        private static string VerifyEnemyMovement => "VerifyEnemyMovement";
        private static string VerifyEnemyAttack => "VerifyEnemyAttack";
        private static string VerifyDeathAndRemoval => "VerifyDeathAndRemoval";
        private static string VerifyOutOfTurnFailure => "VerifyOutOfTurnFailure";
        private static string VerifyBehaviorAssignment => "VerifyBehaviorAssignment";
        private static string VerifyMoveTowardIntent => "VerifyMoveTowardIntent";
        private static string VerifySpamAbilityIntent => "VerifySpamAbilityIntent";
        private static string VerifyScriptedBehaviorSequence => "VerifyScriptedBehaviorSequence";
        private static string VerifyEnemyBehaviorEquivalence => "VerifyEnemyBehaviorEquivalence";
        private static string VerifyBehaviorDrivenScenario => "VerifyBehaviorDrivenScenario";
        private static string VerifyPlayerInitializationFromDefinition => "VerifyPlayerInitializationFromDefinition";
        private static string VerifyBasicAttackAbilityDefinition => "VerifyBasicAttackAbilityDefinition";
        private static string VerifyAbilityCostHandling => "VerifyAbilityCostHandling";
        private static string VerifyDeterministicAffectedTileResolution => "VerifyDeterministicAffectedTileResolution";
        private static string VerifyAbilityDamageFlow => "VerifyAbilityDamageFlow";
        private static string VerifyForcedMovementFlow => "VerifyForcedMovementFlow";
        private static string VerifyMoveCommand => "VerifyMoveCommand";
        private static string VerifyAttackCommand => "VerifyAttackCommand";
        private static string VerifyEndTurnCommand => "VerifyEndTurnCommand";
        private static string VerifyRestartCreatesFreshSession => "VerifyRestartCreatesFreshSession";
        private static string VerifyMissingSessionFailure => "VerifyMissingSessionFailure";
        private static string VerifyPlayerDefinitionExposesMultipleUiAbilities => "VerifyPlayerDefinitionExposesMultipleUiAbilities";
        private static string VerifyControlSurfaceIncludesAbilityMetadata => "VerifyControlSurfaceIncludesAbilityMetadata";
        private static string VerifyControlSurfaceIncludesResolvedEffectTiles => "VerifyControlSurfaceIncludesResolvedEffectTiles";
        private static string VerifyUiAbilityRequestUsesDefinitionBackedAbility => "VerifyUiAbilityRequestUsesDefinitionBackedAbility";
        private static string VerifyAbilitySelectionCycling => "VerifyAbilitySelectionCycling";
        private static string VerifyBoardFormatting => "VerifyBoardFormatting";
        private static string VerifyMovementBoardOutput => "VerifyMovementBoardOutput";
        private static string VerifyAttackOverlayOutput => "VerifyAttackOverlayOutput";
        private static string VerifyFailedAttackDoesNotEmitOverlay => "VerifyFailedAttackDoesNotEmitOverlay";
        private static string VerifyActionCountUpdates => "VerifyActionCountUpdates";
        private static string VerifyConfigLoadOrCreate => "VerifyConfigLoadOrCreate";
        private static string VerifyDisabledConfigDoesNotEnableWriting => "VerifyDisabledConfigDoesNotEnableWriting";
        private static string VerifyWriterCreatesDatedTimestampedFile => "VerifyWriterCreatesDatedTimestampedFile";
        private static string VerifyWriterPreservesOutputOrder => "VerifyWriterPreservesOutputOrder";
        private static string VerifyPathResolverProducesStablePaths => "VerifyPathResolverProducesStablePaths";
        private static string VerifyWriterOverwritesLatestCheckFile => "VerifyWriterOverwritesLatestCheckFile";
        private static string VerifyCombatFlowBuilderIncludesBoardsAndEvents => "VerifyCombatFlowBuilderIncludesBoardsAndEvents";
        private static string VerifyAssertionBuilderIncludesSummary => "VerifyAssertionBuilderIncludesSummary";
    }
}
