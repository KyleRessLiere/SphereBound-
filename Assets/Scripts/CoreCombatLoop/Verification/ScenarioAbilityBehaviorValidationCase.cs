using System;
using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Scenarios;

namespace Spherebound.CoreCombatLoop.Verification
{
    public sealed class ScenarioAbilityBehaviorValidationCase
    {
        public ScenarioAbilityBehaviorValidationCase(
            string checkName,
            ScenarioDefinition scenario,
            ScenarioEventValidationMode eventValidationMode,
            IReadOnlyList<string> expectedEventTypeSequence,
            Action<ScenarioRunResult, List<string>>? additionalValidation = null)
        {
            if (string.IsNullOrWhiteSpace(checkName))
            {
                throw new ArgumentException("Check name is required.", nameof(checkName));
            }

            CheckName = checkName;
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            EventValidationMode = eventValidationMode;
            ExpectedEventTypeSequence = expectedEventTypeSequence ?? throw new ArgumentNullException(nameof(expectedEventTypeSequence));
            AdditionalValidation = additionalValidation;
        }

        public string CheckName { get; }

        public ScenarioDefinition Scenario { get; }

        public ScenarioEventValidationMode EventValidationMode { get; }

        public IReadOnlyList<string> ExpectedEventTypeSequence { get; }

        public Action<ScenarioRunResult, List<string>>? AdditionalValidation { get; }
    }
}
