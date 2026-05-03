using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;
using Spherebound.CoreCombatLoop.Scenarios;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class CombatBehaviorVerifier
    {
        public static IReadOnlyList<string> RunAll()
        {
            var completedChecks = new List<string>();

            VerifyBehaviorAssignment(completedChecks);
            VerifyMoveTowardIntent(completedChecks);
            VerifySpamAbilityIntent(completedChecks);
            VerifyScriptedBehaviorSequence(completedChecks);
            VerifyEnemyBehaviorEquivalence(completedChecks);
            VerifyBehaviorDrivenScenario(completedChecks);

            return completedChecks;
        }

        private static void VerifyBehaviorAssignment(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();
            Ensure(state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player), "Initial state should contain player.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy), "Initial state should contain enemy.");
            Ensure(player.BehaviorAssignment != null, "Player should have a pluggable manual behavior assignment.");
            Ensure(enemy.BehaviorAssignment != null, "Enemy should have a default behavior assignment.");
            completedChecks.Add(nameof(VerifyBehaviorAssignment));
        }

        private static void VerifyMoveTowardIntent(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();
            Ensure(state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy), "Initial state should contain enemy.");
            var behavior = new MoveTowardTargetBehavior("test-move-toward", CombatScenarioFactory.PlayerUnitId);
            var context = CombatBehaviorContext.FromState(state, enemy.Id);
            var decision = behavior.DecideIntent(context);
            Ensure(decision.Intent.IntentType == CombatBehaviorIntentType.Move, "MoveTowardTargetBehavior should produce a move intent when not adjacent.");
            Ensure(decision.Intent.TargetPosition.HasValue && decision.Intent.TargetPosition.Value.X == 2 && decision.Intent.TargetPosition.Value.Y == 2, "MoveTowardTargetBehavior should prefer vertical movement first.");
            completedChecks.Add(nameof(VerifyMoveTowardIntent));
        }

        private static void VerifySpamAbilityIntent(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();
            var behavior = new SpamAbilityBehavior("test-spam", CombatScenarioFactory.ForwardLineAbilityId, CombatScenarioFactory.EnemyUnitId);
            var context = CombatBehaviorContext.FromState(state, CombatScenarioFactory.PlayerUnitId);
            var decision = behavior.DecideIntent(context);
            Ensure(decision.Intent.IntentType == CombatBehaviorIntentType.UseAbility, "SpamAbilityBehavior should produce an ability intent.");
            Ensure(decision.Intent.AbilityId == CombatScenarioFactory.ForwardLineAbilityId, "SpamAbilityBehavior should repeatedly select its configured ability.");
            completedChecks.Add(nameof(VerifySpamAbilityIntent));
        }

        private static void VerifyScriptedBehaviorSequence(List<string> completedChecks)
        {
            var behavior = new ScriptedCombatBehavior(
                "scripted-sequence",
                new[]
                {
                    CombatBehaviorIntent.Move(CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2)),
                    CombatBehaviorIntent.EndTurn(CombatScenarioFactory.PlayerUnitId),
                });
            var context = CombatBehaviorContext.FromState(CombatScenarioFactory.CreateInitialState(), CombatScenarioFactory.PlayerUnitId);
            var firstDecision = behavior.DecideIntent(context);
            var secondDecision = behavior.DecideIntent(context);
            var thirdDecision = behavior.DecideIntent(context);
            Ensure(firstDecision.Intent.IntentType == CombatBehaviorIntentType.Move, "Scripted behavior should return its first configured step.");
            Ensure(secondDecision.Intent.IntentType == CombatBehaviorIntentType.EndTurn, "Scripted behavior should preserve configured order.");
            Ensure(thirdDecision.Intent.IntentType == CombatBehaviorIntentType.EndTurn, "Scripted behavior should fall back deterministically after exhausting steps.");
            completedChecks.Add(nameof(VerifyScriptedBehaviorSequence));
        }

        private static void VerifyEnemyBehaviorEquivalence(List<string> completedChecks)
        {
            var state = new CombatState(
                CombatScenarioFactory.CreateDefaultBoardDimensions(),
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
            var engine = new CombatEngine();
            var result = engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);
            Ensure(result.Succeeded, "Behavior-driven enemy action should succeed.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy), "Enemy should remain in state.");
            Ensure(enemy.Position.X == 3 && enemy.Position.Y == 2, "Behavior-driven enemy movement should match the previous vertical-first chase behavior.");
            Ensure(result.Events.Any(evt => evt is BehaviorIntentSelected), "Behavior-driven enemy action should emit a behavior-decision event.");
            completedChecks.Add(nameof(VerifyEnemyBehaviorEquivalence));
        }

        private static void VerifyBehaviorDrivenScenario(List<string> completedChecks)
        {
            var runner = new ScenarioRunner();
            var result = runner.Run(ScenarioCatalog.CreateBehaviorTurnScenario());
            Ensure(result.ExecutionSucceeded, "Behavior-driven scenario should execute successfully.");
            Ensure(result.Verification.Succeeded, "Behavior-driven scenario should satisfy its verification contract.");
            Ensure(result.Events.Any(evt => evt is BehaviorIntentSelected), "Behavior-driven scenarios should log behavior decisions.");
            completedChecks.Add(nameof(VerifyBehaviorDrivenScenario));
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
