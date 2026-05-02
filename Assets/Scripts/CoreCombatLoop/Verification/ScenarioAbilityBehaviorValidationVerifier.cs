using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Scenarios;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class ScenarioAbilityBehaviorValidationVerifier
    {
        public const string BasicAttackHitScenarioName = "Basic Attack Hit";
        public const string BasicAttackMissScenarioName = "Basic Attack Miss";
        public const string AbilityShapeValidationScenarioName = "Ability Shape Validation";
        public const string MultiTurnCombatResolutionScenarioName = "Multi-Turn Combat Resolution";
        public const string BehaviorMovementInteractionScenarioName = "Behavior + Movement Interaction";
        public const string ForcedMovementValidationScenarioName = "Forced Movement Validation";

        public static IReadOnlyList<string> RunAll()
        {
            var report = CreateReport();
            if (!report.Succeeded)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, report.CheckFailures));
            }

            return report.CompletedChecks;
        }

        public static ScenarioAbilityBehaviorValidationReport CreateReport()
        {
            var completedChecks = new List<string>();
            var scenarioChecks = new List<ScenarioCheckReport>();
            var checkFailures = new List<string>();

            TryRunCheck(nameof(VerifyScenarioList), () => VerifyScenarioList(completedChecks), checkFailures);
            TryRunCheck(nameof(VerifySuiteNaming), () => VerifySuiteNaming(completedChecks), checkFailures);
            TryRunCheck(nameof(VerifyValidationScenarios), () => VerifyValidationScenarios(completedChecks, scenarioChecks), checkFailures);

            return new ScenarioAbilityBehaviorValidationReport(
                nameof(ScenarioAbilityBehaviorValidationVerifier),
                checkFailures.Count == 0,
                completedChecks,
                scenarioChecks,
                checkFailures);
        }

        public static IReadOnlyList<string> GetInitialScenarioNames()
        {
            return new[]
            {
                BasicAttackHitScenarioName,
                BasicAttackMissScenarioName,
                AbilityShapeValidationScenarioName,
                MultiTurnCombatResolutionScenarioName,
                BehaviorMovementInteractionScenarioName,
                ForcedMovementValidationScenarioName,
            };
        }

        private static void VerifyScenarioList(List<string> completedChecks)
        {
            var names = GetInitialScenarioNames();
            Ensure(names.Count >= 5, "Scenario validation suite should define the initial scenario set.");
            Ensure(names.Contains(BasicAttackHitScenarioName), "Scenario validation suite should include Basic Attack Hit.");
            Ensure(names.Contains(BasicAttackMissScenarioName), "Scenario validation suite should include Basic Attack Miss.");
            Ensure(names.Contains(AbilityShapeValidationScenarioName), "Scenario validation suite should include Ability Shape Validation.");
            Ensure(names.Contains(MultiTurnCombatResolutionScenarioName), "Scenario validation suite should include Multi-Turn Combat Resolution.");
            Ensure(names.Contains(BehaviorMovementInteractionScenarioName), "Scenario validation suite should include Behavior + Movement Interaction.");
            completedChecks.Add(nameof(VerifyScenarioList));
        }

        private static void VerifySuiteNaming(List<string> completedChecks)
        {
            Ensure(typeof(ScenarioAbilityBehaviorValidationVerifier).Name == "ScenarioAbilityBehaviorValidationVerifier", "Validation suite should have a stable verifier identity.");
            completedChecks.Add(nameof(VerifySuiteNaming));
        }

        private static void VerifyValidationScenarios(List<string> completedChecks, List<ScenarioCheckReport> scenarioChecks)
        {
            var runner = new ScenarioRunner();

            foreach (var validationCase in ScenarioAbilityBehaviorValidationCatalog.All())
            {
                var result = runner.Run(validationCase.Scenario);
                var failures = new List<string>();

                if (!result.Verification.Succeeded)
                {
                    failures.AddRange(result.Verification.Failures.Select(failure => $"{failure.Code}: {failure.Message}"));
                }

                ScenarioEventSequenceValidator.Validate(
                    validationCase.EventValidationMode,
                    validationCase.ExpectedEventTypeSequence,
                    result.Events,
                    failures);

                validationCase.AdditionalValidation?.Invoke(result, failures);

                scenarioChecks.Add(new ScenarioCheckReport(validationCase.Scenario.Name, failures.Count == 0, result));

                if (failures.Count > 0)
                {
                    throw new InvalidOperationException($"{validationCase.CheckName}: {string.Join(" | ", failures)}");
                }
            }

            completedChecks.Add(nameof(VerifyValidationScenarios));
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
