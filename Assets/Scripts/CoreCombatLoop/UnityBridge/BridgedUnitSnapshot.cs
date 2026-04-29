using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class BridgedUnitSnapshot
    {
        public BridgedUnitSnapshot(
            int unitId,
            CombatUnitSide side,
            GridPosition position,
            int currentHealth,
            UnitLifeState lifeState)
        {
            UnitId = unitId;
            Side = side;
            Position = position;
            CurrentHealth = currentHealth;
            LifeState = lifeState;
        }

        public int UnitId { get; }

        public CombatUnitSide Side { get; }

        public GridPosition Position { get; }

        public int CurrentHealth { get; }

        public UnitLifeState LifeState { get; }
    }
}
