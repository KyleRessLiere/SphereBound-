using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Core
{
    public static class CombatAbilityResolver
    {
        public static IReadOnlyList<ResolvedAbilityTile> ResolveAffectedTiles(
            CombatState state,
            CombatUnitState actor,
            AbilityDefinition ability,
            AbilityUseRequest request,
            out CombatFailureReason failureReason)
        {
            var resolvedTiles = new List<ResolvedAbilityTile>();
            failureReason = CombatFailureReason.None;

            if (!TryResolveAnchorPosition(state, actor, ability, request, out var anchorPosition, out failureReason))
            {
                return resolvedTiles;
            }

            var offsets = ability.TilePattern.Offsets;
            for (var index = 0; index < offsets.Count; index += 1)
            {
                var translatedPosition = TranslateOffset(actor.Position, anchorPosition, ability.TilePattern.Anchor, offsets[index]);
                if (!state.Board.Contains(translatedPosition))
                {
                    continue;
                }

                if (state.TryGetUnitAtPosition(translatedPosition, out var occupyingUnit))
                {
                    resolvedTiles.Add(new ResolvedAbilityTile(translatedPosition, occupyingUnit.Id));
                }
                else
                {
                    resolvedTiles.Add(new ResolvedAbilityTile(translatedPosition, null));
                }
            }

            if (resolvedTiles.Count == 0)
            {
                failureReason = CombatFailureReason.NoAffectedTiles;
                return resolvedTiles;
            }

            return resolvedTiles;
        }

        public static bool MatchesTargetRule(
            CombatUnitState actor,
            CombatUnitState? target,
            AbilityTargetRule targetRule)
        {
            if (target == null)
            {
                return (targetRule & AbilityTargetRule.EmptyTile) != 0
                    || targetRule == AbilityTargetRule.None;
            }

            if ((targetRule & AbilityTargetRule.OccupiedTile) == 0
                && (targetRule & AbilityTargetRule.Self) == 0
                && (targetRule & AbilityTargetRule.Ally) == 0
                && (targetRule & AbilityTargetRule.Enemy) == 0)
            {
                return false;
            }

            if ((targetRule & AbilityTargetRule.Self) != 0 && target.Id == actor.Id)
            {
                return true;
            }

            if ((targetRule & AbilityTargetRule.Ally) != 0 && target.Side == actor.Side && target.Id != actor.Id)
            {
                return true;
            }

            if ((targetRule & AbilityTargetRule.Enemy) != 0 && target.Side != actor.Side)
            {
                return true;
            }

            if ((targetRule & AbilityTargetRule.OccupiedTile) != 0
                && (targetRule & (AbilityTargetRule.Self | AbilityTargetRule.Ally | AbilityTargetRule.Enemy)) == 0)
            {
                return true;
            }

            return false;
        }

        private static bool TryResolveAnchorPosition(
            CombatState state,
            CombatUnitState actor,
            AbilityDefinition ability,
            AbilityUseRequest request,
            out GridPosition anchorPosition,
            out CombatFailureReason failureReason)
        {
            failureReason = CombatFailureReason.None;
            switch (ability.TargetingMode)
            {
                case AbilityTargetingMode.Self:
                    anchorPosition = actor.Position;
                    return true;

                case AbilityTargetingMode.AdjacentUnit:
                    if (!request.TargetUnitId.HasValue)
                    {
                        anchorPosition = default;
                        failureReason = CombatFailureReason.TargetMissing;
                        return false;
                    }

                    if (!state.TryGetUnit(request.TargetUnitId.Value, out var adjacentTarget))
                    {
                        anchorPosition = default;
                        failureReason = CombatFailureReason.TargetMissing;
                        return false;
                    }

                    if (!adjacentTarget.IsAlive)
                    {
                        anchorPosition = default;
                        failureReason = CombatFailureReason.TargetDead;
                        return false;
                    }

                    if (!actor.Position.IsOrthogonallyAdjacentTo(adjacentTarget.Position))
                    {
                        anchorPosition = default;
                        failureReason = CombatFailureReason.TargetNotAdjacent;
                        return false;
                    }

                    if (!MatchesTargetRule(actor, adjacentTarget, ability.TargetRule))
                    {
                        anchorPosition = default;
                        failureReason = CombatFailureReason.InvalidTarget;
                        return false;
                    }

                    anchorPosition = adjacentTarget.Position;
                    return true;

                case AbilityTargetingMode.SpecificTile:
                case AbilityTargetingMode.DirectionalShape:
                case AbilityTargetingMode.Line:
                case AbilityTargetingMode.Area:
                case AbilityTargetingMode.AllUnitsInAffectedShape:
                    if (!request.TargetPosition.HasValue)
                    {
                        anchorPosition = default;
                        failureReason = CombatFailureReason.InvalidTarget;
                        return false;
                    }

                    if (!state.Board.Contains(request.TargetPosition.Value))
                    {
                        anchorPosition = default;
                        failureReason = CombatFailureReason.TargetOutOfBounds;
                        return false;
                    }

                    anchorPosition = request.TargetPosition.Value;
                    return true;

                default:
                    anchorPosition = default;
                    failureReason = CombatFailureReason.InvalidTarget;
                    return false;
            }
        }

        private static GridPosition TranslateOffset(
            GridPosition actorPosition,
            GridPosition anchorPosition,
            AbilityTilePatternAnchor anchor,
            GridOffset offset)
        {
            switch (anchor)
            {
                case AbilityTilePatternAnchor.ActorPosition:
                    return new GridPosition(actorPosition.X + offset.X, actorPosition.Y + offset.Y);

                case AbilityTilePatternAnchor.TargetPosition:
                    return new GridPosition(anchorPosition.X + offset.X, anchorPosition.Y + offset.Y);

                case AbilityTilePatternAnchor.DirectionFromActorToTarget:
                {
                    var normalizedDirection = NormalizeDirection(actorPosition, anchorPosition);
                    var rotatedOffset = RotateIntoDirection(offset, normalizedDirection);
                    return new GridPosition(actorPosition.X + rotatedOffset.X, actorPosition.Y + rotatedOffset.Y);
                }

                default:
                    return new GridPosition(anchorPosition.X + offset.X, anchorPosition.Y + offset.Y);
            }
        }

        private static GridOffset NormalizeDirection(GridPosition actorPosition, GridPosition anchorPosition)
        {
            var deltaX = anchorPosition.X - actorPosition.X;
            var deltaY = anchorPosition.Y - actorPosition.Y;
            if (deltaX == 0 && deltaY == 0)
            {
                return new GridOffset(0, 1);
            }

            if (deltaX != 0)
            {
                return new GridOffset(deltaX > 0 ? 1 : -1, 0);
            }

            return new GridOffset(0, deltaY > 0 ? 1 : -1);
        }

        private static GridOffset RotateIntoDirection(GridOffset offset, GridOffset direction)
        {
            if (direction.X == 0 && direction.Y == 1)
            {
                return offset;
            }

            if (direction.X == 1 && direction.Y == 0)
            {
                return new GridOffset(offset.Y, -offset.X);
            }

            if (direction.X == 0 && direction.Y == -1)
            {
                return new GridOffset(-offset.X, -offset.Y);
            }

            return new GridOffset(-offset.Y, offset.X);
        }
    }
}
