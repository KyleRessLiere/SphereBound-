using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;
using Spherebound.CoreCombatLoop.UnityBridge;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class CombatDebugSurfaceVerifier
    {
        public static IReadOnlyList<string> RunAll()
        {
            var completedChecks = new List<string>();

            VerifyBoardFormatting(completedChecks);
            VerifyMovementBoardOutput(completedChecks);
            VerifyAttackOverlayOutput(completedChecks);
            VerifyFailedAttackDoesNotEmitOverlay(completedChecks);
            VerifyActionCountUpdates(completedChecks);

            return completedChecks;
        }

        private static void VerifyBoardFormatting(List<string> completedChecks)
        {
            var snapshot = ObservableCombatSession.CreateDefault("board-format").CaptureSnapshot();
            var formattedBoard = CombatBoardFormatter.FormatBoard(snapshot);

            Ensure(formattedBoard.Contains("E"), "Formatted board should contain enemy marker.");
            Ensure(formattedBoard.Contains("P"), "Formatted board should contain player marker.");
            Ensure(formattedBoard.Split(Environment.NewLine).Length == snapshot.Board.Height, "Formatted board should include one row per board row.");
            completedChecks.Add(nameof(VerifyBoardFormatting));
        }

        private static void VerifyMovementBoardOutput(List<string> completedChecks)
        {
            var session = ObservableCombatSession.CreateDefault("move-board");
            var presenter = new CombatDebugSurfacePresenter();

            presenter.HandleEvent(new TurnStarted(CombatTurnSide.Player), null, session.CaptureSnapshot(), session.State.RemainingPlayerActions);

            var moveResult = session.ResolveMove(CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2));
            var logs = ReplayLogs(presenter, session, moveResult.Events, session.CaptureSnapshot());

            var boardLog = logs.Single(log => log.Category == "Board");
            Ensure(boardLog.Message.Contains(". P . . . ."), "Movement board output should show the moved player position.");
            completedChecks.Add(nameof(VerifyMovementBoardOutput));
        }

        private static void VerifyAttackOverlayOutput(List<string> completedChecks)
        {
            var session = new ObservableCombatSession(
                "attack-overlay",
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

            var presenter = new CombatDebugSurfacePresenter();
            var snapshotBefore = session.CaptureSnapshot();
            var attackResult = session.ResolveAttack(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId);
            var snapshotAfter = session.CaptureSnapshot();

            var logs = ReplayLogs(presenter, session, attackResult.Events, snapshotAfter, snapshotBefore);
            var attackBoard = logs.Single(log => log.Category == "AttackBoard");

            Ensure(attackBoard.Message.Contains("O"), "Attack overlay should mark the hit tile with O.");
            Ensure(!attackBoard.Message.Contains("E"), "Attack overlay should replace the underlying board symbol on the hit tile.");
            completedChecks.Add(nameof(VerifyAttackOverlayOutput));
        }

        private static void VerifyFailedAttackDoesNotEmitOverlay(List<string> completedChecks)
        {
            var session = ObservableCombatSession.CreateDefault("failed-attack");
            var presenter = new CombatDebugSurfacePresenter();
            var snapshotBefore = session.CaptureSnapshot();
            var attackResult = session.ResolveAttack(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId);
            var snapshotAfter = session.CaptureSnapshot();

            var logs = ReplayLogs(presenter, session, attackResult.Events, snapshotAfter, snapshotBefore);
            Ensure(logs.All(log => log.Category != "AttackBoard"), "Invalid attack should not emit a successful attack overlay.");
            completedChecks.Add(nameof(VerifyFailedAttackDoesNotEmitOverlay));
        }

        private static void VerifyActionCountUpdates(List<string> completedChecks)
        {
            var session = ObservableCombatSession.CreateDefault("action-count");
            var presenter = new CombatDebugSurfacePresenter();

            var initialLogs = presenter.HandleEvent(
                new TurnStarted(CombatTurnSide.Player),
                null,
                session.CaptureSnapshot(),
                session.State.RemainingPlayerActions);
            Ensure(initialLogs.Any(log => log.Category == "Actions" && log.Message == "remainingActions=2"), "Initial action count should be logged.");

            var moveResult = session.ResolveMove(CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2));
            var moveLogs = ReplayLogs(presenter, session, moveResult.Events, session.CaptureSnapshot());
            Ensure(moveLogs.Any(log => log.Category == "Actions" && log.Message == "remainingActions=1"), "Action count should update after spending an action.");

            var endTurnEvents = session.EndPlayerTurnAndRunEnemyTurn();
            var endTurnLogs = ReplayLogs(presenter, session, endTurnEvents, session.CaptureSnapshot());
            Ensure(endTurnLogs.Any(log => log.Category == "Actions" && log.Message == "remainingActions=2"), "Action count should reset for the next player turn.");
            completedChecks.Add(nameof(VerifyActionCountUpdates));
        }

        private static IReadOnlyList<CombatDebugSurfaceLog> ReplayLogs(
            CombatDebugSurfacePresenter presenter,
            ObservableCombatSession session,
            IReadOnlyList<ICombatEvent> events,
            BridgedCombatSessionSnapshot snapshotAfter,
            BridgedCombatSessionSnapshot? snapshotBefore = null)
        {
            var logs = new List<CombatDebugSurfaceLog>();
            var before = snapshotBefore;
            foreach (var combatEvent in events)
            {
                logs.AddRange(presenter.HandleEvent(
                    combatEvent,
                    before,
                    snapshotAfter,
                    session.State.RemainingPlayerActions));
            }

            return logs;
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
