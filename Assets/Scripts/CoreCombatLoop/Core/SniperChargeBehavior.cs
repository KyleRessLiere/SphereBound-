using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class SniperChargeBehavior : ICombatBehaviorDefinition
    {
        private readonly int targetUnitId;
        private readonly string lineShotAbilityId;
        private readonly string lineShotName;
        private readonly int initialChargeTurns;
        private bool isCharging;
        private int chargeTurnsRemaining;

        public SniperChargeBehavior(
            string behaviorId,
            int targetUnitId,
            string lineShotAbilityId,
            string lineShotName,
            int initialChargeTurns = 2)
        {
            if (string.IsNullOrWhiteSpace(behaviorId))
            {
                throw new ArgumentException("Behavior id is required.", nameof(behaviorId));
            }

            if (string.IsNullOrWhiteSpace(lineShotAbilityId))
            {
                throw new ArgumentException("Ability id is required.", nameof(lineShotAbilityId));
            }

            if (string.IsNullOrWhiteSpace(lineShotName))
            {
                throw new ArgumentException("Ability name is required.", nameof(lineShotName));
            }

            BehaviorId = behaviorId;
            this.targetUnitId = targetUnitId;
            this.lineShotAbilityId = lineShotAbilityId;
            this.lineShotName = lineShotName;
            this.initialChargeTurns = initialChargeTurns;
        }

        public string BehaviorId { get; }

        public CombatBehaviorDecision DecideIntent(CombatBehaviorContext context)
        {
            if (!TryGetActorAndTarget(context, out var actor, out var target))
            {
                ResetCharge();
                return new CombatBehaviorDecision(BehaviorId, CombatBehaviorIntent.EndTurn(context.ActingUnitId));
            }

            if (isCharging)
            {
                if (!IsAligned(actor.Position, target.Position))
                {
                    ResetCharge();
                    return BuildMoveOrEndTurnDecision(context, actor, target);
                }

                if (chargeTurnsRemaining > 1)
                {
                    chargeTurnsRemaining -= 1;
                    return new CombatBehaviorDecision(BehaviorId, CombatBehaviorIntent.EndTurn(actor.UnitId));
                }

                ResetCharge();
                return new CombatBehaviorDecision(
                    BehaviorId,
                    CombatBehaviorIntent.UseAbility(actor.UnitId, lineShotAbilityId, target.UnitId, target.Position));
            }

            if (IsAligned(actor.Position, target.Position))
            {
                isCharging = true;
                chargeTurnsRemaining = initialChargeTurns;
                return new CombatBehaviorDecision(BehaviorId, CombatBehaviorIntent.EndTurn(actor.UnitId));
            }

            return BuildMoveOrEndTurnDecision(context, actor, target);
        }

        public EnemyIntentSnapshot DescribeIntent(CombatBehaviorContext context)
        {
            if (!TryGetActorAndTarget(context, out var actor, out var target))
            {
                ResetCharge();
                return EnemyIntentSummaryBuilder.BuildForIntent(
                    context,
                    CombatBehaviorIntent.EndTurn(context.ActingUnitId));
            }

            if (isCharging)
            {
                if (!IsAligned(actor.Position, target.Position))
                {
                    return BuildMoveOrEndTurnIntent(context, actor, target);
                }

                if (chargeTurnsRemaining >= 1)
                {
                    return EnemyIntentSummaryBuilder.BuildCharge(
                        context,
                        lineShotName,
                        chargeTurnsRemaining,
                        target.UnitId,
                        target.Position);
                }
            }

            if (IsAligned(actor.Position, target.Position))
            {
                return EnemyIntentSummaryBuilder.BuildCharge(
                    context,
                    lineShotName,
                    initialChargeTurns,
                    target.UnitId,
                    target.Position);
            }

            return BuildMoveOrEndTurnIntent(context, actor, target);
        }

        private CombatBehaviorDecision BuildMoveOrEndTurnDecision(
            CombatBehaviorContext context,
            CombatBehaviorUnitView actor,
            CombatBehaviorUnitView target)
        {
            var destination = GetDestination(context, actor.Position, target.Position);
            if (!destination.HasValue)
            {
                return new CombatBehaviorDecision(BehaviorId, CombatBehaviorIntent.EndTurn(actor.UnitId));
            }

            return new CombatBehaviorDecision(BehaviorId, CombatBehaviorIntent.Move(actor.UnitId, destination.Value));
        }

        private EnemyIntentSnapshot BuildMoveOrEndTurnIntent(
            CombatBehaviorContext context,
            CombatBehaviorUnitView actor,
            CombatBehaviorUnitView target)
        {
            var destination = GetDestination(context, actor.Position, target.Position);
            if (!destination.HasValue)
            {
                return EnemyIntentSummaryBuilder.BuildForIntent(context, CombatBehaviorIntent.EndTurn(actor.UnitId));
            }

            return EnemyIntentSummaryBuilder.BuildForIntent(
                context,
                CombatBehaviorIntent.Move(actor.UnitId, destination.Value),
                actionNameOverride: "Move",
                summaryOverride: $"Move toward {target.Definition.Name}",
                targetUnitIdOverride: target.UnitId,
                targetPositionOverride: destination.Value);
        }

        private bool TryGetActorAndTarget(
            CombatBehaviorContext context,
            out CombatBehaviorUnitView actor,
            out CombatBehaviorUnitView target)
        {
            if (!context.TryGetActingUnit(out actor)
                || !context.TryGetUnit(targetUnitId, out target)
                || !target.IsAlive)
            {
                actor = null!;
                target = null!;
                return false;
            }

            return true;
        }

        private static bool IsAligned(GridPosition actorPosition, GridPosition targetPosition)
        {
            return actorPosition.X == targetPosition.X || actorPosition.Y == targetPosition.Y;
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

        private void ResetCharge()
        {
            isCharging = false;
            chargeTurnsRemaining = 0;
        }
    }
}
