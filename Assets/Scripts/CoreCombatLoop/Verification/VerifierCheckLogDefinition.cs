using System;

namespace Spherebound.CoreCombatLoop.Verification
{
    public sealed class VerifierCheckLogDefinition
    {
        public VerifierCheckLogDefinition(string suiteName, string checkName, VerifierLogCategory category)
        {
            if (string.IsNullOrWhiteSpace(suiteName))
            {
                throw new ArgumentException("Suite name is required.", nameof(suiteName));
            }

            if (string.IsNullOrWhiteSpace(checkName))
            {
                throw new ArgumentException("Check name is required.", nameof(checkName));
            }

            SuiteName = suiteName;
            CheckName = checkName;
            Category = category;
        }

        public string SuiteName { get; }

        public string CheckName { get; }

        public VerifierLogCategory Category { get; }
    }
}
