namespace Spherebound.CoreCombatLoop.Scenarios
{
    public sealed class ScenarioRunnerFailure
    {
        public ScenarioRunnerFailure(string code, string message, int? stepIndex = null)
        {
            Code = code;
            Message = message;
            StepIndex = stepIndex;
        }

        public string Code { get; }

        public string Message { get; }

        public int? StepIndex { get; }
    }
}
