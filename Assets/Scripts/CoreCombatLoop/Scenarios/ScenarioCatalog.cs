using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Scenarios
{
    public static class ScenarioCatalog
    {
        public static ScenarioDefinition[] All()
        {
            return new[]
            {
                CreateMoveThenKillScenario(),
                CreateInvalidMoveScenario(),
                CreateEnemyTurnScenario(),
            };
        }

        public static ScenarioDefinition CreateMoveThenKillScenario()
        {
            return new ScenarioDefinition(
                "move-then-kill",
                "Move Then Kill",
                () => CreateState(
                    new GridPosition(1, 1),
                    5,
                    new GridPosition(2, 2),
                    1,
                    CombatTurnSide.Player,
                    2),
                new[]
                {
                    ScenarioStep.StartCombat(),
                    ScenarioStep.Move(CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2), "Player steps into attack range"),
                    ScenarioStep.Attack(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId, "Player kills the enemy"),
                },
                new[]
                {
                    new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 5, expectedPosition: new GridPosition(1, 2), expectedLifeState: UnitLifeState.Alive),
                    new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedLifeState: UnitLifeState.Dead),
                },
                true,
                new[]
                {
                    nameof(TurnStarted),
                    nameof(MoveRequested),
                    nameof(UnitMoved),
                    nameof(AttackRequested),
                    nameof(DamageRequested),
                    nameof(UnitDamaged),
                    nameof(UnitDying),
                    nameof(UnitDeath),
                    nameof(UnitRemoved),
                });
        }

        public static ScenarioDefinition CreateInvalidMoveScenario()
        {
            return new ScenarioDefinition(
                "invalid-move",
                "Invalid Move",
                () => CreateState(
                    new GridPosition(1, 1),
                    5,
                    new GridPosition(1, 2),
                    3,
                    CombatTurnSide.Player,
                    2),
                new[]
                {
                    ScenarioStep.StartCombat(),
                    ScenarioStep.Move(CombatScenarioFactory.PlayerUnitId, new GridPosition(1, 2), "Player attempts to move into an occupied tile"),
                },
                new[]
                {
                    new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 5, expectedPosition: new GridPosition(1, 1), expectedLifeState: UnitLifeState.Alive),
                    new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedHealth: 3, expectedPosition: new GridPosition(1, 2), expectedLifeState: UnitLifeState.Alive),
                },
                false,
                new[]
                {
                    nameof(TurnStarted),
                    nameof(MoveRequested),
                    nameof(ActionFailed),
                    nameof(MoveBlocked),
                });
        }

        public static ScenarioDefinition CreateEnemyTurnScenario()
        {
            return new ScenarioDefinition(
                "enemy-turn-cycle",
                "Enemy Turn Cycle",
                CombatScenarioFactory.CreateInitialState,
                new[]
                {
                    ScenarioStep.StartCombat(),
                    ScenarioStep.EndPlayerTurn(),
                },
                new[]
                {
                    new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 5, expectedPosition: new GridPosition(1, 1), expectedLifeState: UnitLifeState.Alive),
                    new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedHealth: 3, expectedPosition: new GridPosition(4, 3), expectedLifeState: UnitLifeState.Alive),
                },
                true,
                new[]
                {
                    nameof(TurnStarted),
                    nameof(TurnEnded),
                    nameof(TurnStarted),
                    nameof(MoveRequested),
                    nameof(UnitMoved),
                    nameof(TurnEnded),
                    nameof(TurnStarted),
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
    }
}
