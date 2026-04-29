using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class BridgedCombatSessionSnapshot
    {
        public BridgedCombatSessionSnapshot(
            string sessionId,
            IReadOnlyList<BridgedUnitSnapshot> units,
            bool isConnected)
        {
            SessionId = sessionId;
            Units = units;
            IsConnected = isConnected;
        }

        public string SessionId { get; }

        public IReadOnlyList<BridgedUnitSnapshot> Units { get; }

        public bool IsConnected { get; }
    }
}
