using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class EnemyIntentVerifier
    {
        private const int SecondaryEnemyUnitId = 3;

        public static IReadOnlyList<string> RunAll()
        {
            var completedChecks = new List<string>();

            VerifyEnemyIntentSummariesProduced(completedChecks);
            VerifyLivingEnemiesAppearInIntentList(completedChecks);
            VerifySniperShowsChargeCountdown(completedChecks);
            VerifySniperCountdownDecreasesDeterministically(completedChecks);
            VerifySniperFiresAfterChargeCompletesWhenAligned(completedChecks);
            VerifySniperDoesNotFireIfAlignmentIsBroken(completedChecks);

            return completedChecks;
        }

        private static void VerifyEnemyIntentSummariesProduced(List<string> completedChecks)
        {
            var intents = CombatEnemyIntentPanelBuilder.Build(CombatScenarioFactory.CreateInitialState());
            Ensure(intents.Count == 1, "Default scenario should expose one living enemy intent.");
            Ensure(intents[0].EnemyDisplayName == "Enemy Grunt", "Default enemy intent should expose the enemy display name.");
            Ensure(!string.IsNullOrWhiteSpace(intents[0].SummaryText), "Enemy intent should include summary text.");
            completedChecks.Add(nameof(VerifyEnemyIntentSummariesProduced));
        }

        private static void VerifyLivingEnemiesAppearInIntentList(List<string> completedChecks)
        {
            var state = new CombatState(
                CombatScenarioFactory.CreateDefaultBoardDimensions(),
                CombatTurnSide.Player,
                CombatScenarioFactory.PlayerActionsPerTurn,
                new[]
                {
                    new CombatUnitState(
                        CombatScenarioFactory.PlayerUnitId,
                        CombatUnitSide.Player,
                        CombatScenarioFactory.PlayerStartingHealth,
                        CombatScenarioFactory.PlayerStartingPosition,
                        UnitLifeState.Alive,
                        CombatScenarioFactory.PlayerDefinition,
                        CombatBehaviorAssignment.Manual(new ManualCombatBehavior())),
                    new CombatUnitState(
                        CombatScenarioFactory.EnemyUnitId,
                        CombatUnitSide.Enemy,
                        CombatScenarioFactory.EnemyStartingHealth,
                        CombatScenarioFactory.EnemyStartingPosition,
                        UnitLifeState.Alive,
                        CombatScenarioFactory.EnemyDefinition,
                        CombatBehaviorAssignment.Default(new MoveTowardTargetBehavior("enemy-grunt-intent", CombatScenarioFactory.PlayerUnitId))),
                    new CombatUnitState(
                        SecondaryEnemyUnitId,
                        CombatUnitSide.Enemy,
                        CombatScenarioFactory.EnemyStartingHealth,
                        new GridPosition(0, 4),
                        UnitLifeState.Alive,
                        CombatScenarioFactory.SniperDefinition,
                        CombatBehaviorAssignment.Default(CreateSniperBehavior())),
                });

            var intents = CombatEnemyIntentPanelBuilder.Build(state);
            Ensure(intents.Count == 2, "All living enemies should appear in the intent list.");
            Ensure(intents.Any(intent => intent.EnemyUnitId == CombatScenarioFactory.EnemyUnitId), "Grunt intent should appear.");
            Ensure(intents.Any(intent => intent.EnemyUnitId == SecondaryEnemyUnitId), "Sniper intent should appear.");
            completedChecks.Add(nameof(VerifyLivingEnemiesAppearInIntentList));
        }

        private static void VerifySniperShowsChargeCountdown(List<string> completedChecks)
        {
            var state = CreateSniperState();
            var intent = CombatEnemyIntentPanelBuilder.Build(state).Single();
            Ensure(intent.IntentType == EnemyIntentType.Charge, "Aligned sniper should expose a charge intent.");
            Ensure(intent.CountdownValue == 2, "Sniper should start with a 2-turn charge countdown.");
            Ensure(intent.SummaryText.Contains("2 turns remaining"), "Sniper summary should include the charge countdown.");
            completedChecks.Add(nameof(VerifySniperShowsChargeCountdown));
        }

        private static void VerifySniperCountdownDecreasesDeterministically(List<string> completedChecks)
        {
            var state = CreateSniperState();
            var engine = new CombatEngine();

            engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);
            var firstIntent = CombatEnemyIntentPanelBuilder.Build(state).Single();
            Ensure(firstIntent.CountdownValue == 2, "After beginning charge, sniper should still report 2 turns remaining.");

            engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);
            var secondIntent = CombatEnemyIntentPanelBuilder.Build(state).Single();
            Ensure(secondIntent.CountdownValue == 1, "Charge countdown should decrease deterministically on the next enemy turn.");
            completedChecks.Add(nameof(VerifySniperCountdownDecreasesDeterministically));
        }

        private static void VerifySniperFiresAfterChargeCompletesWhenAligned(List<string> completedChecks)
        {
            var state = CreateSniperState();
            var engine = new CombatEngine();

            engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);
            engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);
            var fireResult = engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);

            Ensure(fireResult.Succeeded, "Sniper firing turn should succeed when alignment is preserved.");
            Ensure(fireResult.Events.Any(evt => evt is AttackRequested), "Sniper fire should use the existing attack event flow.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player), "Player should remain in state after taking sniper damage.");
            Ensure(player.CurrentHealth == CombatScenarioFactory.PlayerStartingHealth - 1, "Sniper fire should damage the player.");
            completedChecks.Add(nameof(VerifySniperFiresAfterChargeCompletesWhenAligned));
        }

        private static void VerifySniperDoesNotFireIfAlignmentIsBroken(List<string> completedChecks)
        {
            var state = CreateSniperState();
            var engine = new CombatEngine();

            engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);
            engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);
            Ensure(state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player), "Player should exist before alignment is broken.");
            player.Position = new GridPosition(1, 0);

            var result = engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);

            Ensure(result.Events.All(evt => evt is not AttackRequested), "Sniper should not fire when alignment is broken.");
            Ensure(result.Events.Any(evt => evt is MoveRequested || evt is BehaviorIntentSelected), "Sniper should return to ordinary decision-making after losing alignment.");
            Ensure(player.CurrentHealth == CombatScenarioFactory.PlayerStartingHealth, "Broken-alignment sniper turn should not damage the player.");
            completedChecks.Add(nameof(VerifySniperDoesNotFireIfAlignmentIsBroken));
        }

        private static CombatState CreateSniperState()
        {
            return new CombatState(
                CombatScenarioFactory.CreateDefaultBoardDimensions(),
                CombatTurnSide.Enemy,
                0,
                new[]
                {
                    new CombatUnitState(
                        CombatScenarioFactory.PlayerUnitId,
                        CombatUnitSide.Player,
                        CombatScenarioFactory.PlayerStartingHealth,
                        new GridPosition(2, 0),
                        UnitLifeState.Alive,
                        CombatScenarioFactory.PlayerDefinition,
                        CombatBehaviorAssignment.Manual(new ManualCombatBehavior())),
                    new CombatUnitState(
                        CombatScenarioFactory.EnemyUnitId,
                        CombatUnitSide.Enemy,
                        CombatScenarioFactory.EnemyStartingHealth,
                        new GridPosition(2, 4),
                        UnitLifeState.Alive,
                        CombatScenarioFactory.SniperDefinition,
                        CombatBehaviorAssignment.Default(CreateSniperBehavior())),
                });
        }

        private static SniperChargeBehavior CreateSniperBehavior()
        {
            return new SniperChargeBehavior(
                CombatScenarioFactory.EnemySniperBehaviorId,
                CombatScenarioFactory.PlayerUnitId,
                CombatScenarioFactory.SniperLineShotAbilityId,
                "Line Shot");
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
