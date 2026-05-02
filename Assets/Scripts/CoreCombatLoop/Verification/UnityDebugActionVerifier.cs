using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;
using Spherebound.CoreCombatLoop.UnityBridge;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class UnityDebugActionVerifier
    {
        public static IReadOnlyList<string> RunAll()
        {
            var completedChecks = new List<string>();

            VerifyMoveCommand(completedChecks);
            VerifyAttackCommand(completedChecks);
            VerifyEndTurnCommand(completedChecks);
            VerifyRestartCreatesFreshSession(completedChecks);
            VerifyMissingSessionFailure(completedChecks);

            return completedChecks;
        }

        private static void VerifyMoveCommand(List<string> completedChecks)
        {
            var session = ObservableCombatSession.CreateDefault("debug-move");
            session.StartCombat();

            var result = CombatDebugCommandExecutor.Execute(
                session,
                CombatDebugCommandRequest.Move(
                    CombatScenarioFactory.PlayerUnitId,
                    new GridPosition(1, 2)));

            Ensure(result.Succeeded, "Debug move should succeed for a valid adjacent destination.");
            Ensure(result.CommandFailureReason == CombatDebugCommandFailureReason.None, "Debug move should not fail at the bridge-command layer.");
            Ensure(result.GameplayFailureReason == CombatFailureReason.None, "Debug move should not fail at the gameplay layer.");

            var snapshot = session.CaptureSnapshot();
            var player = snapshot.Units.Single(unit => unit.UnitId == CombatScenarioFactory.PlayerUnitId);
            Ensure(player.Position.X == 1 && player.Position.Y == 2, "Debug move should update authoritative player position.");
            Ensure(result.Events.Any(evt => evt is MoveRequested), "Debug move should emit MoveRequested.");
            completedChecks.Add(nameof(VerifyMoveCommand));
        }

        private static void VerifyAttackCommand(List<string> completedChecks)
        {
            var session = new ObservableCombatSession(
                "debug-attack",
                new CombatState(
                    new BoardDimensions(6, 6),
                    CombatTurnSide.Player,
                    2,
                    new[]
                    {
                        new CombatUnitState(
                            CombatScenarioFactory.PlayerUnitId,
                            CombatUnitSide.Player,
                            5,
                            new GridPosition(1, 1),
                            UnitLifeState.Alive),
                        new CombatUnitState(
                            CombatScenarioFactory.EnemyUnitId,
                            CombatUnitSide.Enemy,
                            3,
                            new GridPosition(1, 2),
                            UnitLifeState.Alive),
                    }));

            session.StartCombat();

            var result = CombatDebugCommandExecutor.Execute(
                session,
                CombatDebugCommandRequest.Attack(
                    CombatScenarioFactory.PlayerUnitId,
                    CombatScenarioFactory.EnemyUnitId));

            Ensure(result.Succeeded, "Debug attack should succeed for a valid adjacent target.");
            Ensure(result.Events.Any(evt => evt is AttackRequested), "Debug attack should emit AttackRequested.");
            Ensure(result.Events.Any(evt => evt is DamageRequested), "Debug attack should emit DamageRequested.");

            var snapshot = session.CaptureSnapshot();
            var enemy = snapshot.Units.Single(unit => unit.UnitId == CombatScenarioFactory.EnemyUnitId);
            Ensure(enemy.CurrentHealth == 2, "Debug attack should damage the enemy through the core.");
            completedChecks.Add(nameof(VerifyAttackCommand));
        }

        private static void VerifyEndTurnCommand(List<string> completedChecks)
        {
            var session = ObservableCombatSession.CreateDefault("debug-end-turn");
            session.StartCombat();

            var result = CombatDebugCommandExecutor.Execute(
                session,
                CombatDebugCommandRequest.EndTurn(
                    CombatScenarioFactory.PlayerUnitId));

            Ensure(result.Succeeded, "Debug end turn should succeed.");
            var eventNames = result.Events.Select(evt => evt.GetType().Name).ToList();
            Ensure(eventNames.First() == nameof(TurnEnded), "Debug end turn should start with TurnEnded.");
            Ensure(eventNames.Contains(nameof(TurnStarted)), "Debug end turn should include a subsequent TurnStarted.");

            var snapshot = session.CaptureSnapshot();
            var player = snapshot.Units.Single(unit => unit.UnitId == CombatScenarioFactory.PlayerUnitId);
            Ensure(player.Position.X == 1 && player.Position.Y == 1, "Enemy turn should not corrupt player position in the default scenario.");
            completedChecks.Add(nameof(VerifyEndTurnCommand));
        }

        private static void VerifyRestartCreatesFreshSession(List<string> completedChecks)
        {
            var firstSession = ObservableCombatSession.CreateDefault("restart-before");
            firstSession.StartCombat();
            CombatDebugCommandExecutor.Execute(
                firstSession,
                CombatDebugCommandRequest.Move(
                    CombatScenarioFactory.PlayerUnitId,
                    new GridPosition(1, 2)));

            var restartedSession = ObservableCombatSession.CreateDefault("restart-after");
            restartedSession.StartCombat();
            var snapshot = restartedSession.CaptureSnapshot();
            var player = snapshot.Units.Single(unit => unit.UnitId == CombatScenarioFactory.PlayerUnitId);

            Ensure(player.Position.X == 1 && player.Position.Y == 1, "Restart should create a fresh session with initial player position.");
            Ensure(snapshot.SessionId == "restart-after", "Restart should expose the new session identity.");
            completedChecks.Add(nameof(VerifyRestartCreatesFreshSession));
        }

        private static void VerifyMissingSessionFailure(List<string> completedChecks)
        {
            var result = CombatDebugCommandExecutor.Execute(
                null,
                CombatDebugCommandRequest.Move(
                    CombatScenarioFactory.PlayerUnitId,
                    new GridPosition(1, 2)));

            Ensure(!result.Succeeded, "Missing session should fail.");
            Ensure(result.CommandFailureReason == CombatDebugCommandFailureReason.SessionUnavailable, "Missing session should fail at the bridge-command layer.");
            Ensure(result.Events.Count == 0, "Missing session should not emit gameplay events.");
            completedChecks.Add(nameof(VerifyMissingSessionFailure));
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
