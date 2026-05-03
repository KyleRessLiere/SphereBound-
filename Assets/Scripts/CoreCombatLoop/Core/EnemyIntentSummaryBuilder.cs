using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public static class EnemyIntentSummaryBuilder
    {
        public static EnemyIntentSnapshot BuildForIntent(
            CombatBehaviorContext context,
            CombatBehaviorIntent intent,
            EnemyIntentType? intentTypeOverride = null,
            string? actionNameOverride = null,
            string? summaryOverride = null,
            int? countdownValue = null,
            int? targetUnitIdOverride = null,
            GridPosition? targetPositionOverride = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.TryGetActingUnit(out var actor))
            {
                return new EnemyIntentSnapshot(
                    context.ActingUnitId,
                    $"Enemy {context.ActingUnitId}",
                    EnemyIntentType.EndTurn,
                    "end-turn",
                    "End Turn",
                    null,
                    null,
                    null,
                    null,
                    "Unavailable");
            }

            var targetUnitId = targetUnitIdOverride ?? intent.TargetUnitId;
            var targetPosition = targetPositionOverride ?? intent.TargetPosition;
            var targetDisplayName = TryResolveTargetName(context, targetUnitId);
            var intentType = intentTypeOverride ?? ResolveIntentType(intent.IntentType);
            if (!targetPosition.HasValue && targetUnitId.HasValue && context.TryGetUnit(targetUnitId.Value, out var targetUnit))
            {
                targetPosition = targetUnit.Position;
            }

            var actionId = ResolveActionId(intent, intentType);
            var actionName = actionNameOverride ?? ResolveActionName(actor, intent, intentType);
            var summaryText = summaryOverride ?? BuildDefaultSummary(intentType, actionName, targetDisplayName, targetPosition, countdownValue);

            return new EnemyIntentSnapshot(
                actor.UnitId,
                actor.Definition.Name,
                intentType,
                actionId,
                actionName,
                targetUnitId,
                targetDisplayName,
                targetPosition,
                countdownValue,
                summaryText);
        }

        public static EnemyIntentSnapshot BuildCharge(
            CombatBehaviorContext context,
            string actionId,
            string actionName,
            int countdownValue,
            int? targetUnitId = null,
            GridPosition? targetPosition = null)
        {
            return BuildForIntent(
                context,
                CombatBehaviorIntent.UseAbility(context.ActingUnitId, actionId, targetUnitId, targetPosition),
                EnemyIntentType.Charge,
                actionName,
                $"{actionName} - {countdownValue} turns remaining",
                countdownValue,
                targetUnitId,
                targetPosition);
        }

        public static EnemyIntentSnapshot BuildFire(
            CombatBehaviorContext context,
            string actionId,
            string actionName,
            int? targetUnitId = null,
            GridPosition? targetPosition = null)
        {
            var targetName = TryResolveTargetName(context, targetUnitId);
            var summary = targetName != null
                ? $"Fire {actionName} at {targetName} next"
                : targetPosition.HasValue
                    ? $"Fire {actionName} at {targetPosition.Value} next"
                    : $"Fire {actionName} next";

            return BuildForIntent(
                context,
                CombatBehaviorIntent.UseAbility(context.ActingUnitId, actionId, targetUnitId, targetPosition),
                EnemyIntentType.Fire,
                actionName,
                summary,
                null,
                targetUnitId,
                targetPosition);
        }

        private static EnemyIntentType ResolveIntentType(CombatBehaviorIntentType intentType)
        {
            return intentType switch
            {
                CombatBehaviorIntentType.Move => EnemyIntentType.Move,
                CombatBehaviorIntentType.UseAbility => EnemyIntentType.UseAbility,
                CombatBehaviorIntentType.Pass => EnemyIntentType.EndTurn,
                CombatBehaviorIntentType.EndTurn => EnemyIntentType.EndTurn,
                _ => EnemyIntentType.None,
            };
        }

        private static string ResolveActionId(CombatBehaviorIntent intent, EnemyIntentType intentType)
        {
            if ((intentType == EnemyIntentType.UseAbility || intentType == EnemyIntentType.Fire || intentType == EnemyIntentType.Charge)
                && !string.IsNullOrWhiteSpace(intent.AbilityId))
            {
                return intent.AbilityId!;
            }

            return intentType switch
            {
                EnemyIntentType.Move => "move",
                EnemyIntentType.EndTurn => "end-turn",
                _ => "none",
            };
        }

        private static string ResolveActionName(CombatBehaviorUnitView actor, CombatBehaviorIntent intent, EnemyIntentType intentType)
        {
            if (intentType == EnemyIntentType.UseAbility || intentType == EnemyIntentType.Fire)
            {
                if (!string.IsNullOrWhiteSpace(intent.AbilityId)
                    && actor.Definition.TryGetAbility(intent.AbilityId!, out var ability))
                {
                    return ability.Name;
                }

                return string.IsNullOrWhiteSpace(intent.AbilityId) ? "Ability" : intent.AbilityId!;
            }

            return intentType switch
            {
                EnemyIntentType.Move => "Move",
                EnemyIntentType.Charge => "Charge",
                EnemyIntentType.EndTurn => "End Turn",
                _ => "None",
            };
        }

        private static string BuildDefaultSummary(
            EnemyIntentType intentType,
            string actionName,
            string? targetDisplayName,
            GridPosition? targetPosition,
            int? countdownValue)
        {
            return intentType switch
            {
                EnemyIntentType.Move when targetDisplayName != null => $"Move toward {targetDisplayName}",
                EnemyIntentType.Move when targetPosition.HasValue => $"Move to {targetPosition.Value}",
                EnemyIntentType.UseAbility when targetDisplayName != null => $"{actionName} {targetDisplayName}",
                EnemyIntentType.UseAbility when targetPosition.HasValue => $"{actionName} at {targetPosition.Value}",
                EnemyIntentType.Charge => $"{actionName} - {countdownValue ?? 0} turns remaining",
                EnemyIntentType.Fire when targetDisplayName != null => $"Fire {actionName} at {targetDisplayName} next",
                EnemyIntentType.Fire when targetPosition.HasValue => $"Fire {actionName} at {targetPosition.Value} next",
                EnemyIntentType.EndTurn => "End Turn",
                _ => actionName,
            };
        }

        private static string? TryResolveTargetName(CombatBehaviorContext context, int? targetUnitId)
        {
            if (!targetUnitId.HasValue)
            {
                return null;
            }

            return context.TryGetUnit(targetUnitId.Value, out var target)
                ? target.Definition.Name
                : null;
        }
    }
}
