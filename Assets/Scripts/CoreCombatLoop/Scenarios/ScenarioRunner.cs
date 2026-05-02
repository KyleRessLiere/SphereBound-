using System;
using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Scenarios
{
    public sealed class ScenarioRunner
    {
        private readonly CombatEngine combatEngine;

        public ScenarioRunner()
        {
            combatEngine = new CombatEngine();
        }

        public ScenarioRunResult Run(ScenarioDefinition scenario)
        {
            if (scenario == null)
            {
                throw new ArgumentNullException(nameof(scenario));
            }

            var events = new List<ICombatEvent>();
            var logLines = new List<string>();
            var runnerFailures = new List<ScenarioRunnerFailure>();
            var executionSucceeded = true;

            CombatState finalState;
            try
            {
                finalState = ScenarioStateFactory.CreateFreshState(scenario);
            }
            catch (Exception exception)
            {
                runnerFailures.Add(new ScenarioRunnerFailure("initial-state", exception.Message));
                var emptyVerification = new ScenarioVerificationResult(false, Array.Empty<ScenarioVerificationFailure>());
                return new ScenarioRunResult(
                    scenario,
                    false,
                    null!,
                    events,
                    logLines,
                    emptyVerification,
                    runnerFailures);
            }

            for (var stepIndex = 0; stepIndex < scenario.Steps.Count; stepIndex += 1)
            {
                var step = scenario.Steps[stepIndex];
                if (step == null)
                {
                    executionSucceeded = false;
                    runnerFailures.Add(new ScenarioRunnerFailure("null-step", "Scenario contains a null step.", stepIndex));
                    continue;
                }

                try
                {
                    var stepSucceeded = ExecuteStep(finalState, step, events, logLines, runnerFailures, stepIndex);
                    if (!stepSucceeded)
                    {
                        executionSucceeded = false;
                    }
                }
                catch (Exception exception)
                {
                    executionSucceeded = false;
                    runnerFailures.Add(new ScenarioRunnerFailure("step-exception", exception.Message, stepIndex));
                }
            }

            var verification = ScenarioVerifier.Verify(
                scenario,
                executionSucceeded,
                finalState,
                events,
                runnerFailures);

            return new ScenarioRunResult(
                scenario,
                executionSucceeded,
                finalState,
                events,
                logLines,
                verification,
                runnerFailures);
        }

        public IReadOnlyList<ScenarioRunResult> RunAll(IEnumerable<ScenarioDefinition> scenarios)
        {
            var results = new List<ScenarioRunResult>();
            foreach (var scenario in scenarios)
            {
                results.Add(Run(scenario));
            }

            return results;
        }

        private bool ExecuteStep(
            CombatState state,
            ScenarioStep step,
            List<ICombatEvent> events,
            List<string> logLines,
            List<ScenarioRunnerFailure> runnerFailures,
            int stepIndex)
        {
            switch (step.StepType)
            {
                case ScenarioStepType.StartCombat:
                    AppendEvents(combatEngine.StartCombat(state), events, logLines);
                    return true;

                case ScenarioStepType.Move:
                    if (!step.ActorUnitId.HasValue || !step.Destination.HasValue)
                    {
                        runnerFailures.Add(new ScenarioRunnerFailure("malformed-move", "Move step is missing actor or destination.", stepIndex));
                        return false;
                    }

                    var moveResult = combatEngine.ResolveMove(state, step.ActorUnitId.Value, step.Destination.Value);
                    AppendEvents(moveResult.Events, events, logLines);
                    return moveResult.Succeeded;

                case ScenarioStepType.Attack:
                    if (!step.ActorUnitId.HasValue || !step.TargetUnitId.HasValue)
                    {
                        runnerFailures.Add(new ScenarioRunnerFailure("malformed-attack", "Attack step is missing actor or target.", stepIndex));
                        return false;
                    }

                    var attackResult = combatEngine.ResolveAttack(state, step.ActorUnitId.Value, step.TargetUnitId.Value);
                    AppendEvents(attackResult.Events, events, logLines);
                    return attackResult.Succeeded;

                case ScenarioStepType.EndPlayerTurn:
                    AppendEvents(combatEngine.EndPlayerTurnAndRunEnemyTurn(state), events, logLines);
                    return true;

                case ScenarioStepType.RunBehaviorTurnCycle:
                    AppendEvents(combatEngine.RunBehaviorTurnCycle(state), events, logLines);
                    return true;

                default:
                    runnerFailures.Add(new ScenarioRunnerFailure(
                        "unsupported-step",
                        $"Unsupported scenario step type '{step.StepType}'.",
                        stepIndex));
                    return false;
            }
        }

        private static void AppendEvents(
            IReadOnlyList<ICombatEvent> stepEvents,
            List<ICombatEvent> events,
            List<string> logLines)
        {
            foreach (var combatEvent in stepEvents)
            {
                events.Add(combatEvent);
                logLines.Add(ScenarioLogFormatter.Format(combatEvent));
            }
        }
    }
}
