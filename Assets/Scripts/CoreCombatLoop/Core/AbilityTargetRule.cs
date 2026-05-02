using System;

namespace Spherebound.CoreCombatLoop.Core
{
    [Flags]
    public enum AbilityTargetRule
    {
        None = 0,
        Self = 1 << 0,
        Ally = 1 << 1,
        Enemy = 1 << 2,
        OccupiedTile = 1 << 3,
        EmptyTile = 1 << 4,
    }
}
