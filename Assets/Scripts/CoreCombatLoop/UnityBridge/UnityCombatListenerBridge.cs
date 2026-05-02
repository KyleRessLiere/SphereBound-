using System;
using System.Collections.Generic;
using System.IO;
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
        [SerializeField] private int debugActingUnitId = CombatScenarioFactory.PlayerUnitId;
        [SerializeField] private Vector2Int debugMoveDestination = new Vector2Int(1, 2);
        [SerializeField] private int debugAttackTargetUnitId = CombatScenarioFactory.EnemyUnitId;
        [SerializeField] private string lastActionCountLog = string.Empty;
        [SerializeField] private string lastBoardOutput = string.Empty;
        [SerializeField] private string lastAttackOverlayOutput = string.Empty;
        [SerializeField] private string lastFileOutputPath = string.Empty;
        [SerializeField] private string lastFileOutputConfigPath = string.Empty;
        [SerializeField] private List<UnityBridgedUnitDebugState> debugUnits = new List<UnityBridgedUnitDebugState>();

        private readonly Dictionary<int, GameObject> placeholderObjects = new Dictionary<int, GameObject>();
        private ICombatSessionObserver? sessionObserver;
        private ICombatDebugSession? debugSession;
        private ObservableCombatSession? ownedSession;
        private IDisposable? subscription;
        private readonly CombatDebugSurfacePresenter debugSurfacePresenter = new CombatDebugSurfacePresenter();
        private CombatDebugFileOutputWriter? fileOutputWriter;
        private string? fileOutputSessionId;

        public BridgedCombatSessionSnapshot? CurrentSnapshot { get; private set; }

        public IReadOnlyList<UnityBridgedUnitDebugState> DebugUnits => debugUnits;

        public string LastActionCountLog => lastActionCountLog;

        public string LastBoardOutput => lastBoardOutput;

        public string LastAttackOverlayOutput => lastAttackOverlayOutput;

        public string LastFileOutputPath => lastFileOutputPath;

        public string LastFileOutputConfigPath => lastFileOutputConfigPath;

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
            debugSession?.StartCombat();
            RefreshSnapshot();
        }

        [ContextMenu("Debug Move")]
        public void ExecuteDebugMove()
        {
            ExecuteDebugCommand(new CombatDebugCommandRequest(
                CombatDebugCommandType.Move,
                debugActingUnitId,
                new GridPosition(debugMoveDestination.x, debugMoveDestination.y),
                debugAttackTargetUnitId));
        }

        [ContextMenu("Debug Attack")]
        public void ExecuteDebugAttack()
        {
            ExecuteDebugCommand(new CombatDebugCommandRequest(
                CombatDebugCommandType.Attack,
                debugActingUnitId,
                new GridPosition(debugMoveDestination.x, debugMoveDestination.y),
                debugAttackTargetUnitId));
        }

        [ContextMenu("Debug End Turn")]
        public void ExecuteDebugEndTurn()
        {
            ExecuteDebugCommand(new CombatDebugCommandRequest(
                CombatDebugCommandType.EndTurn,
                debugActingUnitId,
                new GridPosition(debugMoveDestination.x, debugMoveDestination.y),
                debugAttackTargetUnitId));
        }

        [ContextMenu("Debug Restart Combat")]
        public void RestartObservedCombat()
        {
            InitializeDefaultSession();
            if (autoStartCombatOnEnable)
            {
                StartObservedCombat();
            }

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
            debugSession = observer as ICombatDebugSession;
            subscription = observer.Subscribe(HandleCoreEvent);
            RefreshSnapshot();
        }

        public void Detach()
        {
            DetachSubscription();
            sessionObserver = null;
            debugSession = null;
            ownedSession = null;
            CurrentSnapshot = null;
            lastActionCountLog = string.Empty;
            lastBoardOutput = string.Empty;
            lastAttackOverlayOutput = string.Empty;
            lastFileOutputPath = string.Empty;
            lastFileOutputConfigPath = string.Empty;
            fileOutputWriter = null;
            fileOutputSessionId = null;
            debugSurfacePresenter.Reset();
            debugUnits.Clear();
            ClearPlaceholderObjects();
        }

        private void HandleCoreEvent(ICombatEvent combatEvent)
        {
            var snapshotBefore = CurrentSnapshot;
            PublishLog("CoreEvent", ScenarioLogFormatter.Format(combatEvent));
            RefreshSnapshot();
            EmitDerivedDebugOutput(combatEvent, snapshotBefore);
        }

        private void ExecuteDebugCommand(CombatDebugCommandRequest request)
        {
            var result = CombatDebugCommandExecutor.Execute(debugSession, request);
            if (result.CommandFailureReason != CombatDebugCommandFailureReason.None)
            {
                PublishWarning("BridgeControl", $"command={request.CommandType} failed reason={result.CommandFailureReason}");
            }

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

        private void EmitDerivedDebugOutput(ICombatEvent combatEvent, BridgedCombatSessionSnapshot? snapshotBefore)
        {
            if (CurrentSnapshot == null || debugSession is not ObservableCombatSession observableSession)
            {
                return;
            }

            var logs = debugSurfacePresenter.HandleEvent(
                combatEvent,
                snapshotBefore,
                CurrentSnapshot,
                observableSession.State.RemainingPlayerActions);

            foreach (var log in logs)
            {
                switch (log.Category)
                {
                    case "Actions":
                        lastActionCountLog = log.Message;
                        PublishLog("Actions", log.Message);
                        break;
                    case "Board":
                        lastBoardOutput = log.Message;
                        PublishLog("Board", log.Message);
                        break;
                    case "AttackBoard":
                        lastAttackOverlayOutput = log.Message;
                        PublishLog("AttackBoard", log.Message);
                        break;
                }
            }
        }

        private void PublishLog(string category, string message)
        {
            Debug.Log($"[{category}] {message}", this);
            WriteFileOutput(category, message);
        }

        private void PublishWarning(string category, string message)
        {
            Debug.LogWarning($"[{category}] {message}", this);
            WriteFileOutput(category, message);
        }

        private void WriteFileOutput(string category, string message)
        {
            if (sessionObserver == null)
            {
                return;
            }

            var projectRoot = ResolveProjectRoot();
            if (string.IsNullOrEmpty(projectRoot))
            {
                return;
            }

            var config = CombatDebugFileOutputConfigLoader.LoadOrCreate(projectRoot);
            lastFileOutputConfigPath = CombatDebugFileOutputConfigLoader.GetConfigPath(projectRoot);

            if (!config.Enabled)
            {
                fileOutputWriter = null;
                fileOutputSessionId = null;
                lastFileOutputPath = string.Empty;
                return;
            }

            if (fileOutputWriter == null
                || fileOutputSessionId != sessionObserver.SessionId
                || !string.Equals(fileOutputWriter.RootDirectory, Path.Combine(projectRoot, config.OutputRootRelativePath), StringComparison.Ordinal))
            {
                fileOutputWriter = CombatDebugFileOutputWriter.Create(projectRoot, config, sessionObserver.SessionId);
                fileOutputSessionId = sessionObserver.SessionId;
                lastFileOutputPath = fileOutputWriter.FilePath;
            }

            fileOutputWriter.Append(category, message);
        }

        private static string? ResolveProjectRoot()
        {
            var assetsDirectory = Application.dataPath;
            if (string.IsNullOrWhiteSpace(assetsDirectory))
            {
                return null;
            }

            var directoryInfo = Directory.GetParent(assetsDirectory);
            return directoryInfo?.FullName;
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
