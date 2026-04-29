namespace Spherebound.CoreCombatLoop.Scenarios
{
    public sealed class ScenarioVerificationFailure
    {
        public ScenarioVerificationFailure(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; }

        public string Message { get; }
    }
}
