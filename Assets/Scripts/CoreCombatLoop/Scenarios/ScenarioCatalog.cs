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
                CreateBehaviorTurnScenario(),
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
                    new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 5, expectedPosition: CombatScenarioFactory.PlayerStartingPosition, expectedLifeState: UnitLifeState.Alive),
                    new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedHealth: 3, expectedPosition: new GridPosition(2, 2), expectedLifeState: UnitLifeState.Alive),
                },
                true,
                new[]
                {
                    nameof(TurnStarted),
                    nameof(TurnEnded),
                    nameof(TurnStarted),
                    nameof(BehaviorIntentSelected),
                    nameof(MoveRequested),
                    nameof(UnitMoved),
                    nameof(TurnEnded),
                    nameof(TurnStarted),
                });
        }

        public static ScenarioDefinition CreateBehaviorTurnScenario()
        {
            return new ScenarioDefinition(
                "behavior-turn-cycle",
                "Behavior Turn Cycle",
                () => CreateBehaviorDrivenState(),
                new[]
                {
                    ScenarioStep.StartCombat(),
                    ScenarioStep.RunBehaviorTurnCycle(),
                },
                new[]
                {
                    new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 5, expectedPosition: new GridPosition(1, 1), expectedLifeState: UnitLifeState.Alive),
                    new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedHealth: 2, expectedPosition: new GridPosition(1, 2), expectedLifeState: UnitLifeState.Alive),
                },
                true,
                new[]
                {
                    nameof(TurnStarted),
                    nameof(BehaviorIntentSelected),
                    nameof(AbilityRequested),
                    nameof(ActionStarted),
                    nameof(AttackRequested),
                    nameof(DamageRequested),
                    nameof(UnitDamaged),
                    nameof(BehaviorIntentSelected),
                    nameof(TurnEnded),
                    nameof(TurnStarted),
                    nameof(BehaviorIntentSelected),
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

        private static CombatState CreateBehaviorDrivenState()
        {
            return new CombatState(
                CombatScenarioFactory.CreateDefaultBoardDimensions(),
                CombatTurnSide.Player,
                2,
                new[]
                {
                    new CombatUnitState(
                        CombatScenarioFactory.PlayerUnitId,
                        CombatUnitSide.Player,
                        5,
                        new GridPosition(1, 1),
                        UnitLifeState.Alive,
                        CombatScenarioFactory.PlayerDefinition,
                        CombatBehaviorAssignment.Scenario(new ScriptedCombatBehavior(
                            "player-scripted-combo",
                            new[]
                            {
                                CombatBehaviorIntent.UseAbility(
                                    CombatScenarioFactory.PlayerUnitId,
                                    CombatScenarioFactory.BasicAttackAbilityId,
                                    CombatScenarioFactory.EnemyUnitId),
                                CombatBehaviorIntent.EndTurn(CombatScenarioFactory.PlayerUnitId),
                            }))),
                    new CombatUnitState(
                        CombatScenarioFactory.EnemyUnitId,
                        CombatUnitSide.Enemy,
                        3,
                        new GridPosition(1, 2),
                        UnitLifeState.Alive,
                        CombatScenarioFactory.EnemyDefinition,
                        CombatBehaviorAssignment.Scenario(new PassTurnBehavior("enemy-pass"))),
                });
        }
    }
}
