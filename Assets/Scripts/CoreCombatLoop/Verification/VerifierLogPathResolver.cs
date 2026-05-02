using System;
using System.IO;
using System.Text;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class VerifierLogPathResolver
    {
        public const string VerificationDirectoryRelativePath = "Assets/Scripts/CoreCombatLoop/Verification";

        public static string GetSuiteDirectory(string projectRoot, string suiteName)
        {
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw new ArgumentException("Project root is required.", nameof(projectRoot));
            }

            if (string.IsNullOrWhiteSpace(suiteName))
            {
                throw new ArgumentException("Suite name is required.", nameof(suiteName));
            }

            return Path.Combine(
                projectRoot,
                VerificationDirectoryRelativePath,
                ToSafeName(suiteName) + ".logs");
        }

        public static string GetCheckFilePath(string projectRoot, VerifierCheckLogDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return Path.Combine(
                GetSuiteDirectory(projectRoot, definition.SuiteName),
                ToSafeName(definition.CheckName) + ".txt");
        }

        public static string ToSafeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A safe-name value is required.", nameof(value));
            }

            var builder = new StringBuilder(value.Length);
            for (var index = 0; index < value.Length; index += 1)
            {
                var character = value[index];
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                }
                else if (character == '-' || character == '_')
                {
                    builder.Append(character);
                }
                else
                {
                    builder.Append('_');
                }
            }

            return builder.ToString();
        }
    }
}
