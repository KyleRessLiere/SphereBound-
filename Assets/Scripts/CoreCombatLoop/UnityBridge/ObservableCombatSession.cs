using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class ObservableCombatSession : ICombatSessionObserver
    {
        private readonly CombatEngine combatEngine;
        private readonly List<Action<ICombatEvent>> subscribers;

        public ObservableCombatSession(string sessionId, CombatState state)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException("Session id is required.", nameof(sessionId));
            }

            SessionId = sessionId;
            State = state ?? throw new ArgumentNullException(nameof(state));
            combatEngine = new CombatEngine();
            subscribers = new List<Action<ICombatEvent>>();
        }

        public string SessionId { get; }

        public CombatState State { get; }

        public static ObservableCombatSession CreateDefault(string sessionId)
        {
            return new ObservableCombatSession(sessionId, CombatScenarioFactory.CreateInitialState());
        }

        public IDisposable Subscribe(Action<ICombatEvent> onEvent)
        {
            if (onEvent == null)
            {
                throw new ArgumentNullException(nameof(onEvent));
            }

            subscribers.Add(onEvent);
            return new Subscription(subscribers, onEvent);
        }

        public BridgedCombatSessionSnapshot CaptureSnapshot()
        {
            var units = State.UnitsById.Values
                .OrderBy(unit => unit.Id)
                .Select(unit => new BridgedUnitSnapshot(
                    unit.Id,
                    unit.Side,
                    unit.Position,
                    unit.CurrentHealth,
                    unit.LifeState))
                .ToList();

            return new BridgedCombatSessionSnapshot(SessionId, units, true);
        }

        public IReadOnlyList<ICombatEvent> StartCombat()
        {
            return Publish(combatEngine.StartCombat(State));
        }

        public CombatActionResult ResolveMove(int unitId, GridPosition destination)
        {
            var result = combatEngine.ResolveMove(State, unitId, destination);
            Publish(result.Events);
            return result;
        }

        public CombatActionResult ResolveAttack(int attackerUnitId, int targetUnitId)
        {
            var result = combatEngine.ResolveAttack(State, attackerUnitId, targetUnitId);
            Publish(result.Events);
            return result;
        }

        public IReadOnlyList<ICombatEvent> EndPlayerTurnAndRunEnemyTurn()
        {
            return Publish(combatEngine.EndPlayerTurnAndRunEnemyTurn(State));
        }

        private IReadOnlyList<ICombatEvent> Publish(IReadOnlyList<ICombatEvent> events)
        {
            foreach (var combatEvent in events)
            {
                foreach (var subscriber in subscribers.ToArray())
                {
                    subscriber(combatEvent);
                }
            }

            return events;
        }

        private sealed class Subscription : IDisposable
        {
            private readonly List<Action<ICombatEvent>> subscribers;
            private Action<ICombatEvent>? callback;

            public Subscription(List<Action<ICombatEvent>> subscribers, Action<ICombatEvent> callback)
            {
                this.subscribers = subscribers;
                this.callback = callback;
            }

            public void Dispose()
            {
                if (callback == null)
                {
                    return;
                }

                subscribers.Remove(callback);
                callback = null;
            }
        }
    }
}
