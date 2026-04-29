using System;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Scenarios
{
    public static class ScenarioStateFactory
    {
        public static CombatState CreateFreshState(ScenarioDefinition scenario)
        {
            if (scenario == null)
            {
                throw new ArgumentNullException(nameof(scenario));
            }

            var state = scenario.CreateInitialState();
            if (state == null)
            {
                throw new InvalidOperationException($"Scenario '{scenario.Id}' returned a null initial state.");
            }

            return state;
        }
    }
}
