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

        public event Action? RuntimeStateChanged;
        public event Action<ICombatEvent>? CoreEventObserved;

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
            ExecuteDebugCommand(CombatDebugCommandRequest.Move(
                debugActingUnitId,
                new GridPosition(debugMoveDestination.x, debugMoveDestination.y)));
        }

        [ContextMenu("Debug Attack")]
        public void ExecuteDebugAttack()
        {
            ExecuteDebugCommand(CombatDebugCommandRequest.Attack(
                debugActingUnitId,
                debugAttackTargetUnitId));
        }

        [ContextMenu("Debug End Turn")]
        public void ExecuteDebugEndTurn()
        {
            ExecuteDebugCommand(CombatDebugCommandRequest.EndTurn(debugActingUnitId));
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
            RuntimeStateChanged?.Invoke();
        }

        public CombatRuntimeControlSurfaceModel BuildRuntimeControlSurfaceModel()
        {
            if (debugSession is ObservableCombatSession observableSession)
            {
                return CombatRuntimeControlSurfaceBuilder.Build(observableSession.State);
            }

            return new CombatRuntimeControlSurfaceModel(
                canMove: false,
                canEndTurn: false,
                remainingPlayerActions: 0,
                abilityButtons: Array.Empty<CombatRuntimeAbilityButtonModel>());
        }

        public CombatDebugCommandResult ExecuteRuntimeMove(CombatRuntimeDirection direction)
        {
            if (!TryGetPlayerGridPosition(out var playerPosition))
            {
                return new CombatDebugCommandResult(
                    false,
                    CombatDebugCommandFailureReason.SessionUnavailable,
                    Array.Empty<ICombatEvent>(),
                    CombatFailureReason.None);
            }

            var destination = direction switch
            {
                CombatRuntimeDirection.Up => new GridPosition(playerPosition.X, playerPosition.Y + 1),
                CombatRuntimeDirection.Down => new GridPosition(playerPosition.X, playerPosition.Y - 1),
                CombatRuntimeDirection.Left => new GridPosition(playerPosition.X - 1, playerPosition.Y),
                _ => new GridPosition(playerPosition.X + 1, playerPosition.Y),
            };

            return ExecuteDebugCommand(CombatDebugCommandRequest.Move(CombatScenarioFactory.PlayerUnitId, destination));
        }

        public CombatDebugCommandResult ExecuteRuntimeAbility(CombatRuntimeAbilityButtonModel abilityButton)
        {
            return ExecuteDebugCommand(CombatDebugCommandRequest.Ability(
                abilityButton.ActingUnitId,
                abilityButton.AbilityId,
                abilityButton.TargetUnitId,
                abilityButton.TargetPosition));
        }

        public CombatDebugCommandResult ExecuteRuntimeAbilityPreview(CombatRuntimeAbilityPreview preview)
        {
            if (preview == null)
            {
                throw new ArgumentNullException(nameof(preview));
            }

            return ExecuteDebugCommand(CombatDebugCommandRequest.Ability(
                preview.Request.ActorUnitId,
                preview.Request.AbilityId,
                preview.Request.TargetUnitId,
                preview.Request.TargetPosition));
        }

        public CombatDebugCommandResult ExecuteRuntimeEndTurn()
        {
            return ExecuteDebugCommand(CombatDebugCommandRequest.EndTurn(CombatScenarioFactory.PlayerUnitId));
        }

        public CombatDebugCommandResult ExecuteRuntimeMoveTo(GridPosition destination)
        {
            return ExecuteDebugCommand(CombatDebugCommandRequest.Move(CombatScenarioFactory.PlayerUnitId, destination));
        }

        public IReadOnlyList<GridPosition> BuildRuntimeValidMoveTiles()
        {
            if (debugSession is not ObservableCombatSession observableSession)
            {
                return Array.Empty<GridPosition>();
            }

            if (!observableSession.State.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player)
                || !player.IsAlive)
            {
                return Array.Empty<GridPosition>();
            }

            var moveTiles = new List<GridPosition>(4);
            var candidates = new[]
            {
                new GridPosition(player.Position.X, player.Position.Y + 1),
                new GridPosition(player.Position.X, player.Position.Y - 1),
                new GridPosition(player.Position.X - 1, player.Position.Y),
                new GridPosition(player.Position.X + 1, player.Position.Y),
            };

            for (var index = 0; index < candidates.Length; index += 1)
            {
                var candidate = candidates[index];
                if (!observableSession.State.Board.Contains(candidate))
                {
                    continue;
                }

                if (observableSession.State.TryGetUnitAtPosition(candidate, out _))
                {
                    continue;
                }

                moveTiles.Add(candidate);
            }

            return moveTiles;
        }

        public CombatRuntimeAbilityPreview? BuildRuntimeAbilityPreview(CombatRuntimeAbilityButtonModel abilityButton, GridPosition targetTile)
        {
            if (abilityButton == null)
            {
                throw new ArgumentNullException(nameof(abilityButton));
            }

            if (debugSession is not ObservableCombatSession observableSession)
            {
                return null;
            }

            if (!observableSession.State.TryGetUnit(abilityButton.ActingUnitId, out var actor)
                || !actor.IsAlive
                || !actor.Definition.TryGetAbility(abilityButton.AbilityId, out var ability))
            {
                return null;
            }

            var request = ResolveRuntimeAbilityRequest(observableSession.State, actor, ability, targetTile);
            var resolvedTiles = CombatAbilityResolver.ResolveAffectedTiles(
                observableSession.State,
                actor,
                ability,
                request,
                out var failureReason);

            var positions = BuildPreviewHighlightTiles(observableSession.State, actor, ability, request, targetTile, resolvedTiles, failureReason);
            if (positions.Count == 0)
            {
                return null;
            }

            return new CombatRuntimeAbilityPreview(abilityButton, request, targetTile, positions);
        }

        public CombatRuntimeAbilityPreview? BuildRuntimeAbilityPreview(CombatRuntimeAbilityButtonModel abilityButton)
        {
            if (abilityButton == null)
            {
                throw new ArgumentNullException(nameof(abilityButton));
            }

            if (debugSession is not ObservableCombatSession observableSession)
            {
                return null;
            }

            if (!observableSession.State.TryGetUnit(abilityButton.ActingUnitId, out var actor)
                || !actor.IsAlive
                || !actor.Definition.TryGetAbility(abilityButton.AbilityId, out var ability))
            {
                return null;
            }

            var request = CombatRuntimeAbilityRequestResolver.CreateRuntimeRequest(observableSession.State, actor, ability);
            var resolvedTiles = CombatAbilityResolver.ResolveAffectedTiles(
                observableSession.State,
                actor,
                ability,
                request,
                out var failureReason);

            var targetTile = request.TargetPosition
                ?? (request.TargetUnitId.HasValue && observableSession.State.TryGetUnit(request.TargetUnitId.Value, out var targetUnit)
                    ? targetUnit.Position
                    : CombatRuntimeAbilityRequestResolver.ResolveForwardTargetPosition(observableSession.State, actor));

            var positions = BuildPreviewHighlightTiles(observableSession.State, actor, ability, request, targetTile, resolvedTiles, failureReason);
            if (positions.Count == 0)
            {
                return null;
            }

            return new CombatRuntimeAbilityPreview(abilityButton, request, targetTile, positions);
        }

        private void HandleCoreEvent(ICombatEvent combatEvent)
        {
            var snapshotBefore = CurrentSnapshot;
            PublishLog("CoreEvent", ScenarioLogFormatter.Format(combatEvent));
            CoreEventObserved?.Invoke(combatEvent);
            RefreshSnapshot();
            EmitDerivedDebugOutput(combatEvent, snapshotBefore);
        }

        private CombatDebugCommandResult ExecuteDebugCommand(CombatDebugCommandRequest request)
        {
            var result = CombatDebugCommandExecutor.Execute(debugSession, request);
            if (result.CommandFailureReason != CombatDebugCommandFailureReason.None)
            {
                PublishWarning("BridgeControl", $"command={request.CommandType} failed reason={result.CommandFailureReason}");
            }

            RefreshSnapshot();
            return result;
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

            RuntimeStateChanged?.Invoke();
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

        private bool TryGetPlayerGridPosition(out GridPosition playerPosition)
        {
            if (CurrentSnapshot != null)
            {
                foreach (var unit in CurrentSnapshot.Units)
                {
                    if (unit.Side == CombatUnitSide.Player && unit.LifeState == UnitLifeState.Alive)
                    {
                        playerPosition = unit.Position;
                        return true;
                    }
                }
            }

            playerPosition = default;
            return false;
        }

        private static AbilityUseRequest ResolveRuntimeAbilityRequest(
            CombatState state,
            CombatUnitState actor,
            AbilityDefinition ability,
            GridPosition selectedTile)
        {
            switch (ability.TargetingMode)
            {
                case AbilityTargetingMode.Self:
                    return new AbilityUseRequest(actor.Id, ability.Id);

                case AbilityTargetingMode.AdjacentUnit:
                    if (state.TryGetUnitAtPosition(selectedTile, out var targetUnit))
                    {
                        return new AbilityUseRequest(actor.Id, ability.Id, targetUnit.Id, selectedTile);
                    }

                    return new AbilityUseRequest(actor.Id, ability.Id, targetPosition: selectedTile);

                case AbilityTargetingMode.SpecificTile:
                case AbilityTargetingMode.Area:
                case AbilityTargetingMode.AllUnitsInAffectedShape:
                    return new AbilityUseRequest(actor.Id, ability.Id, targetPosition: selectedTile);

                case AbilityTargetingMode.DirectionalShape:
                case AbilityTargetingMode.Line:
                    return new AbilityUseRequest(
                        actor.Id,
                        ability.Id,
                        targetPosition: CombatRuntimeAbilityRequestResolver.ResolveForwardTargetPosition(state, actor));

                default:
                    return CombatRuntimeAbilityRequestResolver.CreateRuntimeRequest(state, actor, ability);
            }
        }

        private static IReadOnlyList<GridPosition> BuildPreviewHighlightTiles(
            CombatState state,
            CombatUnitState actor,
            AbilityDefinition ability,
            AbilityUseRequest request,
            GridPosition fallbackTargetTile,
            IReadOnlyList<ResolvedAbilityTile> resolvedTiles,
            CombatFailureReason failureReason)
        {
            if (failureReason == CombatFailureReason.None)
            {
                var resolvedPositions = new List<GridPosition>(resolvedTiles.Count);
                for (var index = 0; index < resolvedTiles.Count; index += 1)
                {
                    resolvedPositions.Add(resolvedTiles[index].Position);
                }

                return resolvedPositions;
            }

            if (!TryResolvePreviewAnchorPosition(state, actor, ability, request, fallbackTargetTile, out var anchorPosition))
            {
                return Array.Empty<GridPosition>();
            }

            var previewPositions = new List<GridPosition>();
            var offsets = ability.TilePattern.Offsets;
            for (var index = 0; index < offsets.Count; index += 1)
            {
                var translatedPosition = TranslatePreviewOffset(actor.Position, anchorPosition, ability.TilePattern.Anchor, offsets[index]);
                if (!state.Board.Contains(translatedPosition))
                {
                    continue;
                }

                previewPositions.Add(translatedPosition);
            }

            return previewPositions;
        }

        private static bool TryResolvePreviewAnchorPosition(
            CombatState state,
            CombatUnitState actor,
            AbilityDefinition ability,
            AbilityUseRequest request,
            GridPosition fallbackTargetTile,
            out GridPosition anchorPosition)
        {
            switch (ability.TargetingMode)
            {
                case AbilityTargetingMode.Self:
                    anchorPosition = actor.Position;
                    return true;

                case AbilityTargetingMode.AdjacentUnit:
                    if (request.TargetUnitId.HasValue && state.TryGetUnit(request.TargetUnitId.Value, out var targetUnit))
                    {
                        anchorPosition = targetUnit.Position;
                        return true;
                    }

                    anchorPosition = request.TargetPosition ?? fallbackTargetTile;
                    return state.Board.Contains(anchorPosition);

                case AbilityTargetingMode.SpecificTile:
                case AbilityTargetingMode.DirectionalShape:
                case AbilityTargetingMode.Line:
                case AbilityTargetingMode.Area:
                case AbilityTargetingMode.AllUnitsInAffectedShape:
                    anchorPosition = request.TargetPosition ?? fallbackTargetTile;
                    return state.Board.Contains(anchorPosition);

                default:
                    anchorPosition = fallbackTargetTile;
                    return state.Board.Contains(anchorPosition);
            }
        }

        private static GridPosition TranslatePreviewOffset(
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
                    var normalizedDirection = NormalizePreviewDirection(actorPosition, anchorPosition);
                    var rotatedOffset = RotatePreviewOffset(offset, normalizedDirection);
                    return new GridPosition(actorPosition.X + rotatedOffset.X, actorPosition.Y + rotatedOffset.Y);

                default:
                    return new GridPosition(anchorPosition.X + offset.X, anchorPosition.Y + offset.Y);
            }
        }

        private static GridOffset NormalizePreviewDirection(GridPosition actorPosition, GridPosition anchorPosition)
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

        private static GridOffset RotatePreviewOffset(GridOffset offset, GridOffset direction)
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
