using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatDebugFileOutputWriter
    {
        public CombatDebugFileOutputWriter(string rootDirectory, string filePath)
        {
            RootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public string RootDirectory { get; }

        public string FilePath { get; }

        public static CombatDebugFileOutputWriter Create(
            string projectRoot,
            CombatDebugFileOutputConfig config,
            string sessionId,
            Func<DateTime>? nowProvider = null)
        {
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                throw new ArgumentException("Project root is required.", nameof(projectRoot));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException("Session id is required.", nameof(sessionId));
            }

            var now = (nowProvider ?? (() => DateTime.Now))();
            var outputRoot = Path.Combine(projectRoot, config.OutputRootRelativePath);
            var datedDirectory = Path.Combine(outputRoot, now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            Directory.CreateDirectory(datedDirectory);

            var fileName = $"{now.ToString("HHmmssfff", CultureInfo.InvariantCulture)}-{Sanitize(sessionId)}.txt";
            var filePath = Path.Combine(datedDirectory, fileName);
            File.WriteAllText(filePath, string.Empty, Encoding.UTF8);
            return new CombatDebugFileOutputWriter(outputRoot, filePath);
        }

        public void Append(string category, string message)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category is required.", nameof(category));
            }

            var builder = new StringBuilder();
            builder.Append('[');
            builder.Append(category);
            builder.Append(']');
            if (!string.IsNullOrEmpty(message))
            {
                builder.Append(' ');
                builder.Append(message);
            }

            builder.AppendLine();
            File.AppendAllText(FilePath, builder.ToString(), Encoding.UTF8);
        }

        private static string Sanitize(string value)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                builder.Append(Array.IndexOf(invalidCharacters, character) >= 0 ? '-' : character);
            }

            return builder.ToString();
        }
    }
}
