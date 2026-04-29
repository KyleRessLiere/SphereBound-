using Spherebound.CoreCombatLoop.Scenarios;

namespace Spherebound.CoreCombatLoop.Verification
{
    public sealed class ScenarioCheckReport
    {
        public ScenarioCheckReport(string checkName, bool succeeded, ScenarioRunResult scenarioRun)
        {
            CheckName = checkName;
            Succeeded = succeeded;
            ScenarioRun = scenarioRun;
        }

        public string CheckName { get; }

        public bool Succeeded { get; }

        public ScenarioRunResult ScenarioRun { get; }
    }
}
