using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class VerifierLogOutputVerifier
    {
        public static IReadOnlyList<string> RunAll()
        {
            var completedChecks = new List<string>();

            VerifyPathResolverProducesStablePaths(completedChecks);
            VerifyWriterOverwritesLatestCheckFile(completedChecks);
            VerifyCombatFlowBuilderIncludesBoardsAndEvents(completedChecks);
            VerifyAssertionBuilderIncludesSummary(completedChecks);

            return completedChecks;
        }

        private static void VerifyPathResolverProducesStablePaths(List<string> completedChecks)
        {
            var projectRoot = CreateTemporaryRoot();
            var definition = new VerifierCheckLogDefinition("CombatLoopVerifier", "VerifyPlayerMovement", VerifierLogCategory.CombatFlow);
            var firstPath = VerifierLogPathResolver.GetCheckFilePath(projectRoot, definition);
            var secondPath = VerifierLogPathResolver.GetCheckFilePath(projectRoot, definition);

            Ensure(firstPath == secondPath, "Path resolver should produce the same path for the same suite/check definition.");
            Ensure(
                firstPath.EndsWith(
                    Path.Combine("Assets", "Scripts", "CoreCombatLoop", "Verification", "CombatLoopVerifier.logs", "VerifyPlayerMovement.txt"),
                    StringComparison.Ordinal),
                "Path resolver should create a stable verifier log path beside the verifier code.");
            completedChecks.Add(nameof(VerifyPathResolverProducesStablePaths));
        }

        private static void VerifyWriterOverwritesLatestCheckFile(List<string> completedChecks)
        {
            var projectRoot = CreateTemporaryRoot();
            var definition = new VerifierCheckLogDefinition("CombatLoopVerifier", "VerifyPlayerMovement", VerifierLogCategory.CombatFlow);
            var suiteReport = new VerifierSuiteFileReport(
                "CombatLoopVerifier",
                true,
                new[]
                {
                    new VerifierCheckFileReport(definition, true, "first"),
                });
            VerifierLogWriter.WriteSuite(projectRoot, suiteReport);
            VerifierLogWriter.WriteSuite(
                projectRoot,
                new VerifierSuiteFileReport(
                    "CombatLoopVerifier",
                    true,
                    new[]
                    {
                        new VerifierCheckFileReport(definition, true, "second"),
                    }));

            var path = VerifierLogPathResolver.GetCheckFilePath(projectRoot, definition);
            var text = File.ReadAllText(path);
            Ensure(text == "second", "Writer should overwrite a check log file with the latest run content.");
            completedChecks.Add(nameof(VerifyWriterOverwritesLatestCheckFile));
        }

        private static void VerifyCombatFlowBuilderIncludesBoardsAndEvents(List<string> completedChecks)
        {
            var content = CombatFlowVerifierLogBuilder.Build(
                "CombatLoopVerifier",
                "VerifyPlayerMovement",
                true,
                "[ ][P]",
                new[] { "MoveRequested unit=1 destination=(1, 2)" },
                "[P][ ]",
                new[] { "failureReason=None" });

            Ensure(content.Contains("[initial-board]"), "Combat-flow builder should include an initial-board section.");
            Ensure(content.Contains("[events]"), "Combat-flow builder should include an events section.");
            Ensure(content.Contains("[final-board]"), "Combat-flow builder should include a final-board section.");
            Ensure(content.Contains("MoveRequested unit=1 destination=(1, 2)"), "Combat-flow builder should include event lines.");
            completedChecks.Add(nameof(VerifyCombatFlowBuilderIncludesBoardsAndEvents));
        }

        private static void VerifyAssertionBuilderIncludesSummary(List<string> completedChecks)
        {
            var content = AssertionVerifierLogBuilder.Build(
                "CombatRuntimeUiDataVerifier",
                "VerifyAbilitySelectionCycling",
                true,
                "Runtime ability selection cycles deterministically.",
                new[] { "expected wrap behavior was preserved" });

            Ensure(content.Contains("[summary]"), "Assertion builder should include a summary section.");
            Ensure(content.Contains("[details]"), "Assertion builder should include a details section when provided.");
            Ensure(content.Contains("Runtime ability selection cycles deterministically."), "Assertion builder should include the summary text.");
            completedChecks.Add(nameof(VerifyAssertionBuilderIncludesSummary));
        }

        private static string CreateTemporaryRoot()
        {
            return Path.Combine(Path.GetTempPath(), "spherebound-verifier-log-" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
