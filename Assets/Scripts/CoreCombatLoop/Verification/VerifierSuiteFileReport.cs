using System;
using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Verification
{
    public sealed class VerifierSuiteFileReport
    {
        public VerifierSuiteFileReport(string suiteName, bool succeeded, IReadOnlyList<VerifierCheckFileReport> checks)
        {
            if (string.IsNullOrWhiteSpace(suiteName))
            {
                throw new ArgumentException("Suite name is required.", nameof(suiteName));
            }

            SuiteName = suiteName;
            Succeeded = succeeded;
            Checks = checks ?? throw new ArgumentNullException(nameof(checks));
        }

        public string SuiteName { get; }

        public bool Succeeded { get; }

        public IReadOnlyList<VerifierCheckFileReport> Checks { get; }
    }
}
