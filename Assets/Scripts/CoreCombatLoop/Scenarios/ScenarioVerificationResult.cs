using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Scenarios
{
    public sealed class ScenarioVerificationResult
    {
        public ScenarioVerificationResult(bool succeeded, IReadOnlyList<ScenarioVerificationFailure> failures)
        {
            Succeeded = succeeded;
            Failures = failures;
        }

        public bool Succeeded { get; }

        public IReadOnlyList<ScenarioVerificationFailure> Failures { get; }
    }
}
