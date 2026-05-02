namespace Spherebound.CoreCombatLoop.Core
{
    public enum CombatFailureReason
    {
        None = 0,
        ActorMissing = 1,
        ActorDead = 2,
        OutOfTurn = 3,
        NoActionsRemaining = 4,
        DestinationOutOfBounds = 5,
        DestinationOccupied = 6,
        DestinationNotOrthogonallyAdjacent = 7,
        TargetMissing = 8,
        TargetDead = 9,
        TargetNotAdjacent = 10,
        AbilityMissing = 11,
        InvalidTarget = 12,
        TargetOutOfBounds = 13,
        NoAffectedTiles = 14,
        BehaviorMissing = 15,
    }
}
