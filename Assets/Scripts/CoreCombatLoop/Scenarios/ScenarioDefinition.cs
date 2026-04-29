using System;
using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Scenarios
{
    public sealed class ScenarioDefinition
    {
        public ScenarioDefinition(
            string id,
            string name,
            Func<CombatState> createInitialState,
            IReadOnlyList<ScenarioStep> steps,
            IReadOnlyList<ScenarioExpectation> expectations,
            bool expectedExecutionSucceeded,
            IReadOnlyList<string> expectedEventTypeSequence)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Scenario id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Scenario name is required.", nameof(name));
            }

            Id = id;
            Name = name;
            CreateInitialState = createInitialState ?? throw new ArgumentNullException(nameof(createInitialState));
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
            Expectations = expectations ?? throw new ArgumentNullException(nameof(expectations));
            ExpectedExecutionSucceeded = expectedExecutionSucceeded;
            ExpectedEventTypeSequence = expectedEventTypeSequence ?? throw new ArgumentNullException(nameof(expectedEventTypeSequence));
        }

        public string Id { get; }

        public string Name { get; }

        public Func<CombatState> CreateInitialState { get; }

        public IReadOnlyList<ScenarioStep> Steps { get; }

        public IReadOnlyList<ScenarioExpectation> Expectations { get; }

        public bool ExpectedExecutionSucceeded { get; }

        public IReadOnlyList<string> ExpectedEventTypeSequence { get; }
    }
}
