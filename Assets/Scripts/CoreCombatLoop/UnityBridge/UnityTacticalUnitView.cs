using Spherebound.CoreCombatLoop.Core;
using UnityEngine;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class UnityTacticalUnitView : MonoBehaviour
    {
        [SerializeField] private int unitId;
        [SerializeField] private CombatUnitSide side;
        [SerializeField] private Renderer unitRenderer = null!;

        public int UnitId => unitId;

        public void Initialize(int id, CombatUnitSide unitSide, Renderer rendererComponent)
        {
            unitId = id;
            side = unitSide;
            unitRenderer = rendererComponent;
            if (unitRenderer != null)
            {
                unitRenderer.material.color = unitSide == CombatUnitSide.Player
                    ? new Color(0.2f, 0.55f, 0.95f, 1f)
                    : new Color(0.85f, 0.3f, 0.3f, 1f);
            }
        }

        public void SyncVisible(bool isVisible, Vector3 worldPosition)
        {
            gameObject.SetActive(isVisible);
            if (isVisible)
            {
                transform.position = worldPosition;
            }
        }
    }
}
