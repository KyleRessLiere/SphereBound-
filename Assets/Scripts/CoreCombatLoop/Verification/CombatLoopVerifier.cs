using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class CombatLoopVerifier
    {
        public static IReadOnlyList<string> RunAll()
        {
            var completedChecks = new List<string>();

            VerifyInitialScenario(completedChecks);
            VerifyTurnProgression(completedChecks);
            VerifyPlayerMovement(completedChecks);
            VerifyInvalidMovement(completedChecks);
            VerifyAttackAndDamage(completedChecks);
            VerifyEnemyMovement(completedChecks);
            VerifyEnemyAttack(completedChecks);
            VerifyDeathAndRemoval(completedChecks);
            VerifyOutOfTurnFailure(completedChecks);

            return completedChecks;
        }

        private static void VerifyInitialScenario(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();

            Ensure(state.Board.Width == 4 && state.Board.Height == 4, "Initial board should be 4x4.");
            Ensure(state.ActiveTurn == CombatTurnSide.Player, "Player should act first.");
            Ensure(state.RemainingPlayerActions == 2, "Player should start with 2 actions.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player), "Player should exist.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy), "Enemy should exist.");
            Ensure(player.CurrentHealth == 5, "Player should start with 5 HP.");
            Ensure(enemy.CurrentHealth == 3, "Enemy should start with 3 HP.");
            completedChecks.Add(nameof(VerifyInitialScenario));
        }

        private static void VerifyTurnProgression(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();
            var engine = new CombatEngine();

            var startEvents = engine.StartCombat(state);
            Ensure(startEvents.Count == 1 && startEvents[0] is TurnStarted, "Combat should start with TurnStarted.");

            var events = engine.EndPlayerTurnAndRunEnemyTurn(state).ToList();
            Ensure(events[0] is TurnEnded playerEnded && playerEnded.Side == CombatTurnSide.Player, "Player turn should end first.");
            Ensure(events[1] is TurnStarted enemyStarted && enemyStarted.Side == CombatTurnSide.Enemy, "Enemy turn should start after player turn ends.");
            Ensure(events[events.Count - 1] is TurnStarted playerStarted && playerStarted.Side == CombatTurnSide.Player, "Next player turn should start after enemy turn ends.");
            Ensure(state.ActiveTurn == CombatTurnSide.Player, "Turn progression should return to player.");
            Ensure(state.RemainingPlayerActions == 2, "Player actions should reset for the new player turn.");
            completedChecks.Add(nameof(VerifyTurnProgression));
        }

        private static void VerifyPlayerMovement(List<string> completedChecks)
        {
            var state = CreateState(new GridPosition(1, 1), 5, CombatScenarioFactory.EnemyStartingPosition, 3, CombatTurnSide.Player, 2);
            var engine = new CombatEngine();

            var result = engine.ResolveMove(state, CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2));
            Ensure(result.Succeeded, "Valid player move should succeed.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player), "Player should remain in state.");
            Ensure(player.Position.X == 1 && player.Position.Y == 2, "Player position should update after a successful move.");
            Ensure(state.RemainingPlayerActions == 1, "Successful player move should spend one action.");
            Ensure(ContainsEvent<UnitMoved>(result.Events), "Successful move should emit UnitMoved.");
            Ensure(ContainsEvent<ActionSpent>(result.Events), "Successful move should emit ActionSpent.");
            completedChecks.Add(nameof(VerifyPlayerMovement));
        }

        private static void VerifyInvalidMovement(List<string> completedChecks)
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(1, 2), 3, CombatTurnSide.Player, 2);
            var engine = new CombatEngine();

            var result = engine.ResolveMove(state, CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2));
            Ensure(!result.Succeeded, "Move into an occupied tile should fail.");
            Ensure(result.FailureReason == CombatFailureReason.DestinationOccupied, "Occupied movement should fail with DestinationOccupied.");
            Ensure(state.RemainingPlayerActions == 2, "Failed movement should not spend an action.");
            Ensure(ContainsEvent<ActionFailed>(result.Events), "Failed movement should emit ActionFailed.");
            Ensure(ContainsEvent<MoveBlocked>(result.Events), "Failed movement should emit MoveBlocked.");
            completedChecks.Add(nameof(VerifyInvalidMovement));
        }

        private static void VerifyAttackAndDamage(List<string> completedChecks)
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(1, 2), 3, CombatTurnSide.Player, 2);
            var engine = new CombatEngine();

            var result = engine.ResolveAttack(state, CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId);
            Ensure(result.Succeeded, "Adjacent player attack should succeed.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy), "Enemy should remain alive after one damage.");
            Ensure(enemy.CurrentHealth == 2, "Successful attack should deal 1 damage.");
            Ensure(state.RemainingPlayerActions == 1, "Successful player attack should spend one action.");
            Ensure(ContainsEvent<AttackRequested>(result.Events), "Attack should emit AttackRequested.");
            Ensure(ContainsEvent<DamageRequested>(result.Events), "Attack should emit DamageRequested.");
            Ensure(ContainsEvent<UnitDamaged>(result.Events), "Attack should emit UnitDamaged.");
            completedChecks.Add(nameof(VerifyAttackAndDamage));
        }

        private static void VerifyEnemyMovement(List<string> completedChecks)
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(3, 3), 3, CombatTurnSide.Enemy, 0);
            var engine = new CombatEngine();

            var result = engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);
            Ensure(result.Succeeded, "Enemy movement should succeed when not adjacent.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy), "Enemy should remain in state after moving.");
            Ensure(enemy.Position.X == 3 && enemy.Position.Y == 2, "Enemy should prefer vertical movement first.");
            completedChecks.Add(nameof(VerifyEnemyMovement));
        }

        private static void VerifyEnemyAttack(List<string> completedChecks)
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(1, 2), 3, CombatTurnSide.Enemy, 0);
            var engine = new CombatEngine();

            var result = engine.RunEnemyBehavior(state, CombatScenarioFactory.EnemyUnitId);
            Ensure(result.Succeeded, "Enemy attack should succeed when adjacent.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player), "Player should remain alive after one enemy attack.");
            Ensure(player.CurrentHealth == 4, "Enemy attack should deal 1 damage.");
            Ensure(!ContainsEvent<ActionSpent>(result.Events), "Enemy behavior should not spend player actions.");
            completedChecks.Add(nameof(VerifyEnemyAttack));
        }

        private static void VerifyDeathAndRemoval(List<string> completedChecks)
        {
            var state = CreateState(new GridPosition(1, 1), 5, new GridPosition(1, 2), 1, CombatTurnSide.Player, 2);
            var engine = new CombatEngine();

            var result = engine.ResolveAttack(state, CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId);
            Ensure(result.Succeeded, "Lethal attack should succeed.");
            Ensure(!state.ContainsUnit(CombatScenarioFactory.EnemyUnitId), "Dead enemy should be removed from the board state.");

            var eventNames = result.Events.Select(evt => evt.GetType().Name).ToList();
            Ensure(eventNames.IndexOf(nameof(DamageRequested)) < eventNames.IndexOf(nameof(UnitDamaged)), "DamageRequested should occur before UnitDamaged.");
            Ensure(eventNames.IndexOf(nameof(UnitDamaged)) < eventNames.IndexOf(nameof(UnitDying)), "UnitDamaged should occur before UnitDying.");
            Ensure(eventNames.IndexOf(nameof(UnitDying)) < eventNames.IndexOf(nameof(UnitDeath)), "UnitDying should occur before UnitDeath.");
            Ensure(eventNames.IndexOf(nameof(UnitDeath)) < eventNames.IndexOf(nameof(UnitRemoved)), "UnitDeath should occur before UnitRemoved.");
            completedChecks.Add(nameof(VerifyDeathAndRemoval));
        }

        private static void VerifyOutOfTurnFailure(List<string> completedChecks)
        {
            var state = CreateState(new GridPosition(1, 1), 5, CombatScenarioFactory.EnemyStartingPosition, 3, CombatTurnSide.Enemy, 2);
            var engine = new CombatEngine();

            var result = engine.ResolveMove(state, CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2));
            Ensure(!result.Succeeded, "Out-of-turn player action should fail.");
            Ensure(result.FailureReason == CombatFailureReason.OutOfTurn, "Out-of-turn action should fail with OutOfTurn.");
            Ensure(state.RemainingPlayerActions == 2, "Out-of-turn failure should not spend player actions.");
            completedChecks.Add(nameof(VerifyOutOfTurnFailure));
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
                CombatScenarioFactory.CreateDefaultBoardDimensions(),
                activeTurn,
                remainingPlayerActions,
                new[]
                {
                    new CombatUnitState(
                        CombatScenarioFactory.PlayerUnitId,
                        CombatUnitSide.Player,
                        playerHealth,
                        playerPosition,
                        UnitLifeState.Alive),
                    new CombatUnitState(
                        CombatScenarioFactory.EnemyUnitId,
                        CombatUnitSide.Enemy,
                        enemyHealth,
                        enemyPosition,
                        UnitLifeState.Alive),
                });
        }

        private static bool ContainsEvent<TEvent>(IEnumerable<ICombatEvent> events)
            where TEvent : class, ICombatEvent
        {
            return events.Any(evt => evt is TEvent);
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
