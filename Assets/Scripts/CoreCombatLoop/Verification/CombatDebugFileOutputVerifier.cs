using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Spherebound.CoreCombatLoop.UnityBridge;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class CombatDebugFileOutputVerifier
    {
        public static IReadOnlyList<string> RunAll()
        {
            var completedChecks = new List<string>();

            VerifyConfigLoadOrCreate(completedChecks);
            VerifyDisabledConfigDoesNotEnableWriting(completedChecks);
            VerifyWriterCreatesDatedTimestampedFile(completedChecks);
            VerifyWriterPreservesOutputOrder(completedChecks);

            return completedChecks;
        }

        private static void VerifyConfigLoadOrCreate(List<string> completedChecks)
        {
            var projectRoot = CreateTemporaryRoot();
            var config = CombatDebugFileOutputConfigLoader.LoadOrCreate(projectRoot);
            var configPath = CombatDebugFileOutputConfigLoader.GetConfigPath(projectRoot);

            Ensure(File.Exists(configPath), "Config loader should create the config file when missing.");
            Ensure(!config.Enabled, "Default config should start with file output disabled.");
            Ensure(config.OutputRootRelativePath == "CombatDebugOutput", "Default config should use CombatDebugOutput root.");
            completedChecks.Add(nameof(VerifyConfigLoadOrCreate));
        }

        private static void VerifyDisabledConfigDoesNotEnableWriting(List<string> completedChecks)
        {
            var config = new CombatDebugFileOutputConfig(false, "CombatDebugOutput");
            Ensure(!config.Enabled, "Disabled config should remain disabled.");
            completedChecks.Add(nameof(VerifyDisabledConfigDoesNotEnableWriting));
        }

        private static void VerifyWriterCreatesDatedTimestampedFile(List<string> completedChecks)
        {
            var projectRoot = CreateTemporaryRoot();
            var config = new CombatDebugFileOutputConfig(true, "CombatDebugOutput");
            var now = new DateTime(2026, 5, 2, 14, 30, 15, 123, DateTimeKind.Local);

            var writer = CombatDebugFileOutputWriter.Create(projectRoot, config, "session:test", () => now);
            var expectedDirectory = Path.Combine(projectRoot, "CombatDebugOutput", "2026-05-02");
            var fileName = Path.GetFileName(writer.FilePath);

            Ensure(Directory.Exists(expectedDirectory), "Writer should create the dated output directory.");
            Ensure(writer.FilePath.StartsWith(expectedDirectory, StringComparison.Ordinal), "Writer should place the file inside the dated directory.");
            Ensure(fileName.StartsWith("143015123-", StringComparison.Ordinal), "Writer should use a timestamp prefix in the file name.");
            Ensure(fileName.EndsWith(".txt", StringComparison.Ordinal), "Writer should create a text file.");
            completedChecks.Add(nameof(VerifyWriterCreatesDatedTimestampedFile));
        }

        private static void VerifyWriterPreservesOutputOrder(List<string> completedChecks)
        {
            var projectRoot = CreateTemporaryRoot();
            var config = new CombatDebugFileOutputConfig(true, "CombatDebugOutput");
            var writer = CombatDebugFileOutputWriter.Create(projectRoot, config, "ordered-session", () => new DateTime(2026, 5, 2, 15, 45, 1, 1, DateTimeKind.Local));

            writer.Append("CoreEvent", "TurnStarted side=Player");
            writer.Append("Actions", "remainingActions=2");
            writer.Append("Board", "[ ][P][ ][ ]");

            var lines = File.ReadAllLines(writer.FilePath);
            Ensure(lines.Length == 3, "Writer should append one line per write.");
            Ensure(lines[0] == "[CoreEvent] TurnStarted side=Player", "First written line should be preserved.");
            Ensure(lines[1] == "[Actions] remainingActions=2", "Second written line should be preserved.");
            Ensure(lines[2] == "[Board] [ ][P][ ][ ]", "Third written line should be preserved.");
            completedChecks.Add(nameof(VerifyWriterPreservesOutputOrder));
        }

        private static string CreateTemporaryRoot()
        {
            return Path.Combine(Path.GetTempPath(), "spherebound-debug-file-" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
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
