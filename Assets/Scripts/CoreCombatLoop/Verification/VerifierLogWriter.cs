using System;
using System.IO;
using System.Text;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class VerifierLogWriter
    {
        public static void WriteSuite(string projectRoot, VerifierSuiteFileReport suiteReport)
        {
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw new ArgumentException("Project root is required.", nameof(projectRoot));
            }

            if (suiteReport == null)
            {
                throw new ArgumentNullException(nameof(suiteReport));
            }

            var suiteDirectory = VerifierLogPathResolver.GetSuiteDirectory(projectRoot, suiteReport.SuiteName);
            Directory.CreateDirectory(suiteDirectory);

            for (var index = 0; index < suiteReport.Checks.Count; index += 1)
            {
                var check = suiteReport.Checks[index];
                var path = VerifierLogPathResolver.GetCheckFilePath(projectRoot, check.Definition);
                File.WriteAllText(path, check.Content, Encoding.UTF8);
            }
        }
    }
}
