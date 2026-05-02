using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public readonly struct CombatDebugCommandRequest
    {
        public CombatDebugCommandRequest(
            CombatDebugCommandType commandType,
            int actingUnitId,
            GridPosition moveDestination,
            int targetUnitId)
        {
            CommandType = commandType;
            ActingUnitId = actingUnitId;
            MoveDestination = moveDestination;
            TargetUnitId = targetUnitId;
        }

        public CombatDebugCommandType CommandType { get; }

        public int ActingUnitId { get; }

        public GridPosition MoveDestination { get; }

        public int TargetUnitId { get; }
    }
}
