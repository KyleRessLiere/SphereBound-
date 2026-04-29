using System;
using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;
using Spherebound.CoreCombatLoop.Scenarios;
using UnityEngine;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class UnityCombatListenerBridge : MonoBehaviour
    {
        [SerializeField] private bool initializeDefaultSessionOnEnable = true;
        [SerializeField] private bool autoStartCombatOnEnable = true;
        [SerializeField] private bool createPlaceholderObjects;
        [SerializeField] private List<UnityBridgedUnitDebugState> debugUnits = new List<UnityBridgedUnitDebugState>();

        private readonly Dictionary<int, GameObject> placeholderObjects = new Dictionary<int, GameObject>();
        private ICombatSessionObserver? sessionObserver;
        private ObservableCombatSession? ownedSession;
        private IDisposable? subscription;

        public BridgedCombatSessionSnapshot? CurrentSnapshot { get; private set; }

        public IReadOnlyList<UnityBridgedUnitDebugState> DebugUnits => debugUnits;

        public void OnEnable()
        {
            if (!initializeDefaultSessionOnEnable)
            {
                return;
            }

            InitializeDefaultSession();
            if (autoStartCombatOnEnable)
            {
                StartObservedCombat();
            }
        }

        public void OnDisable()
        {
            Detach();
        }

        [ContextMenu("Initialize Default Session")]
        public void InitializeDefaultSession()
        {
            var session = ObservableCombatSession.CreateDefault($"unity-session-{GetInstanceID()}");
            ownedSession = session;
            Attach(session);
        }

        [ContextMenu("Start Observed Combat")]
        public void StartObservedCombat()
        {
            ownedSession?.StartCombat();
            RefreshSnapshot();
        }

        public void Attach(ICombatSessionObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            DetachSubscription();
            sessionObserver = observer;
            subscription = observer.Subscribe(HandleCoreEvent);
            RefreshSnapshot();
        }

        public void Detach()
        {
            DetachSubscription();
            sessionObserver = null;
            ownedSession = null;
            CurrentSnapshot = null;
            debugUnits.Clear();
            ClearPlaceholderObjects();
        }

        private void HandleCoreEvent(ICombatEvent combatEvent)
        {
            Debug.Log($"[CoreEvent] {ScenarioLogFormatter.Format(combatEvent)}", this);
            RefreshSnapshot();
        }

        private void RefreshSnapshot()
        {
            if (sessionObserver == null)
            {
                return;
            }

            CurrentSnapshot = sessionObserver.CaptureSnapshot();
            UpdateDebugUnits(CurrentSnapshot);

            if (createPlaceholderObjects)
            {
                SyncPlaceholderObjects(CurrentSnapshot);
            }
        }

        private void UpdateDebugUnits(BridgedCombatSessionSnapshot snapshot)
        {
            debugUnits.Clear();
            foreach (var unit in snapshot.Units)
            {
                debugUnits.Add(new UnityBridgedUnitDebugState
                {
                    UnitId = unit.UnitId,
                    Side = unit.Side.ToString(),
                    GridPosition = new Vector2Int(unit.Position.X, unit.Position.Y),
                    CurrentHealth = unit.CurrentHealth,
                    IsAlive = unit.LifeState == UnitLifeState.Alive,
                });
            }
        }

        private void SyncPlaceholderObjects(BridgedCombatSessionSnapshot snapshot)
        {
            var activeUnitIds = new HashSet<int>();

            foreach (var unit in snapshot.Units)
            {
                activeUnitIds.Add(unit.UnitId);

                if (!placeholderObjects.TryGetValue(unit.UnitId, out var placeholder))
                {
                    placeholder = new GameObject($"DebugUnit_{unit.UnitId}_{unit.Side}");
                    placeholder.transform.SetParent(transform, false);
                    placeholderObjects.Add(unit.UnitId, placeholder);
                }

                placeholder.transform.position = new Vector3(unit.Position.X, 0f, unit.Position.Y);
                placeholder.SetActive(unit.LifeState == UnitLifeState.Alive);
            }

            var orphanedIds = new List<int>();
            foreach (var entry in placeholderObjects)
            {
                if (!activeUnitIds.Contains(entry.Key))
                {
                    orphanedIds.Add(entry.Key);
                }
            }

            foreach (var orphanedId in orphanedIds)
            {
                if (placeholderObjects.TryGetValue(orphanedId, out var placeholder))
                {
                    Destroy(placeholder);
                }

                placeholderObjects.Remove(orphanedId);
            }
        }

        private void ClearPlaceholderObjects()
        {
            foreach (var entry in placeholderObjects)
            {
                if (entry.Value != null)
                {
                    Destroy(entry.Value);
                }
            }

            placeholderObjects.Clear();
        }

        private void DetachSubscription()
        {
            subscription?.Dispose();
            subscription = null;
        }
    }
}
