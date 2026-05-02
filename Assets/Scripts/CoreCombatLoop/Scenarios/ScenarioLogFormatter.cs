using System.Globalization;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Scenarios
{
    public static class ScenarioLogFormatter
    {
        public static string Format(ICombatEvent combatEvent)
        {
            switch (combatEvent)
            {
                case TurnStarted turnStarted:
                    return $"TurnStarted side={turnStarted.Side}";
                case TurnEnded turnEnded:
                    return $"TurnEnded side={turnEnded.Side}";
                case ActionStarted actionStarted:
                    return $"ActionStarted unit={actionStarted.UnitId} action={actionStarted.ActionType}";
                case ActionSpent actionSpent:
                    return $"ActionSpent unit={actionSpent.UnitId} remainingActions={actionSpent.RemainingPlayerActions}";
                case ActionEnded actionEnded:
                    return $"ActionEnded unit={actionEnded.UnitId} action={actionEnded.ActionType}";
                case MoveRequested moveRequested:
                    return $"MoveRequested unit={moveRequested.UnitId} destination={moveRequested.Destination}";
                case UnitMoved unitMoved:
                    return $"UnitMoved unit={unitMoved.UnitId} from={unitMoved.From} to={unitMoved.To}";
                case AttackRequested attackRequested:
                    return $"AttackRequested attacker={attackRequested.AttackerUnitId} target={attackRequested.TargetUnitId}";
                case AbilityRequested abilityRequested:
                    return $"AbilityRequested actor={abilityRequested.ActorUnitId} ability={abilityRequested.AbilityId} targetUnit={abilityRequested.TargetUnitId?.ToString() ?? "none"} targetPosition={abilityRequested.TargetPosition?.ToString() ?? "none"}";
                case DamageRequested damageRequested:
                    return $"DamageRequested source={damageRequested.SourceUnitId} target={damageRequested.TargetUnitId} amount={damageRequested.Amount.ToString(CultureInfo.InvariantCulture)}";
                case UnitDamaged unitDamaged:
                    return $"UnitDamaged unit={unitDamaged.UnitId} previous={unitDamaged.PreviousHealth} current={unitDamaged.CurrentHealth} amount={unitDamaged.Amount}";
                case UnitDying unitDying:
                    return $"UnitDying unit={unitDying.UnitId}";
                case UnitDeath unitDeath:
                    return $"UnitDeath unit={unitDeath.UnitId}";
                case UnitRemoved unitRemoved:
                    return $"UnitRemoved unit={unitRemoved.UnitId} position={unitRemoved.Position}";
                case ActionFailed actionFailed:
                    return $"ActionFailed unit={actionFailed.UnitId} action={actionFailed.ActionType} reason={actionFailed.Reason}";
                case MoveBlocked moveBlocked:
                    return $"MoveBlocked unit={moveBlocked.UnitId} destination={moveBlocked.Destination} reason={moveBlocked.Reason}";
                default:
                    return combatEvent.GetType().Name;
            }
        }
    }
}
