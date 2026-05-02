using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatBehaviorDecision
    {
        public CombatBehaviorDecision(string behaviorId, CombatBehaviorIntent intent)
        {
            if (string.IsNullOrWhiteSpace(behaviorId))
            {
                throw new ArgumentException("Behavior id is required.", nameof(behaviorId));
            }

            BehaviorId = behaviorId;
            Intent = intent ?? throw new ArgumentNullException(nameof(intent));
        }

        public string BehaviorId { get; }

        public CombatBehaviorIntent Intent { get; }
    }
}
