using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatBehaviorUnitView
    {
        public CombatBehaviorUnitView(
            int unitId,
            CombatUnitSide side,
            GridPosition position,
            int currentHealth,
            bool isAlive,
            CombatUnitDefinition definition)
        {
            UnitId = unitId;
            Side = side;
            Position = position;
            CurrentHealth = currentHealth;
            IsAlive = isAlive;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public int UnitId { get; }

        public CombatUnitSide Side { get; }

        public GridPosition Position { get; }

        public int CurrentHealth { get; }

        public bool IsAlive { get; }

        public CombatUnitDefinition Definition { get; }
    }
}
