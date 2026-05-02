using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatBehaviorIntent
    {
        private CombatBehaviorIntent(
            CombatBehaviorIntentType intentType,
            int actorUnitId,
            GridPosition? targetPosition,
            int? targetUnitId,
            string? abilityId)
        {
            IntentType = intentType;
            ActorUnitId = actorUnitId;
            TargetPosition = targetPosition;
            TargetUnitId = targetUnitId;
            AbilityId = abilityId;
        }

        public CombatBehaviorIntentType IntentType { get; }

        public int ActorUnitId { get; }

        public GridPosition? TargetPosition { get; }

        public int? TargetUnitId { get; }

        public string? AbilityId { get; }

        public static CombatBehaviorIntent Pass(int actorUnitId)
        {
            return new CombatBehaviorIntent(CombatBehaviorIntentType.Pass, actorUnitId, null, null, null);
        }

        public static CombatBehaviorIntent EndTurn(int actorUnitId)
        {
            return new CombatBehaviorIntent(CombatBehaviorIntentType.EndTurn, actorUnitId, null, null, null);
        }

        public static CombatBehaviorIntent Move(int actorUnitId, GridPosition targetPosition)
        {
            return new CombatBehaviorIntent(CombatBehaviorIntentType.Move, actorUnitId, targetPosition, null, null);
        }

        public static CombatBehaviorIntent UseAbility(
            int actorUnitId,
            string abilityId,
            int? targetUnitId = null,
            GridPosition? targetPosition = null)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                throw new ArgumentException("Ability id is required.", nameof(abilityId));
            }

            return new CombatBehaviorIntent(CombatBehaviorIntentType.UseAbility, actorUnitId, targetPosition, targetUnitId, abilityId);
        }
    }
}
