using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class BridgedCombatSessionSnapshot
    {
        public BridgedCombatSessionSnapshot(
            string sessionId,
            BoardDimensions board,
            IReadOnlyList<BridgedUnitSnapshot> units,
            bool isConnected)
        {
            SessionId = sessionId;
            Board = board;
            Units = units;
            IsConnected = isConnected;
        }

        public string SessionId { get; }

        public BoardDimensions Board { get; }

        public IReadOnlyList<BridgedUnitSnapshot> Units { get; }

        public bool IsConnected { get; }
    }
}
