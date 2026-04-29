using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Scenarios
{
    public static class ScenarioVerifier
    {
        public static ScenarioVerificationResult Verify(
            ScenarioDefinition scenario,
            bool executionSucceeded,
            CombatState finalState,
            IReadOnlyList<ICombatEvent> events,
            IReadOnlyList<ScenarioRunnerFailure> runnerFailures)
        {
            var failures = new List<ScenarioVerificationFailure>();

            foreach (var runnerFailure in runnerFailures)
            {
                failures.Add(new ScenarioVerificationFailure(
                    $"runner:{runnerFailure.Code}",
                    runnerFailure.Message));
            }

            if (executionSucceeded != scenario.ExpectedExecutionSucceeded)
            {
                failures.Add(new ScenarioVerificationFailure(
                    "execution-mismatch",
                    $"Scenario '{scenario.Id}' expected executionSucceeded={scenario.ExpectedExecutionSucceeded} but was {executionSucceeded}."));
            }

            VerifyExpectedEvents(scenario.ExpectedEventTypeSequence, events, failures);
            VerifyExpectations(scenario.Expectations, finalState, failures);

            return new ScenarioVerificationResult(failures.Count == 0, failures);
        }

        private static void VerifyExpectedEvents(
            IReadOnlyList<string> expectedEventTypeSequence,
            IReadOnlyList<ICombatEvent> events,
            List<ScenarioVerificationFailure> failures)
        {
            var eventTypeNames = events.Select(evt => evt.GetType().Name).ToList();
            var searchIndex = 0;

            foreach (var expectedEventName in expectedEventTypeSequence)
            {
                var foundIndex = -1;
                for (var i = searchIndex; i < eventTypeNames.Count; i += 1)
                {
                    if (string.Equals(eventTypeNames[i], expectedEventName, StringComparison.Ordinal))
                    {
                        foundIndex = i;
                        break;
                    }
                }

                if (foundIndex < 0)
                {
                    failures.Add(new ScenarioVerificationFailure(
                        "event-sequence",
                        $"Expected event '{expectedEventName}' was not observed in the required order."));
                    return;
                }

                searchIndex = foundIndex + 1;
            }
        }

        private static void VerifyExpectations(
            IReadOnlyList<ScenarioExpectation> expectations,
            CombatState finalState,
            List<ScenarioVerificationFailure> failures)
        {
            foreach (var expectation in expectations)
            {
                finalState.TryGetUnit(expectation.UnitId, out var unit);

                if (expectation.ExpectedLifeState.HasValue)
                {
                    if (expectation.ExpectedLifeState.Value == UnitLifeState.Dead)
                    {
                        if (unit != null)
                        {
                            failures.Add(new ScenarioVerificationFailure(
                                "life-state",
                                $"Unit {expectation.UnitId} was expected to be removed/dead but is still present."));
                        }

                        continue;
                    }

                    if (unit == null || unit.LifeState != expectation.ExpectedLifeState.Value)
                    {
                        failures.Add(new ScenarioVerificationFailure(
                            "life-state",
                            $"Unit {expectation.UnitId} expected lifeState={expectation.ExpectedLifeState.Value}."));
                    }
                }

                if (unit == null)
                {
                    failures.Add(new ScenarioVerificationFailure(
                        "unit-missing",
                        $"Expected unit {expectation.UnitId} to be present for verification."));
                    continue;
                }

                if (expectation.ExpectedHealth.HasValue && unit.CurrentHealth != expectation.ExpectedHealth.Value)
                {
                    failures.Add(new ScenarioVerificationFailure(
                        "health",
                        $"Unit {expectation.UnitId} expected health={expectation.ExpectedHealth.Value} but was {unit.CurrentHealth}."));
                }

                if (expectation.ExpectedPosition.HasValue)
                {
                    var expectedPosition = expectation.ExpectedPosition.Value;
                    if (unit.Position.X != expectedPosition.X || unit.Position.Y != expectedPosition.Y)
                    {
                        failures.Add(new ScenarioVerificationFailure(
                            "position",
                            $"Unit {expectation.UnitId} expected position={expectedPosition} but was {unit.Position}."));
                    }
                }
            }
        }
    }
}
