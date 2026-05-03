using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class MoveTowardTargetBehavior : ICombatBehaviorDefinition
    {
        public MoveTowardTargetBehavior(string behaviorId, int targetUnitId)
        {
            if (string.IsNullOrWhiteSpace(behaviorId))
            {
                throw new ArgumentException("Behavior id is required.", nameof(behaviorId));
            }

            BehaviorId = behaviorId;
            TargetUnitId = targetUnitId;
        }

        public string BehaviorId { get; }

        public int TargetUnitId { get; }

        public CombatBehaviorDecision DecideIntent(CombatBehaviorContext context)
        {
            return new CombatBehaviorDecision(BehaviorId, BuildIntent(context));
        }

        public EnemyIntentSnapshot DescribeIntent(CombatBehaviorContext context)
        {
            if (!context.TryGetActingUnit(out var actor)
                || !context.TryGetUnit(TargetUnitId, out var target)
                || !target.IsAlive)
            {
                return EnemyIntentSummaryBuilder.BuildForIntent(
                    context,
                    CombatBehaviorIntent.EndTurn(context.ActingUnitId));
            }

            var intent = BuildIntent(context);
            if (intent.IntentType == CombatBehaviorIntentType.Move)
            {
                return EnemyIntentSummaryBuilder.BuildForIntent(
                    context,
                    intent,
                    actionNameOverride: "Move",
                    summaryOverride: $"Move toward {target.Definition.Name}",
                    targetUnitIdOverride: target.UnitId);
            }

            return EnemyIntentSummaryBuilder.BuildForIntent(context, intent);
        }

        private CombatBehaviorIntent BuildIntent(CombatBehaviorContext context)
        {
            if (!context.TryGetActingUnit(out var actor)
                || !context.TryGetUnit(TargetUnitId, out var target)
                || !target.IsAlive)
            {
                return CombatBehaviorIntent.EndTurn(context.ActingUnitId);
            }

            if (actor.Position.IsOrthogonallyAdjacentTo(target.Position)
                && !string.IsNullOrWhiteSpace(actor.Definition.DefaultAttackAbilityId))
            {
                return CombatBehaviorIntent.UseAbility(actor.UnitId, actor.Definition.DefaultAttackAbilityId!, target.UnitId);
            }

            var destination = GetDestination(context, actor.Position, target.Position);
            if (!destination.HasValue)
            {
                return CombatBehaviorIntent.EndTurn(actor.UnitId);
            }

            return CombatBehaviorIntent.Move(actor.UnitId, destination.Value);
        }

        private static GridPosition? GetDestination(CombatBehaviorContext context, GridPosition from, GridPosition to)
        {
            var verticalStep = GetVerticalStep(from, to);
            var horizontalStep = GetHorizontalStep(from, to);

            if (verticalStep.HasValue && IsMoveCandidateValid(context, verticalStep.Value))
            {
                return verticalStep.Value;
            }

            if (horizontalStep.HasValue && IsMoveCandidateValid(context, horizontalStep.Value))
            {
                return horizontalStep.Value;
            }

            if (verticalStep.HasValue)
            {
                return verticalStep.Value;
            }

            if (horizontalStep.HasValue)
            {
                return horizontalStep.Value;
            }

            return null;
        }

        private static bool IsMoveCandidateValid(CombatBehaviorContext context, GridPosition destination)
        {
            return context.Board.Contains(destination) && !context.TryGetUnitAtPosition(destination, out _);
        }

        private static GridPosition? GetVerticalStep(GridPosition from, GridPosition to)
        {
            if (from.Y == to.Y)
            {
                return null;
            }

            return new GridPosition(from.X, from.Y + (to.Y > from.Y ? 1 : -1));
        }

        private static GridPosition? GetHorizontalStep(GridPosition from, GridPosition to)
        {
            if (from.X == to.X)
            {
                return null;
            }

            return new GridPosition(from.X + (to.X > from.X ? 1 : -1), from.Y);
        }
    }
}
