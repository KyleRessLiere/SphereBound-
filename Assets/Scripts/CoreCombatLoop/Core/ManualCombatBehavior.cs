using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class ManualCombatBehavior : ICombatBehaviorDefinition
    {
        private CombatBehaviorIntent? pendingIntent;

        public ManualCombatBehavior(string behaviorId = "manual-input")
        {
            if (string.IsNullOrWhiteSpace(behaviorId))
            {
                throw new ArgumentException("Behavior id is required.", nameof(behaviorId));
            }

            BehaviorId = behaviorId;
        }

        public string BehaviorId { get; }

        public void SetPendingIntent(CombatBehaviorIntent intent)
        {
            pendingIntent = intent ?? throw new ArgumentNullException(nameof(intent));
        }

        public void ClearPendingIntent()
        {
            pendingIntent = null;
        }

        public CombatBehaviorDecision DecideIntent(CombatBehaviorContext context)
        {
            if (pendingIntent != null)
            {
                var selectedIntent = pendingIntent;
                pendingIntent = null;
                return new CombatBehaviorDecision(BehaviorId, selectedIntent);
            }

            return new CombatBehaviorDecision(BehaviorId, CombatBehaviorIntent.EndTurn(context.ActingUnitId));
        }
    }
}
