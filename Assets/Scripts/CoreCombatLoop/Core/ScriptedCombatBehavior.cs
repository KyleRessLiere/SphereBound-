using System;
using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class ScriptedCombatBehavior : ICombatBehaviorDefinition
    {
        private readonly IReadOnlyList<CombatBehaviorIntent> scriptedIntents;
        private int nextIntentIndex;

        public ScriptedCombatBehavior(string behaviorId, IReadOnlyList<CombatBehaviorIntent> scriptedIntents)
        {
            if (string.IsNullOrWhiteSpace(behaviorId))
            {
                throw new ArgumentException("Behavior id is required.", nameof(behaviorId));
            }

            BehaviorId = behaviorId;
            this.scriptedIntents = scriptedIntents ?? throw new ArgumentNullException(nameof(scriptedIntents));
        }

        public string BehaviorId { get; }

        public CombatBehaviorDecision DecideIntent(CombatBehaviorContext context)
        {
            if (nextIntentIndex >= scriptedIntents.Count)
            {
                return new CombatBehaviorDecision(BehaviorId, CombatBehaviorIntent.EndTurn(context.ActingUnitId));
            }

            var intent = scriptedIntents[nextIntentIndex];
            nextIntentIndex += 1;
            return new CombatBehaviorDecision(BehaviorId, intent);
        }
    }
}
