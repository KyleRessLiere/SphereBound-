using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatActionResult
    {
        public CombatActionResult(bool succeeded, IReadOnlyList<ICombatEvent> events, CombatFailureReason failureReason)
        {
            Succeeded = succeeded;
            Events = events;
            FailureReason = failureReason;
        }

        public bool Succeeded { get; }

        public IReadOnlyList<ICombatEvent> Events { get; }

        public CombatFailureReason FailureReason { get; }
    }
}
