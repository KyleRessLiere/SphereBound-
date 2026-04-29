namespace Spherebound.CoreCombatLoop.Core
{
    public interface ICombatEvent
    {
    }

    public sealed class MoveRequested : ICombatEvent
    {
        public MoveRequested(int unitId, GridPosition destination)
        {
            UnitId = unitId;
            Destination = destination;
        }

        public int UnitId { get; }

        public GridPosition Destination { get; }
    }

    public sealed class AttackRequested : ICombatEvent
    {
        public AttackRequested(int attackerUnitId, int targetUnitId)
        {
            AttackerUnitId = attackerUnitId;
            TargetUnitId = targetUnitId;
        }

        public int AttackerUnitId { get; }

        public int TargetUnitId { get; }
    }

    public sealed class DamageRequested : ICombatEvent
    {
        public DamageRequested(int sourceUnitId, int targetUnitId, int amount)
        {
            SourceUnitId = sourceUnitId;
            TargetUnitId = targetUnitId;
            Amount = amount;
        }

        public int SourceUnitId { get; }

        public int TargetUnitId { get; }

        public int Amount { get; }
    }

    public sealed class TurnStarted : ICombatEvent
    {
        public TurnStarted(CombatTurnSide side)
        {
            Side = side;
        }

        public CombatTurnSide Side { get; }
    }

    public sealed class TurnEnded : ICombatEvent
    {
        public TurnEnded(CombatTurnSide side)
        {
            Side = side;
        }

        public CombatTurnSide Side { get; }
    }

    public sealed class ActionStarted : ICombatEvent
    {
        public ActionStarted(int unitId, CombatActionType actionType)
        {
            UnitId = unitId;
            ActionType = actionType;
        }

        public int UnitId { get; }

        public CombatActionType ActionType { get; }
    }

    public sealed class ActionSpent : ICombatEvent
    {
        public ActionSpent(int unitId, int remainingPlayerActions)
        {
            UnitId = unitId;
            RemainingPlayerActions = remainingPlayerActions;
        }

        public int UnitId { get; }

        public int RemainingPlayerActions { get; }
    }

    public sealed class ActionEnded : ICombatEvent
    {
        public ActionEnded(int unitId, CombatActionType actionType)
        {
            UnitId = unitId;
            ActionType = actionType;
        }

        public int UnitId { get; }

        public CombatActionType ActionType { get; }
    }

    public sealed class UnitMoved : ICombatEvent
    {
        public UnitMoved(int unitId, GridPosition from, GridPosition to)
        {
            UnitId = unitId;
            From = from;
            To = to;
        }

        public int UnitId { get; }

        public GridPosition From { get; }

        public GridPosition To { get; }
    }

    public sealed class UnitDamaged : ICombatEvent
    {
        public UnitDamaged(int unitId, int previousHealth, int currentHealth, int amount)
        {
            UnitId = unitId;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            Amount = amount;
        }

        public int UnitId { get; }

        public int PreviousHealth { get; }

        public int CurrentHealth { get; }

        public int Amount { get; }
    }

    public sealed class UnitDying : ICombatEvent
    {
        public UnitDying(int unitId)
        {
            UnitId = unitId;
        }

        public int UnitId { get; }
    }

    public sealed class UnitDeath : ICombatEvent
    {
        public UnitDeath(int unitId)
        {
            UnitId = unitId;
        }

        public int UnitId { get; }
    }

    public sealed class UnitRemoved : ICombatEvent
    {
        public UnitRemoved(int unitId, GridPosition position)
        {
            UnitId = unitId;
            Position = position;
        }

        public int UnitId { get; }

        public GridPosition Position { get; }
    }

    public sealed class ActionFailed : ICombatEvent
    {
        public ActionFailed(int unitId, CombatActionType actionType, CombatFailureReason reason)
        {
            UnitId = unitId;
            ActionType = actionType;
            Reason = reason;
        }

        public int UnitId { get; }

        public CombatActionType ActionType { get; }

        public CombatFailureReason Reason { get; }
    }

    public sealed class MoveBlocked : ICombatEvent
    {
        public MoveBlocked(int unitId, GridPosition destination, CombatFailureReason reason)
        {
            UnitId = unitId;
            Destination = destination;
            Reason = reason;
        }

        public int UnitId { get; }

        public GridPosition Destination { get; }

        public CombatFailureReason Reason { get; }
    }
}
