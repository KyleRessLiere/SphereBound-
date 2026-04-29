using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Scenarios;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class ScenarioRunnerVerifier
    {
        public static IReadOnlyList<string> RunAll()
        {
            var report = CreateReport();
            if (!report.Succeeded)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, report.CheckFailures));
            }

            return report.CompletedChecks;
        }

        public static ScenarioSuiteReport CreateReport()
        {
            var completedChecks = new List<string>();
            var scenarioChecks = new List<ScenarioCheckReport>();
            var checkFailures = new List<string>();

            TryRunCheck(
                nameof(VerifyCatalogScenarios),
                () => VerifyCatalogScenarios(completedChecks, scenarioChecks),
                checkFailures);

            TryRunCheck(
                nameof(VerifyDeterministicRepeatedRun),
                () => VerifyDeterministicRepeatedRun(completedChecks),
                checkFailures);

            TryRunCheck(
                nameof(VerifyMultiScenarioIndependence),
                () => VerifyMultiScenarioIndependence(completedChecks),
                checkFailures);

            return new ScenarioSuiteReport(
                nameof(ScenarioRunnerVerifier),
                checkFailures.Count == 0,
                completedChecks,
                scenarioChecks,
                checkFailures);
        }

        private static void VerifyCatalogScenarios(List<string> completedChecks, List<ScenarioCheckReport> scenarioChecks)
        {
            var runner = new ScenarioRunner();
            var results = runner.RunAll(ScenarioCatalog.All());

            Ensure(results.Count >= 2, "Scenario catalog should contain multiple scenarios.");

            foreach (var result in results)
            {
                Ensure(result.LogLines.Count == result.Events.Count, $"Scenario '{result.Scenario.Id}' should produce one log line per event.");
                scenarioChecks.Add(new ScenarioCheckReport(result.Scenario.Name, result.Verification.Succeeded, result));
                Ensure(result.Verification.Succeeded, $"Scenario '{result.Scenario.Id}' should satisfy its verification contract.");
            }

            completedChecks.Add(nameof(VerifyCatalogScenarios));
        }

        private static void VerifyDeterministicRepeatedRun(List<string> completedChecks)
        {
            var runner = new ScenarioRunner();
            var scenario = ScenarioCatalog.CreateMoveThenKillScenario();

            var firstRun = runner.Run(scenario);
            var secondRun = runner.Run(scenario);

            Ensure(firstRun.ExecutionSucceeded == secondRun.ExecutionSucceeded, "Repeated runs should produce the same execution result.");
            Ensure(firstRun.LogLines.SequenceEqual(secondRun.LogLines), "Repeated runs should produce identical logs.");
            Ensure(firstRun.Events.Select(evt => evt.GetType().Name).SequenceEqual(secondRun.Events.Select(evt => evt.GetType().Name)), "Repeated runs should produce identical event sequences.");
            completedChecks.Add(nameof(VerifyDeterministicRepeatedRun));
        }

        private static void VerifyMultiScenarioIndependence(List<string> completedChecks)
        {
            var runner = new ScenarioRunner();
            var firstScenario = ScenarioCatalog.CreateMoveThenKillScenario();
            var secondScenario = ScenarioCatalog.CreateEnemyTurnScenario();

            var firstRun = runner.Run(firstScenario);
            var secondRun = runner.Run(secondScenario);
            var firstRunAgain = runner.Run(firstScenario);

            Ensure(firstRun.LogLines.SequenceEqual(firstRunAgain.LogLines), "Running another scenario should not change earlier scenario results.");
            Ensure(!firstRun.LogLines.SequenceEqual(secondRun.LogLines), "Different scenarios should be independently observable.");
            completedChecks.Add(nameof(VerifyMultiScenarioIndependence));
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void TryRunCheck(string checkName, Action runCheck, List<string> checkFailures)
        {
            try
            {
                runCheck();
            }
            catch (Exception exception)
            {
                checkFailures.Add($"{checkName}: {exception.Message}");
            }
        }
    }
}
