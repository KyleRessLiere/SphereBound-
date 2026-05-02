using System;
using System.Collections.Generic;
using System.IO;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public static class CombatDebugFileOutputConfigLoader
    {
        public const string DefaultConfigRelativePath = "CombatDebugOutput/combat-debug-output.config";

        public static CombatDebugFileOutputConfig LoadOrCreate(string projectRoot)
        {
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw new ArgumentException("Project root is required.", nameof(projectRoot));
            }

            var configPath = GetConfigPath(projectRoot);
            var configDirectory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            if (!File.Exists(configPath))
            {
                var defaultConfig = new CombatDebugFileOutputConfig(false, "CombatDebugOutput");
                File.WriteAllText(configPath, Serialize(defaultConfig));
                return defaultConfig;
            }

            return Parse(File.ReadAllLines(configPath));
        }

        public static string GetConfigPath(string projectRoot)
        {
            return Path.Combine(projectRoot, DefaultConfigRelativePath);
        }

        private static CombatDebugFileOutputConfig Parse(IReadOnlyList<string> lines)
        {
            var enabled = false;
            var outputRoot = "CombatDebugOutput";

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0 || separatorIndex == line.Length - 1)
                {
                    continue;
                }

                var key = line.Substring(0, separatorIndex).Trim();
                var value = line.Substring(separatorIndex + 1).Trim();

                if (key.Equals("enabled", StringComparison.OrdinalIgnoreCase))
                {
                    enabled = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                else if (key.Equals("output_root", StringComparison.OrdinalIgnoreCase))
                {
                    outputRoot = string.IsNullOrWhiteSpace(value) ? outputRoot : value;
                }
            }

            return new CombatDebugFileOutputConfig(enabled, outputRoot);
        }

        private static string Serialize(CombatDebugFileOutputConfig config)
        {
            return "# Unity combat debug file output" + Environment.NewLine
                + $"enabled={(config.Enabled ? "true" : "false")}" + Environment.NewLine
                + $"output_root={config.OutputRootRelativePath}" + Environment.NewLine;
        }
    }
}
