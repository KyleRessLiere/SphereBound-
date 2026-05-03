using System;
using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class UnityCombatTacticalBoardView : MonoBehaviour
    {
        [SerializeField] private UnityCombatListenerBridge bridge = null!;
        [SerializeField] private UnityCombatRuntimeUiController runtimeUiController = null!;
        [SerializeField] private Camera boardCamera = null!;
        [SerializeField] private Transform tileRoot = null!;
        [SerializeField] private Transform unitRoot = null!;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private float tileGap = 0.1f;
        [SerializeField] private float sideGap = 0.3f;
        [SerializeField] private float tileHeight = 0.12f;
        [SerializeField] private float unitHeightOffset = 0.42f;

        private readonly Dictionary<GridPosition, UnityTacticalTileView> tileViews = new Dictionary<GridPosition, UnityTacticalTileView>();
        private readonly Dictionary<int, UnityTacticalUnitView> unitViews = new Dictionary<int, UnityTacticalUnitView>();
        private readonly HashSet<GridPosition> moveHighlights = new HashSet<GridPosition>();
        private readonly HashSet<GridPosition> previewHighlights = new HashSet<GridPosition>();
        private bool isMoveModeArmed;
        private GridPosition? selectedTile;
        private GridPosition? invalidTile;
        private CombatRuntimeAbilityButtonModel? armedAbility;
        private CombatRuntimeAbilityPreview? currentPreview;
        private BoardDimensions currentBoard;

        private void OnEnable()
        {
            if (bridge != null)
            {
                bridge.RuntimeStateChanged += RefreshBoard;
                bridge.CoreEventObserved += HandleCoreEvent;
            }

            if (runtimeUiController != null)
            {
                runtimeUiController.MoveActivationRequested += ArmMoveMode;
                runtimeUiController.AbilityActivationRequested += ArmAbilityPreview;
            }

            RefreshBoard();
        }

        private void OnDisable()
        {
            if (bridge != null)
            {
                bridge.RuntimeStateChanged -= RefreshBoard;
                bridge.CoreEventObserved -= HandleCoreEvent;
            }

            if (runtimeUiController != null)
            {
                runtimeUiController.MoveActivationRequested -= ArmMoveMode;
                runtimeUiController.AbilityActivationRequested -= ArmAbilityPreview;
            }
        }

        private void Update()
        {
            if (WasCancelPressed())
            {
                CancelInteractionMode();
            }

            if (WasPrimaryPointerPressed())
            {
                HandlePointerClick();
            }
        }

        public void Configure(UnityCombatListenerBridge bridgeReference, UnityCombatRuntimeUiController runtimeUiReference, Camera cameraReference)
        {
            bridge = bridgeReference;
            runtimeUiController = runtimeUiReference;
            boardCamera = cameraReference;
        }

        private void RefreshBoard()
        {
            var snapshot = bridge != null ? bridge.CurrentSnapshot : null;
            if (snapshot == null || !snapshot.IsConnected)
            {
                return;
            }

            EnsureRoots();
            currentBoard = snapshot.Board;
            EnsureTiles(snapshot.Board);
            SyncUnits(snapshot);
            RebuildMoveHighlights();
            SyncInteractionStateWithControlSurface();
            if (currentPreview != null)
            {
                currentPreview = bridge.BuildRuntimeAbilityPreview(currentPreview.AbilityButton, currentPreview.TargetTile);
                if (currentPreview == null)
                {
                    armedAbility = null;
                    previewHighlights.Clear();
                }
                else
                {
                    CopyPreviewHighlights(currentPreview);
                }
            }

            ApplyTileHighlights();
        }

        private void HandleCoreEvent(ICombatEvent combatEvent)
        {
            if (combatEvent is ActionFailed)
            {
                invalidTile = selectedTile;
            }
            else if (combatEvent is UnitMoved or UnitRemoved or UnitDeath or ActionEnded)
            {
                invalidTile = null;
                if (combatEvent is not ActionFailed)
                {
                    if (currentPreview != null)
                    {
                        CancelPreview();
                    }
                }
            }

            ApplyTileHighlights();
        }

        private void ArmAbilityPreview(CombatRuntimeAbilityButtonModel abilityButton)
        {
            if (armedAbility != null
                && currentPreview != null
                && armedAbility.AbilityId == abilityButton.AbilityId
                && armedAbility.ActingUnitId == abilityButton.ActingUnitId)
            {
                bridge.ExecuteRuntimeAbilityPreview(currentPreview);
                CancelInteractionMode();
                return;
            }

            isMoveModeArmed = false;
            armedAbility = abilityButton;
            invalidTile = null;
            currentPreview = bridge.BuildRuntimeAbilityPreview(abilityButton);
            previewHighlights.Clear();
            if (currentPreview != null)
            {
                CopyPreviewHighlights(currentPreview);
            }

            ApplyTileHighlights();
        }

        private void HandlePointerClick()
        {
            if (boardCamera == null)
            {
                return;
            }

            var controlSurface = bridge != null
                ? bridge.BuildRuntimeControlSurfaceModel()
                : null;

            var screenPosition = ReadPointerScreenPosition();
            if (screenPosition == null)
            {
                return;
            }

            var ray = boardCamera.ScreenPointToRay(screenPosition.Value);
            if (!Physics.Raycast(ray, out var hit))
            {
                if (armedAbility != null || currentPreview != null || isMoveModeArmed)
                {
                    CancelInteractionMode();
                }
                else
                {
                    selectedTile = null;
                    isMoveModeArmed = false;
                    ApplyTileHighlights();
                }

                return;
            }

            var tileView = hit.collider.GetComponentInParent<UnityTacticalTileView>();
            if (tileView == null)
            {
                if (armedAbility != null || currentPreview != null || isMoveModeArmed)
                {
                    CancelInteractionMode();
                }

                return;
            }

            var clickedTile = tileView.GridPosition;
            selectedTile = clickedTile;
            invalidTile = null;

            if (armedAbility != null && currentPreview != null)
            {
                if (IsAbilityStillInteractable(controlSurface, armedAbility))
                {
                    bridge.ExecuteRuntimeAbilityPreview(currentPreview);
                    CancelInteractionMode();
                }
                else
                {
                    CancelPreview();
                }

                return;
            }

            if (armedAbility != null)
            {
                invalidTile = clickedTile;
                ApplyTileHighlights();
                return;
            }

            if (isMoveModeArmed && moveHighlights.Contains(clickedTile) && controlSurface != null && controlSurface.CanMove)
            {
                bridge.ExecuteRuntimeMoveTo(clickedTile);
                isMoveModeArmed = false;
            }

            ApplyTileHighlights();
        }

        private void ArmMoveMode()
        {
            if (bridge != null && !bridge.BuildRuntimeControlSurfaceModel().CanMove)
            {
                return;
            }

            isMoveModeArmed = true;
            armedAbility = null;
            currentPreview = null;
            previewHighlights.Clear();
            invalidTile = null;
            ApplyTileHighlights();
        }

        private static bool WasCancelPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }

        private static bool WasPrimaryPointerPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }

        private static Vector2? ReadPointerScreenPosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null)
            {
                return null;
            }

            return Mouse.current.position.ReadValue();
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;
#else
            return null;
