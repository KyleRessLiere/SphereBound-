using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Scenarios
{
    public sealed class ScenarioRunResult
    {
        public ScenarioRunResult(
            ScenarioDefinition scenario,
            bool executionSucceeded,
            CombatState finalState,
            IReadOnlyList<ICombatEvent> events,
            IReadOnlyList<string> logLines,
            ScenarioVerificationResult verification,
            IReadOnlyList<ScenarioRunnerFailure> runnerFailures)
        {
            Scenario = scenario;
            ExecutionSucceeded = executionSucceeded;
            FinalState = finalState;
            Events = events;
            LogLines = logLines;
            Verification = verification;
            RunnerFailures = runnerFailures;
        }

        public ScenarioDefinition Scenario { get; }

        public bool ExecutionSucceeded { get; }

        public CombatState FinalState { get; }

        public IReadOnlyList<ICombatEvent> Events { get; }

        public IReadOnlyList<string> LogLines { get; }

        public ScenarioVerificationResult Verification { get; }

        public IReadOnlyList<ScenarioRunnerFailure> RunnerFailures { get; }
    }
}
