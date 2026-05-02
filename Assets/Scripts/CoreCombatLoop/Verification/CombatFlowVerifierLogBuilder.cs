using System.Collections.Generic;
using System.Text;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class CombatFlowVerifierLogBuilder
    {
        public static string Build(
            string suiteName,
            string checkName,
            bool succeeded,
            string initialBoard,
            IReadOnlyList<string> eventLines,
            string finalBoard,
            IReadOnlyList<string>? notes = null)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"suite: {suiteName}");
            builder.AppendLine($"check: {checkName}");
            builder.AppendLine($"result: {(succeeded ? "pass" : "fail")}");
            builder.AppendLine();
            builder.AppendLine("[initial-board]");
            builder.AppendLine(initialBoard);
            builder.AppendLine();
            builder.AppendLine("[events]");

            if (eventLines.Count == 0)
            {
                builder.AppendLine("(none)");
            }
            else
            {
                for (var index = 0; index < eventLines.Count; index += 1)
                {
                    builder.AppendLine(eventLines[index]);
                }
            }

            builder.AppendLine();
            builder.AppendLine("[final-board]");
            builder.AppendLine(finalBoard);

            if (notes != null && notes.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("[notes]");
                for (var index = 0; index < notes.Count; index += 1)
                {
                    builder.AppendLine(notes[index]);
                }
            }

            return builder.ToString();
        }
    }
}
