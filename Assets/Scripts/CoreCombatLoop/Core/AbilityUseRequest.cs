namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class AbilityUseRequest
    {
        public AbilityUseRequest(int actorUnitId, string abilityId, int? targetUnitId = null, GridPosition? targetPosition = null)
        {
            ActorUnitId = actorUnitId;
            AbilityId = abilityId;
            TargetUnitId = targetUnitId;
            TargetPosition = targetPosition;
        }

        public int ActorUnitId { get; }

        public string AbilityId { get; }

        public int? TargetUnitId { get; }

        public GridPosition? TargetPosition { get; }
    }
}
