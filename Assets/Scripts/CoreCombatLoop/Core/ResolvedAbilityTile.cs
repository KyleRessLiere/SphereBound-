namespace Spherebound.CoreCombatLoop.Core
{
    public readonly struct ResolvedAbilityTile
    {
        public ResolvedAbilityTile(GridPosition position, int? occupantUnitId)
        {
            Position = position;
            OccupantUnitId = occupantUnitId;
        }

        public GridPosition Position { get; }

        public int? OccupantUnitId { get; }
    }
}
