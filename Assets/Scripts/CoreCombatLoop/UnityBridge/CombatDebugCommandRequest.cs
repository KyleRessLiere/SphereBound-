using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public readonly struct CombatDebugCommandRequest
    {
        private CombatDebugCommandRequest(
            CombatDebugCommandType commandType,
            int actingUnitId,
            GridPosition? moveDestination,
            int? targetUnitId,
            string? abilityId,
            GridPosition? targetPosition)
        {
            CommandType = commandType;
            ActingUnitId = actingUnitId;
            MoveDestination = moveDestination;
            TargetUnitId = targetUnitId;
            AbilityId = abilityId;
            TargetPosition = targetPosition;
        }

        public static CombatDebugCommandRequest Move(int actingUnitId, GridPosition moveDestination)
        {
            return new CombatDebugCommandRequest(CombatDebugCommandType.Move, actingUnitId, moveDestination, null, null, null);
        }

        public static CombatDebugCommandRequest Attack(int actingUnitId, int targetUnitId)
        {
            return new CombatDebugCommandRequest(CombatDebugCommandType.Attack, actingUnitId, null, targetUnitId, null, null);
        }

        public static CombatDebugCommandRequest EndTurn(int actingUnitId)
        {
            return new CombatDebugCommandRequest(CombatDebugCommandType.EndTurn, actingUnitId, null, null, null, null);
        }

        public static CombatDebugCommandRequest Ability(int actingUnitId, string abilityId, int? targetUnitId, GridPosition? targetPosition)
        {
            return new CombatDebugCommandRequest(CombatDebugCommandType.Ability, actingUnitId, null, targetUnitId, abilityId, targetPosition);
        }

        public CombatDebugCommandType CommandType { get; }

        public int ActingUnitId { get; }

        public GridPosition? MoveDestination { get; }

        public int? TargetUnitId { get; }

        public string? AbilityId { get; }

        public GridPosition? TargetPosition { get; }
    }
}
