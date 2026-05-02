using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class ScenarioEventSequenceValidator
    {
        public static void Validate(
            ScenarioEventValidationMode mode,
            IReadOnlyList<string> expectedEventTypeSequence,
            IReadOnlyList<ICombatEvent> events,
            List<string> failures)
        {
            var actualTypeNames = events.Select(combatEvent => combatEvent.GetType().Name).ToList();

            if (mode == ScenarioEventValidationMode.ExactSequence)
            {
                ValidateExact(expectedEventTypeSequence, actualTypeNames, failures);
                return;
            }

            ValidateOrderedSubsequence(expectedEventTypeSequence, actualTypeNames, failures);
        }

        private static void ValidateExact(
            IReadOnlyList<string> expected,
            IReadOnlyList<string> actual,
            List<string> failures)
        {
            if (expected.Count != actual.Count)
            {
                failures.Add($"Expected exact event count {expected.Count} but observed {actual.Count}.");
                return;
            }

            for (var index = 0; index < expected.Count; index += 1)
            {
                if (!string.Equals(expected[index], actual[index], StringComparison.Ordinal))
                {
                    failures.Add($"Expected exact event '{expected[index]}' at index {index} but observed '{actual[index]}'.");
                    return;
                }
            }
        }

        private static void ValidateOrderedSubsequence(
            IReadOnlyList<string> expected,
            IReadOnlyList<string> actual,
            List<string> failures)
        {
            var searchIndex = 0;

            for (var expectedIndex = 0; expectedIndex < expected.Count; expectedIndex += 1)
            {
                var expectedEventName = expected[expectedIndex];
                var foundIndex = -1;

                for (var actualIndex = searchIndex; actualIndex < actual.Count; actualIndex += 1)
                {
                    if (string.Equals(actual[actualIndex], expectedEventName, StringComparison.Ordinal))
                    {
                        foundIndex = actualIndex;
                        break;
                    }
                }

                if (foundIndex < 0)
                {
                    failures.Add($"Expected ordered event '{expectedEventName}' was not observed in the required order.");
                    return;
                }

                searchIndex = foundIndex + 1;
            }
        }
    }
}
