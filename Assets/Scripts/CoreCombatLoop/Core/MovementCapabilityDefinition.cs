using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class MovementCapabilityDefinition
    {
        public MovementCapabilityDefinition(int range, int actionCost, bool orthogonalOnly)
        {
            if (range <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(range), "Movement range must be positive.");
            }

            if (actionCost <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(actionCost), "Movement action cost must be positive.");
            }

            Range = range;
            ActionCost = actionCost;
            OrthogonalOnly = orthogonalOnly;
        }

        public int Range { get; }

        public int ActionCost { get; }

        public bool OrthogonalOnly { get; }
    }
}
