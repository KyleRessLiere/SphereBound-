namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatUnitState
    {
        public CombatUnitState(int id, CombatUnitSide side, int currentHealth, GridPosition position, UnitLifeState lifeState)
        {
            if (currentHealth < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(currentHealth), "Unit health cannot be negative.");
            }

            Id = id;
            Side = side;
            CurrentHealth = currentHealth;
            Position = position;
            LifeState = lifeState;
        }

        public int Id { get; }

        public CombatUnitSide Side { get; }

        public int CurrentHealth { get; set; }

        public GridPosition Position { get; set; }

        public UnitLifeState LifeState { get; set; }

        public bool IsAlive
        {
            get { return LifeState == UnitLifeState.Alive; }
        }
    }
}
