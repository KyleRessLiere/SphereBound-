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
            AppendSuccessfulActionTail(state, actor, CombatActionType.Move, events);
            return new CombatActionResult(true, events, CombatFailureReason.None);
        }

        public CombatActionResult ResolveAttack(CombatState state, int attackerUnitId, int targetUnitId)
        {
            var events = new List<ICombatEvent>
            {
                new AttackRequested(attackerUnitId, targetUnitId),
                new ActionStarted(attackerUnitId, CombatActionType.Attack),
            };

            if (!TryGetValidActor(state, attackerUnitId, CombatActionType.Attack, events, out var attacker))
            {
                return BuildFailedActionResult(events, attackerUnitId, CombatActionType.Attack);
            }

            if (!state.TryGetUnit(targetUnitId, out var target))
            {
                events.Add(new ActionFailed(attackerUnitId, CombatActionType.Attack, CombatFailureReason.TargetMissing));
                events.Add(new ActionEnded(attackerUnitId, CombatActionType.Attack));
                return new CombatActionResult(false, events, CombatFailureReason.TargetMissing);
            }

            if (!target.IsAlive)
            {
                events.Add(new ActionFailed(attackerUnitId, CombatActionType.Attack, CombatFailureReason.TargetDead));
                events.Add(new ActionEnded(attackerUnitId, CombatActionType.Attack));
                return new CombatActionResult(false, events, CombatFailureReason.TargetDead);
            }

            if (!attacker.Position.IsOrthogonallyAdjacentTo(target.Position))
            {
                events.Add(new ActionFailed(attackerUnitId, CombatActionType.Attack, CombatFailureReason.TargetNotAdjacent));
                events.Add(new ActionEnded(attackerUnitId, CombatActionType.Attack));
                return new CombatActionResult(false, events, CombatFailureReason.TargetNotAdjacent);
            }

            ApplyDamage(state, attacker, target, 1, events);
            AppendSuccessfulActionTail(state, attacker, CombatActionType.Attack, events);
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

                events.Add(new TurnEnded(CombatTurnSide.Enemy));

                if (state.ContainsUnit(CombatScenarioFactory.PlayerUnitId) && state.ContainsUnit(CombatScenarioFactory.EnemyUnitId))
                {
                    state.ActiveTurn = CombatTurnSide.Player;
                    state.RemainingPlayerActions = CombatScenarioFactory.PlayerActionsPerTurn;
                    events.Add(new TurnStarted(CombatTurnSide.Player));
                }
            }

            return events;
        }

        public CombatActionResult RunEnemyBehavior(CombatState state, int enemyUnitId)
        {
            if (!state.TryGetUnit(enemyUnitId, out var enemy))
            {
                return new CombatActionResult(false, System.Array.Empty<ICombatEvent>(), CombatFailureReason.ActorMissing);
            }

            if (!state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player))
            {
                return new CombatActionResult(false, System.Array.Empty<ICombatEvent>(), CombatFailureReason.TargetMissing);
            }

            if (enemy.Position.IsOrthogonallyAdjacentTo(player.Position))
            {
                return ResolveAttack(state, enemyUnitId, player.Id);
            }

            return ResolveMove(state, enemyUnitId, GetEnemyMoveDestination(state, enemy, player));
        }

        private static CombatFailureReason ValidateMovement(CombatState state, CombatUnitState actor, GridPosition destination)
        {
            if (!state.Board.Contains(destination))
            {
                return CombatFailureReason.DestinationOutOfBounds;
            }

            if (!actor.Position.IsOrthogonallyAdjacentTo(destination))
            {
                return CombatFailureReason.DestinationNotOrthogonallyAdjacent;
            }

            if (state.TryGetUnitAtPosition(destination, out _))
            {
                return CombatFailureReason.DestinationOccupied;
            }

            return CombatFailureReason.None;
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
            List<ICombatEvent> events)
        {
            if (actor.Side == CombatUnitSide.Player)
            {
                state.RemainingPlayerActions -= 1;
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

            if (actor.Side == CombatUnitSide.Player && state.RemainingPlayerActions <= 0)
            {
                events.Add(new ActionFailed(unitId, actionType, CombatFailureReason.NoActionsRemaining));
                return false;
            }

            return true;
        }
    }
}
