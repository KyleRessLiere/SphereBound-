using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatDebugCommandResult
    {
        public CombatDebugCommandResult(
            bool succeeded,
            CombatDebugCommandFailureReason commandFailureReason,
            IReadOnlyList<ICombatEvent> events,
            CombatFailureReason gameplayFailureReason)
        {
            Succeeded = succeeded;
            CommandFailureReason = commandFailureReason;
            Events = events;
            GameplayFailureReason = gameplayFailureReason;
        }

        public bool Succeeded { get; }

        public CombatDebugCommandFailureReason CommandFailureReason { get; }

        public IReadOnlyList<ICombatEvent> Events { get; }

        public CombatFailureReason GameplayFailureReason { get; }
    }
}
