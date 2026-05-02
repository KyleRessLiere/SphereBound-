using System;

namespace Spherebound.CoreCombatLoop.Verification
{
    public sealed class VerifierCheckFileReport
    {
        public VerifierCheckFileReport(VerifierCheckLogDefinition definition, bool succeeded, string content)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Succeeded = succeeded;
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public VerifierCheckLogDefinition Definition { get; }

        public bool Succeeded { get; }

        public string Content { get; }
    }
}
