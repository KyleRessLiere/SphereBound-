using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Verification
{
    public sealed class ScenarioAbilityBehaviorValidationReport
    {
        public ScenarioAbilityBehaviorValidationReport(
            string suiteName,
            bool succeeded,
            IReadOnlyList<string> completedChecks,
            IReadOnlyList<ScenarioCheckReport> scenarioChecks,
            IReadOnlyList<string> checkFailures)
        {
            SuiteName = suiteName;
            Succeeded = succeeded;
            CompletedChecks = completedChecks;
            ScenarioChecks = scenarioChecks;
            CheckFailures = checkFailures;
        }

        public string SuiteName { get; }

        public bool Succeeded { get; }

        public IReadOnlyList<string> CompletedChecks { get; }

        public IReadOnlyList<ScenarioCheckReport> ScenarioChecks { get; }

        public IReadOnlyList<string> CheckFailures { get; }
    }
}
