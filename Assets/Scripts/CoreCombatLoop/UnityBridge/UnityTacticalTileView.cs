using Spherebound.CoreCombatLoop.Core;
using UnityEngine;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class UnityTacticalTileView : MonoBehaviour
    {
        private static readonly Color IdleColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        private static readonly Color SelectedColor = new Color(0.85f, 0.85f, 0.95f, 1f);
        private static readonly Color MoveColor = new Color(0.95f, 0.85f, 0.25f, 1f);
        private static readonly Color AttackPreviewColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        private static readonly Color InvalidColor = new Color(0.95f, 0.45f, 0.75f, 1f);

        [SerializeField] private Renderer tileRenderer = null!;
        [SerializeField] private GridPosition gridPosition;

        public GridPosition GridPosition => gridPosition;

        public void Initialize(GridPosition position, Renderer rendererComponent)
        {
            gridPosition = position;
            tileRenderer = rendererComponent;
            ApplyHighlight(UnityTacticalTileHighlightState.Idle);
        }

        public void ApplyHighlight(UnityTacticalTileHighlightState highlightState)
        {
            if (tileRenderer == null)
            {
                return;
            }

            tileRenderer.material.color = highlightState switch
            {
                UnityTacticalTileHighlightState.Selected => SelectedColor,
                UnityTacticalTileHighlightState.MoveAvailable => MoveColor,
                UnityTacticalTileHighlightState.AttackPreview => AttackPreviewColor,
                UnityTacticalTileHighlightState.Invalid => InvalidColor,
                _ => IdleColor,
            };
        }
    }
}
