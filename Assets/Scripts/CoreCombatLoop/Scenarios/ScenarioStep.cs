using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Scenarios
{
    public sealed class ScenarioStep
    {
        private ScenarioStep(
            ScenarioStepType stepType,
            int? actorUnitId,
            GridPosition? destination,
            int? targetUnitId,
            string description)
        {
            StepType = stepType;
            ActorUnitId = actorUnitId;
            Destination = destination;
            TargetUnitId = targetUnitId;
            Description = description;
        }

        public ScenarioStepType StepType { get; }

        public int? ActorUnitId { get; }

        public GridPosition? Destination { get; }

        public int? TargetUnitId { get; }

        public string Description { get; }

        public static ScenarioStep StartCombat(string description = "Start combat")
        {
            return new ScenarioStep(ScenarioStepType.StartCombat, null, null, null, description);
        }

        public static ScenarioStep Move(int actorUnitId, GridPosition destination, string description = "")
        {
            return new ScenarioStep(ScenarioStepType.Move, actorUnitId, destination, null, description);
        }

        public static ScenarioStep Attack(int actorUnitId, int targetUnitId, string description = "")
        {
            return new ScenarioStep(ScenarioStepType.Attack, actorUnitId, null, targetUnitId, description);
        }

        public static ScenarioStep EndPlayerTurn(string description = "End player turn")
        {
            return new ScenarioStep(ScenarioStepType.EndPlayerTurn, null, null, null, description);
        }

        public static ScenarioStep RunBehaviorTurnCycle(string description = "Run behavior turn cycle")
        {
            return new ScenarioStep(ScenarioStepType.RunBehaviorTurnCycle, null, null, null, description);
        }
    }
}