#endif
        }

        private void CopyPreviewHighlights(CombatRuntimeAbilityPreview preview)
        {
            previewHighlights.Clear();
            for (var index = 0; index < preview.HighlightTiles.Count; index += 1)
            {
                previewHighlights.Add(preview.HighlightTiles[index]);
            }
        }

        private void CancelPreview()
        {
            armedAbility = null;
            currentPreview = null;
            previewHighlights.Clear();
            invalidTile = null;
            ApplyTileHighlights();
        }

        private void CancelInteractionMode()
        {
            isMoveModeArmed = false;
            CancelPreview();
        }

        private void SyncInteractionStateWithControlSurface()
        {
            if (bridge == null)
            {
                return;
            }

            var controlSurface = bridge.BuildRuntimeControlSurfaceModel();
            if (!controlSurface.CanMove)
            {
                isMoveModeArmed = false;
            }

            if (armedAbility == null)
            {
                return;
            }

            var matchingAbility = FindMatchingAbility(controlSurface, armedAbility);
            if (matchingAbility == null || !matchingAbility.IsInteractable)
            {
                CancelPreview();
                return;
            }

            armedAbility = matchingAbility;
        }

        private static CombatRuntimeAbilityButtonModel? FindMatchingAbility(
            CombatRuntimeControlSurfaceModel controlSurface,
            CombatRuntimeAbilityButtonModel abilityButton)
        {
            for (var index = 0; index < controlSurface.AbilityButtons.Count; index += 1)
            {
                var candidate = controlSurface.AbilityButtons[index];
                if (candidate.AbilityId == abilityButton.AbilityId
                    && candidate.ActingUnitId == abilityButton.ActingUnitId)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool IsAbilityStillInteractable(
            CombatRuntimeControlSurfaceModel? controlSurface,
            CombatRuntimeAbilityButtonModel abilityButton)
        {
            if (controlSurface == null)
            {
                return false;
            }

            var matchingAbility = FindMatchingAbility(controlSurface, abilityButton);
            return matchingAbility != null && matchingAbility.IsInteractable;
        }

        private void EnsureRoots()
        {
            if (tileRoot == null)
            {
                var tileRootObject = new GameObject("TacticalBoardTiles");
                tileRootObject.transform.SetParent(transform, false);
                tileRoot = tileRootObject.transform;
            }

            if (unitRoot == null)
            {
                var unitRootObject = new GameObject("TacticalBoardUnits");
                unitRootObject.transform.SetParent(transform, false);
                unitRoot = unitRootObject.transform;
            }
        }

        private void EnsureTiles(BoardDimensions board)
        {
            for (var x = 0; x < board.Width; x += 1)
            {
                for (var y = 0; y < board.Height; y += 1)
                {
                    var gridPosition = new GridPosition(x, y);
                    if (tileViews.ContainsKey(gridPosition))
                    {
                        continue;
                    }

                    var tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tileObject.name = $"Tile_{x}_{y}";
                    tileObject.transform.SetParent(tileRoot, false);
                    tileObject.transform.position = ToWorldPosition(gridPosition, tileHeight * 0.5f);
                    tileObject.transform.localScale = new Vector3(tileSize, tileHeight, tileSize);
                    var tileView = tileObject.AddComponent<UnityTacticalTileView>();
                    tileView.Initialize(gridPosition, tileObject.GetComponent<Renderer>());
                    tileViews.Add(gridPosition, tileView);
                }
            }
        }

        private void SyncUnits(BridgedCombatSessionSnapshot snapshot)
        {
            var aliveUnitIds = new HashSet<int>();
            for (var index = 0; index < snapshot.Units.Count; index += 1)
            {
                var unit = snapshot.Units[index];
                if (unit.LifeState != UnitLifeState.Alive)
                {
                    continue;
                }

                aliveUnitIds.Add(unit.UnitId);
                if (!unitViews.TryGetValue(unit.UnitId, out var unitView))
                {
                    unitView = CreateUnitView(unit);
                    unitViews.Add(unit.UnitId, unitView);
                }

                unitView.SyncVisible(true, ToWorldPosition(unit.Position, unitHeightOffset));
            }

            foreach (var entry in unitViews)
            {
                if (!aliveUnitIds.Contains(entry.Key))
                {
                    entry.Value.SyncVisible(false, entry.Value.transform.position);
                }
            }
        }

        private UnityTacticalUnitView CreateUnitView(BridgedUnitSnapshot unit)
        {
            var primitiveType = unit.Side == CombatUnitSide.Player ? PrimitiveType.Sphere : PrimitiveType.Cube;
            var unitObject = GameObject.CreatePrimitive(primitiveType);
            unitObject.name = unit.Side == CombatUnitSide.Player ? "PlayerUnit" : $"EnemyUnit_{unit.UnitId}";
            unitObject.transform.SetParent(unitRoot, false);
            unitObject.transform.localScale = unit.Side == CombatUnitSide.Player
                ? new Vector3(tileSize * 0.55f, tileSize * 0.55f, tileSize * 0.55f)
                : new Vector3(tileSize * 0.5f, tileSize * 0.5f, tileSize * 0.5f);
            var unitView = unitObject.AddComponent<UnityTacticalUnitView>();
            unitView.Initialize(unit.UnitId, unit.Side, unitObject.GetComponent<Renderer>());
            return unitView;
        }

        private void RebuildMoveHighlights()
        {
            moveHighlights.Clear();
            var positions = bridge.BuildRuntimeValidMoveTiles();
            for (var index = 0; index < positions.Count; index += 1)
            {
                moveHighlights.Add(positions[index]);
            }
        }

        private void ApplyTileHighlights()
        {
            foreach (var entry in tileViews)
            {
                var gridPosition = entry.Key;
                var state = UnityTacticalTileHighlightState.Idle;

                if (isMoveModeArmed && moveHighlights.Contains(gridPosition))
                {
                    state = UnityTacticalTileHighlightState.MoveAvailable;
                }

                if (selectedTile.HasValue && selectedTile.Value.Equals(gridPosition))
                {
                    state = UnityTacticalTileHighlightState.Selected;
                }

                if (previewHighlights.Contains(gridPosition))
                {
                    state = UnityTacticalTileHighlightState.AttackPreview;
                }

                if (invalidTile.HasValue && invalidTile.Value.Equals(gridPosition))
                {
                    state = UnityTacticalTileHighlightState.Invalid;
                }

                entry.Value.ApplyHighlight(state);
            }
        }

        private Vector3 ToWorldPosition(GridPosition gridPosition, float yOffset)
        {
            var step = tileSize + tileGap;
            var halfWidth = (currentBoard.Width - 1) * step * 0.5f;
            var boardDepth = ((currentBoard.Height - 1) * step) + sideGap;
            var halfDepth = boardDepth * 0.5f;
            var splitIndex = currentBoard.Height / 2;
            var zPosition = gridPosition.Y * step;
            if (gridPosition.Y >= splitIndex)
            {
                zPosition += sideGap;
            }

            return new Vector3(
                (gridPosition.X * step) - halfWidth,
                yOffset,
                zPosition - halfDepth);
        }
    }
}
