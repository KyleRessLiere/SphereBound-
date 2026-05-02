using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class SpamAbilityBehavior : ICombatBehaviorDefinition
    {
        public SpamAbilityBehavior(string behaviorId, string abilityId, int? preferredTargetUnitId = null)
        {
            if (string.IsNullOrWhiteSpace(behaviorId))
            {
                throw new ArgumentException("Behavior id is required.", nameof(behaviorId));
            }

            if (string.IsNullOrWhiteSpace(abilityId))
            {
                throw new ArgumentException("Ability id is required.", nameof(abilityId));
            }

            BehaviorId = behaviorId;
            AbilityId = abilityId;
            PreferredTargetUnitId = preferredTargetUnitId;
        }

        public string BehaviorId { get; }

        public string AbilityId { get; }

        public int? PreferredTargetUnitId { get; }

        public CombatBehaviorDecision DecideIntent(CombatBehaviorContext context)
        {
            if (!context.TryGetActingUnit(out var actor))
            {
                return new CombatBehaviorDecision(BehaviorId, CombatBehaviorIntent.EndTurn(context.ActingUnitId));
            }

            var targetUnitId = PreferredTargetUnitId;
            if (!targetUnitId.HasValue)
            {
                targetUnitId = TryFindFirstOpponentUnitId(context, actor.Side);
            }

            return new CombatBehaviorDecision(
                BehaviorId,
                CombatBehaviorIntent.UseAbility(actor.UnitId, AbilityId, targetUnitId));
        }

        private static int? TryFindFirstOpponentUnitId(CombatBehaviorContext context, CombatUnitSide actorSide)
        {
            for (var index = 0; index < context.Units.Count; index += 1)
            {
                var unit = context.Units[index];
                if (unit.IsAlive && unit.Side != actorSide)
                {
                    return unit.UnitId;
                }
            }

            return null;
        }
    }
}
