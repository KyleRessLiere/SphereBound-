using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatEngine
    {
        public IReadOnlyList<ICombatEvent> StartCombat(CombatState state)
        {
            return new ICombatEvent[]
            {
                new TurnStarted(state.ActiveTurn),
            };
        }

        public CombatActionResult RunBehaviorTurn(CombatState state, int unitId)
        {
            var events = new List<ICombatEvent>();

            if (!state.TryGetUnit(unitId, out var actor))
            {
                events.Add(new ActionFailed(unitId, CombatActionType.TurnTransition, CombatFailureReason.ActorMissing));
                return new CombatActionResult(false, events, CombatFailureReason.ActorMissing);
            }

            if (actor.BehaviorAssignment == null)
            {
                events.Add(new ActionFailed(unitId, CombatActionType.TurnTransition, CombatFailureReason.BehaviorMissing));
                return new CombatActionResult(false, events, CombatFailureReason.BehaviorMissing);
            }

            var behaviorAssignment = actor.BehaviorAssignment!;
            var success = true;
            var failureReason = CombatFailureReason.None;

            while (actor.IsAlive)
            {
                var expectedTurn = actor.Side == CombatUnitSide.Player ? CombatTurnSide.Player : CombatTurnSide.Enemy;
                if (state.ActiveTurn != expectedTurn)
                {
                    break;
                }

                if (actor.Side == CombatUnitSide.Player && state.RemainingPlayerActions <= 0)
                {
                    break;
                }

                var context = CombatBehaviorContext.FromState(state, actor.Id);
                var decision = behaviorAssignment.Behavior.DecideIntent(context);
                events.Add(new BehaviorIntentSelected(
                    actor.Id,
                    decision.BehaviorId,
                    decision.Intent.IntentType,
                    decision.Intent.AbilityId,
                    decision.Intent.TargetUnitId,
                    decision.Intent.TargetPosition));

                if (decision.Intent.IntentType == CombatBehaviorIntentType.Pass)
                {
                    break;
                }

                if (decision.Intent.IntentType == CombatBehaviorIntentType.EndTurn)
                {
                    break;
                }

                var intentResult = ResolveBehaviorIntent(state, decision.Intent);
                events.AddRange(intentResult.Events);

                if (!intentResult.Succeeded)
                {
                    success = false;
                    failureReason = intentResult.FailureReason;
                    break;
                }

                if (!state.TryGetUnit(unitId, out actor))
                {
                    break;
                }

                if (actor.Side != CombatUnitSide.Player)
                {
                    break;
                }
            }

            return new CombatActionResult(success, events, failureReason);
        }

        public IReadOnlyList<ICombatEvent> RunBehaviorTurnCycle(CombatState state)
        {
            if (!TryGetCurrentActingUnitId(state, out var actingUnitId))
            {
                return System.Array.Empty<ICombatEvent>();
            }

            var events = new List<ICombatEvent>();
            var turnResult = RunBehaviorTurn(state, actingUnitId);
            events.AddRange(turnResult.Events);

            if (state.ActiveTurn == CombatTurnSide.Player)
            {
                events.AddRange(EndPlayerTurnAndRunEnemyTurn(state));
            }
            else if (state.ActiveTurn == CombatTurnSide.Enemy)
            {
                events.AddRange(AdvanceEnemyTurnToPlayer(state));
            }

            return events;
        }

        public CombatActionResult ResolveMove(CombatState state, int unitId, GridPosition destination)
        {
            var events = new List<ICombatEvent>
            {
                new MoveRequested(unitId, destination),
                new ActionStarted(unitId, CombatActionType.Move),
            };

            if (!TryGetValidActor(state, unitId, CombatActionType.Move, events, out var actor))
            {
                return BuildFailedActionResult(events, unitId, CombatActionType.Move, destination);
            }

            if (!TryValidateActionCost(state, actor, actor.Definition.Movement.ActionCost, CombatActionType.Move, events))
            {
                return BuildFailedActionResult(events, unitId, CombatActionType.Move, destination);
            }

            var failureReason = ValidateMovement(state, actor, destination);
            if (failureReason != CombatFailureReason.None)
            {
                events.Add(new ActionFailed(unitId, CombatActionType.Move, failureReason));
                events.Add(new MoveBlocked(unitId, destination, failureReason));
                events.Add(new ActionEnded(unitId, CombatActionType.Move));
                return new CombatActionResult(false, events, failureReason);
            }

            var origin = actor.Position;
            actor.Position = destination;
            events.Add(new UnitMoved(unitId, origin, destination));
            AppendSuccessfulActionTail(state, actor, CombatActionType.Move, actor.Definition.Movement.ActionCost, events);
            return new CombatActionResult(true, events, CombatFailureReason.None);
        }

        public CombatActionResult ResolveAttack(CombatState state, int attackerUnitId, int targetUnitId)
        {
            var abilityId = CombatScenarioFactory.BasicAttackAbilityId;
            if (state.TryGetUnit(attackerUnitId, out var attacker)
                && !string.IsNullOrWhiteSpace(attacker.Definition.DefaultAttackAbilityId))
            {
                abilityId = attacker.Definition.DefaultAttackAbilityId!;
            }

            return ResolveAbilityInternal(
                state,
                new AbilityUseRequest(attackerUnitId, abilityId, targetUnitId),
                CombatActionType.Attack);
        }

        public CombatActionResult ResolveAbility(CombatState state, AbilityUseRequest request)
        {
            return ResolveAbilityInternal(state, request, CombatActionType.Ability);
        }

        private CombatActionResult ResolveAbilityInternal(
            CombatState state,
            AbilityUseRequest request,
            CombatActionType fallbackActionType)
        {
            var events = new List<ICombatEvent>
            {
                new AbilityRequested(request.ActorUnitId, request.AbilityId, request.TargetUnitId, request.TargetPosition),
                new ActionStarted(request.ActorUnitId, fallbackActionType),
            };

            if (!TryGetValidActor(state, request.ActorUnitId, fallbackActionType, events, out var actor))
            {
                return BuildFailedActionResult(events, request.ActorUnitId, fallbackActionType);
            }

            if (!actor.Definition.TryGetAbility(request.AbilityId, out var ability))
            {
                events.Add(new ActionFailed(request.ActorUnitId, fallbackActionType, CombatFailureReason.AbilityMissing));
                events.Add(new ActionEnded(request.ActorUnitId, fallbackActionType));
                return new CombatActionResult(false, events, CombatFailureReason.AbilityMissing);
            }

            if (!TryValidateActionCost(state, actor, ability.ActionCost, ability.ActionType, events))
            {
                events.Add(new ActionEnded(request.ActorUnitId, ability.ActionType));
                return new CombatActionResult(false, events, CombatFailureReason.NoActionsRemaining);
            }

            var affectedTiles = CombatAbilityResolver.ResolveAffectedTiles(state, actor, ability, request, out var abilityFailureReason);
            if (abilityFailureReason != CombatFailureReason.None)
            {
                events.Add(new ActionFailed(request.ActorUnitId, ability.ActionType, abilityFailureReason));
                events.Add(new ActionEnded(request.ActorUnitId, ability.ActionType));
                return new CombatActionResult(false, events, abilityFailureReason);
            }

            var affectedUnits = GetAffectedUnits(state, affectedTiles, actor, ability.TargetRule);
            if (ability.RequiresAffectedUnit && affectedUnits.Count == 0)
            {
                events.Add(new ActionFailed(request.ActorUnitId, ability.ActionType, CombatFailureReason.NoAffectedTiles));
                events.Add(new ActionEnded(request.ActorUnitId, ability.ActionType));
                return new CombatActionResult(false, events, CombatFailureReason.NoAffectedTiles);
            }

            if (!TryValidateForcedMovementPlans(state, actor, ability, affectedUnits, out var movementPlans, out var movementFailureReason))
            {
                events.Add(new ActionFailed(request.ActorUnitId, ability.ActionType, movementFailureReason));
                events.Add(new ActionEnded(request.ActorUnitId, ability.ActionType));
                return new CombatActionResult(false, events, movementFailureReason);
            }

            ApplyAbilityEffects(state, actor, ability, affectedUnits, movementPlans, events);
            AppendSuccessfulActionTail(state, actor, ability.ActionType, ability.ActionCost, events);
            return new CombatActionResult(true, events, CombatFailureReason.None);
        }

        public IReadOnlyList<ICombatEvent> EndPlayerTurnAndRunEnemyTurn(CombatState state)
        {
            var events = new List<ICombatEvent>();
            if (state.ActiveTurn == CombatTurnSide.Player)
            {
                events.Add(new TurnEnded(CombatTurnSide.Player));
                state.ActiveTurn = CombatTurnSide.Enemy;
                events.Add(new TurnStarted(CombatTurnSide.Enemy));

                if (state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy) && enemy.IsAlive)
                {
                    var enemyEvents = RunEnemyBehavior(state, enemy.Id);
                    events.AddRange(enemyEvents.Events);
                }

                events.AddRange(AdvanceEnemyTurnToPlayer(state));
            }

            return events;
        }

        public CombatActionResult RunEnemyBehavior(CombatState state, int enemyUnitId)
        {
            if (!state.TryGetUnit(enemyUnitId, out var enemy))
            {
                return new CombatActionResult(false, System.Array.Empty<ICombatEvent>(), CombatFailureReason.ActorMissing);
            }

            if (enemy.BehaviorAssignment == null)
            {
                if (!state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out _))
                {
                    return new CombatActionResult(false, System.Array.Empty<ICombatEvent>(), CombatFailureReason.TargetMissing);
                }

                enemy.BehaviorAssignment = CombatBehaviorAssignment.Default(
                    new MoveTowardTargetBehavior("enemy-default-chase", CombatScenarioFactory.PlayerUnitId));
            }

            return RunBehaviorTurn(state, enemyUnitId);
        }

        private static CombatFailureReason ValidateMovement(CombatState state, CombatUnitState actor, GridPosition destination)
        {
            if (!state.Board.Contains(destination))
            {
                return CombatFailureReason.DestinationOutOfBounds;
            }

            var deltaX = destination.X - actor.Position.X;
            if (deltaX < 0)
            {
                deltaX = -deltaX;
            }

            var deltaY = destination.Y - actor.Position.Y;
            if (deltaY < 0)
            {
                deltaY = -deltaY;
            }

            var movement = actor.Definition.Movement;
            if (movement.OrthogonalOnly && !actor.Position.IsOrthogonallyAdjacentTo(destination))
            {
                return CombatFailureReason.DestinationNotOrthogonallyAdjacent;
            }

            if ((deltaX + deltaY) > movement.Range)
            {
                return CombatFailureReason.DestinationNotOrthogonallyAdjacent;
            }

            if (state.TryGetUnitAtPosition(destination, out _))
            {
                return CombatFailureReason.DestinationOccupied;
            }

            return CombatFailureReason.None;
        }

        private CombatActionResult ResolveBehaviorIntent(CombatState state, CombatBehaviorIntent intent)
        {
            switch (intent.IntentType)
            {
                case CombatBehaviorIntentType.Move:
                    if (!intent.TargetPosition.HasValue)
                    {
                        return new CombatActionResult(false, System.Array.Empty<ICombatEvent>(), CombatFailureReason.TargetOutOfBounds);
                    }

                    return ResolveMove(state, intent.ActorUnitId, intent.TargetPosition.Value);

                case CombatBehaviorIntentType.UseAbility:
                    if (string.IsNullOrWhiteSpace(intent.AbilityId))
                    {
                        return new CombatActionResult(false, System.Array.Empty<ICombatEvent>(), CombatFailureReason.AbilityMissing);
                    }

                    return ResolveAbility(state, new AbilityUseRequest(intent.ActorUnitId, intent.AbilityId!, intent.TargetUnitId, intent.TargetPosition));

                case CombatBehaviorIntentType.Pass:
                case CombatBehaviorIntentType.EndTurn:
                    return new CombatActionResult(true, System.Array.Empty<ICombatEvent>(), CombatFailureReason.None);

                default:
                    return new CombatActionResult(false, System.Array.Empty<ICombatEvent>(), CombatFailureReason.InvalidTarget);
            }
        }

        private IReadOnlyList<ICombatEvent> AdvanceEnemyTurnToPlayer(CombatState state)
        {
            var events = new List<ICombatEvent>
            {
                new TurnEnded(CombatTurnSide.Enemy),
            };

            if (state.ContainsUnit(CombatScenarioFactory.PlayerUnitId) && state.ContainsUnit(CombatScenarioFactory.EnemyUnitId))
            {
                state.ActiveTurn = CombatTurnSide.Player;
                state.RemainingPlayerActions = state.PlayerActionsPerTurn;
                events.Add(new TurnStarted(CombatTurnSide.Player));
            }

            return events;
        }

        private static bool TryGetCurrentActingUnitId(CombatState state, out int unitId)
        {
            if (state.ActiveTurn == CombatTurnSide.Player
                && state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player)
                && player.IsAlive)
            {
                unitId = player.Id;
                return true;
            }

            if (state.ActiveTurn == CombatTurnSide.Enemy
                && state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy)
                && enemy.IsAlive)
            {
                unitId = enemy.Id;
                return true;
            }

            unitId = 0;
            return false;
        }

        private static GridPosition GetEnemyMoveDestination(CombatState state, CombatUnitState enemy, CombatUnitState player)
        {
            var verticalStep = GetVerticalStep(enemy.Position, player.Position);
            var horizontalStep = GetHorizontalStep(enemy.Position, player.Position);

            if (verticalStep.HasValue)
            {
                var candidate = verticalStep.Value;
                if (ValidateMovement(state, enemy, candidate) == CombatFailureReason.None)
                {
                    return candidate;
                }
            }

            if (horizontalStep.HasValue)
            {
                var candidate = horizontalStep.Value;
                if (ValidateMovement(state, enemy, candidate) == CombatFailureReason.None)
                {
                    return candidate;
                }
            }

            if (verticalStep.HasValue)
            {
                return verticalStep.Value;
            }

            if (horizontalStep.HasValue)
            {
                return horizontalStep.Value;
            }

            return enemy.Position;
        }

        private static GridPosition? GetVerticalStep(GridPosition from, GridPosition to)
        {
            if (from.Y == to.Y)
            {
                return null;
            }

            var direction = to.Y > from.Y ? 1 : -1;
            return new GridPosition(from.X, from.Y + direction);
        }

        private static GridPosition? GetHorizontalStep(GridPosition from, GridPosition to)
        {
            if (from.X == to.X)
            {
                return null;
            }

            var direction = to.X > from.X ? 1 : -1;
            return new GridPosition(from.X + direction, from.Y);
        }

        private static void ApplyDamage(
            CombatState state,
            CombatUnitState source,
            CombatUnitState target,
            int amount,
            List<ICombatEvent> events)
        {
            events.Add(new DamageRequested(source.Id, target.Id, amount));

            var previousHealth = target.CurrentHealth;
            var currentHealth = previousHealth - amount;
            if (currentHealth < 0)
            {
                currentHealth = 0;
            }

            target.CurrentHealth = currentHealth;
            events.Add(new UnitDamaged(target.Id, previousHealth, currentHealth, amount));

            if (currentHealth > 0)
            {
                return;
            }

            events.Add(new UnitDying(target.Id));
            target.LifeState = UnitLifeState.Dead;
            events.Add(new UnitDeath(target.Id));
            var removalPosition = target.Position;
            state.RemoveUnit(target.Id);
            events.Add(new UnitRemoved(target.Id, removalPosition));
        }

        private static void AppendSuccessfulActionTail(
            CombatState state,
            CombatUnitState actor,
            CombatActionType actionType,
            int actionCost,
            List<ICombatEvent> events)
        {
            if (actor.Side == CombatUnitSide.Player)
            {
                state.RemainingPlayerActions -= actionCost;
                events.Add(new ActionSpent(actor.Id, state.RemainingPlayerActions));
            }

            events.Add(new ActionEnded(actor.Id, actionType));
        }

        private static CombatActionResult BuildFailedActionResult(
            List<ICombatEvent> events,
            int unitId,
            CombatActionType actionType,
            GridPosition? blockedDestination = null)
        {
            var failureEvent = (ActionFailed)events[events.Count - 1];
            if (blockedDestination.HasValue)
            {
                events.Add(new MoveBlocked(unitId, blockedDestination.Value, failureEvent.Reason));
            }

            events.Add(new ActionEnded(unitId, actionType));
            return new CombatActionResult(false, events, failureEvent.Reason);
        }

        private static bool TryGetValidActor(
            CombatState state,
            int unitId,
            CombatActionType actionType,
            List<ICombatEvent> events,
            out CombatUnitState actor)
        {
            if (!state.TryGetUnit(unitId, out actor))
            {
                events.Add(new ActionFailed(unitId, actionType, CombatFailureReason.ActorMissing));
                return false;
            }

            if (!actor.IsAlive)
            {
                events.Add(new ActionFailed(unitId, actionType, CombatFailureReason.ActorDead));
                return false;
            }

            var expectedTurn = actor.Side == CombatUnitSide.Player ? CombatTurnSide.Player : CombatTurnSide.Enemy;
            if (state.ActiveTurn != expectedTurn)
            {
                events.Add(new ActionFailed(unitId, actionType, CombatFailureReason.OutOfTurn));
                return false;
            }

            return true;
        }

        private static bool TryValidateActionCost(
            CombatState state,
            CombatUnitState actor,
            int actionCost,
            CombatActionType actionType,
            List<ICombatEvent> events)
        {
            if (actor.Side == CombatUnitSide.Player && state.RemainingPlayerActions < actionCost)
            {
                events.Add(new ActionFailed(actor.Id, actionType, CombatFailureReason.NoActionsRemaining));
                return false;
            }

            return true;
        }

        private static List<CombatUnitState> GetAffectedUnits(
            CombatState state,
            IReadOnlyList<ResolvedAbilityTile> affectedTiles,
            CombatUnitState actor,
            AbilityTargetRule targetRule)
        {
            var affectedUnits = new List<CombatUnitState>();
            for (var index = 0; index < affectedTiles.Count; index += 1)
            {
                var tile = affectedTiles[index];
                if (!tile.OccupantUnitId.HasValue)
                {
                    continue;
                }

                if (!state.TryGetUnit(tile.OccupantUnitId.Value, out var unit))
                {
                    continue;
                }

                if (!CombatAbilityResolver.MatchesTargetRule(actor, unit, targetRule))
                {
                    continue;
                }

                if (!affectedUnits.Contains(unit))
                {
                    affectedUnits.Add(unit);
                }
            }

            return affectedUnits;
        }

        private static bool TryValidateForcedMovementPlans(
            CombatState state,
            CombatUnitState actor,
            AbilityDefinition ability,
            IReadOnlyList<CombatUnitState> affectedUnits,
            out List<MovementPlan> movementPlans,
            out CombatFailureReason failureReason)
        {
            movementPlans = new List<MovementPlan>();
            failureReason = CombatFailureReason.None;

            for (var effectIndex = 0; effectIndex < ability.Effects.Count; effectIndex += 1)
            {
                var effect = ability.Effects[effectIndex];
                if (effect.Kind != AbilityEffectKind.ForcedMovement)
                {
                    continue;
                }

                for (var unitIndex = 0; unitIndex < affectedUnits.Count; unitIndex += 1)
                {
                    var affectedUnit = affectedUnits[unitIndex];
                    var destination = new GridPosition(
                        affectedUnit.Position.X + effect.ForcedMovementOffset.X,
                        affectedUnit.Position.Y + effect.ForcedMovementOffset.Y);
                    failureReason = ValidateMovement(state, affectedUnit, destination);
                    if (failureReason != CombatFailureReason.None)
                    {
                        return false;
                    }

                    movementPlans.Add(new MovementPlan(affectedUnit, destination));
                }
            }

            return true;
        }

        private static void ApplyAbilityEffects(
            CombatState state,
            CombatUnitState actor,
            AbilityDefinition ability,
            IReadOnlyList<CombatUnitState> affectedUnits,
            IReadOnlyList<MovementPlan> movementPlans,
            List<ICombatEvent> events)
        {
            for (var effectIndex = 0; effectIndex < ability.Effects.Count; effectIndex += 1)
            {
                var effect = ability.Effects[effectIndex];
                switch (effect.Kind)
                {
                    case AbilityEffectKind.Damage:
                        for (var unitIndex = 0; unitIndex < affectedUnits.Count; unitIndex += 1)
                        {
                            var target = affectedUnits[unitIndex];
                            events.Add(new AttackRequested(actor.Id, target.Id));
                            ApplyDamage(state, actor, target, effect.Amount, events);
                        }
                        break;

                    case AbilityEffectKind.Healing:
                        for (var unitIndex = 0; unitIndex < affectedUnits.Count; unitIndex += 1)
                        {
                            ApplyHealing(affectedUnits[unitIndex], effect.Amount);
                        }
                        break;

                    case AbilityEffectKind.ForcedMovement:
                        for (var planIndex = 0; planIndex < movementPlans.Count; planIndex += 1)
                        {
                            ApplyMovementPlan(movementPlans[planIndex], events);
                        }
                        break;
                }
            }
        }

        private static void ApplyHealing(CombatUnitState target, int amount)
        {
            target.CurrentHealth += amount;
        }

        private static void ApplyMovementPlan(MovementPlan movementPlan, List<ICombatEvent> events)
        {
            events.Add(new MoveRequested(movementPlan.Unit.Id, movementPlan.Destination));
            var origin = movementPlan.Unit.Position;
            movementPlan.Unit.Position = movementPlan.Destination;
            events.Add(new UnitMoved(movementPlan.Unit.Id, origin, movementPlan.Destination));
        }

        private readonly struct MovementPlan
        {
            public MovementPlan(CombatUnitState unit, GridPosition destination)
            {
                Unit = unit;
                Destination = destination;
            }

            public CombatUnitState Unit { get; }

            public GridPosition Destination { get; }
        }
    }
}
