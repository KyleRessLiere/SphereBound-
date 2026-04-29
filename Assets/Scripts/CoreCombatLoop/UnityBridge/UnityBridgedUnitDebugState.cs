using System;
using UnityEngine;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    [Serializable]
    public sealed class UnityBridgedUnitDebugState
    {
        [SerializeField] private int unitId;
        [SerializeField] private string side = string.Empty;
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private int currentHealth;
        [SerializeField] private bool isAlive;

        public int UnitId
        {
            get => unitId;
            set => unitId = value;
        }

        public string Side
        {
            get => side;
            set => side = value;
        }

        public Vector2Int GridPosition
        {
            get => gridPosition;
            set => gridPosition = value;
        }

        public int CurrentHealth
        {
            get => currentHealth;
            set => currentHealth = value;
        }

        public bool IsAlive
        {
            get => isAlive;
            set => isAlive = value;
        }
    }
}
