namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatDebugFileOutputConfig
    {
        public CombatDebugFileOutputConfig(bool enabled, string outputRootRelativePath)
        {
            Enabled = enabled;
            OutputRootRelativePath = string.IsNullOrWhiteSpace(outputRootRelativePath)
                ? "CombatDebugOutput"
                : outputRootRelativePath;
        }

        public bool Enabled { get; }

        public string OutputRootRelativePath { get; }
    }
}
