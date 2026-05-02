using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public static class CombatRuntimeAbilityRequestResolver
    {
        public static AbilityUseRequest CreateRuntimeRequest(CombatState state, CombatUnitState actor, AbilityDefinition ability)
        {
            switch (ability.TargetingMode)
            {
                case AbilityTargetingMode.Self:
                    return new AbilityUseRequest(actor.Id, ability.Id);

                case AbilityTargetingMode.AdjacentUnit:
                    return new AbilityUseRequest(actor.Id, ability.Id, FindAdjacentEnemyUnitId(state, actor));

                case AbilityTargetingMode.SpecificTile:
                case AbilityTargetingMode.DirectionalShape:
                case AbilityTargetingMode.Line:
                case AbilityTargetingMode.Area:
                case AbilityTargetingMode.AllUnitsInAffectedShape:
                    return new AbilityUseRequest(actor.Id, ability.Id, targetPosition: ResolveForwardTargetPosition(state, actor));

                default:
                    return new AbilityUseRequest(actor.Id, ability.Id);
            }
        }

        public static IReadOnlyList<GridPosition> ResolveEffectTiles(CombatState state, CombatUnitState actor, AbilityDefinition ability)
        {
            var request = CreateRuntimeRequest(state, actor, ability);
            var resolvedTiles = CombatAbilityResolver.ResolveAffectedTiles(state, actor, ability, request, out var failureReason);
            if (failureReason != CombatFailureReason.None)
            {
                return System.Array.Empty<GridPosition>();
            }

            var positions = new List<GridPosition>(resolvedTiles.Count);
            for (var index = 0; index < resolvedTiles.Count; index += 1)
            {
                positions.Add(resolvedTiles[index].Position);
            }

            return positions;
        }

        private static int? FindAdjacentEnemyUnitId(CombatState state, CombatUnitState actor)
        {
            foreach (var unitEntry in state.UnitsById)
            {
                var candidate = unitEntry.Value;
                if (!candidate.IsAlive || candidate.Side == actor.Side)
                {
                    continue;
                }

                if (actor.Position.IsOrthogonallyAdjacentTo(candidate.Position))
                {
                    return candidate.Id;
                }
            }

            return null;
        }

        private static GridPosition ResolveForwardTargetPosition(CombatState state, CombatUnitState actor)
        {
            var forward = new GridPosition(actor.Position.X, actor.Position.Y + 1);
            if (state.Board.Contains(forward))
            {
                return forward;
            }

            var backward = new GridPosition(actor.Position.X, actor.Position.Y - 1);
            if (state.Board.Contains(backward))
            {
                return backward;
            }

            return actor.Position;
        }
    }
}
