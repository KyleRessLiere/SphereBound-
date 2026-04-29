namespace Spherebound.CoreCombatLoop.Core
{
    public static class CombatScenarioFactory
    {
        public const int PlayerUnitId = 1;
        public const int EnemyUnitId = 2;
        public const int BoardWidth = 6;
        public const int BoardHeight = 6;
        public const int PlayerStartingHealth = 5;
        public const int EnemyStartingHealth = 3;
        public const int PlayerActionsPerTurn = 2;

        public static CombatState CreateInitialState()
        {
            return new CombatState(
                new BoardDimensions(BoardWidth, BoardHeight),
                CombatTurnSide.Player,
                PlayerActionsPerTurn,
                new[]
                {
                    new CombatUnitState(
                        PlayerUnitId,
                        CombatUnitSide.Player,
                        PlayerStartingHealth,
                        new GridPosition(1, 1),
                        UnitLifeState.Alive),
                    new CombatUnitState(
                        EnemyUnitId,
                        CombatUnitSide.Enemy,
                        EnemyStartingHealth,
                        new GridPosition(4, 4),
                        UnitLifeState.Alive),
                });
        }
    }
}
