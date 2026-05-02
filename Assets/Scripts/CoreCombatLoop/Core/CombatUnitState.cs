namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatUnitState
    {
        public CombatUnitState(int id, CombatUnitSide side, int currentHealth, GridPosition position, UnitLifeState lifeState)
            : this(
                id,
                side,
                currentHealth,
                position,
                lifeState,
                CombatScenarioFactory.GetDefaultDefinition(side),
                null)
        {
        }

        public CombatUnitState(
            int id,
            CombatUnitSide side,
            int currentHealth,
            GridPosition position,
            UnitLifeState lifeState,
            CombatUnitDefinition definition)
            : this(id, side, currentHealth, position, lifeState, definition, null)
        {
        }

        public CombatUnitState(
            int id,
            CombatUnitSide side,
            int currentHealth,
            GridPosition position,
            UnitLifeState lifeState,
            CombatUnitDefinition definition,
            CombatBehaviorAssignment? behaviorAssignment)
        {
            if (currentHealth < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(currentHealth), "Unit health cannot be negative.");
            }

            Definition = definition ?? throw new System.ArgumentNullException(nameof(definition));

            Id = id;
            Side = side;
            CurrentHealth = currentHealth;
            Position = position;
            LifeState = lifeState;
            BehaviorAssignment = behaviorAssignment;
        }

        public int Id { get; }

        public CombatUnitSide Side { get; }

        public CombatUnitDefinition Definition { get; }

        public int CurrentHealth { get; set; }

        public GridPosition Position { get; set; }

        public UnitLifeState LifeState { get; set; }

        public CombatBehaviorAssignment? BehaviorAssignment { get; set; }

        public bool IsAlive
        {
            get { return LifeState == UnitLifeState.Alive; }
        }
    }
}
