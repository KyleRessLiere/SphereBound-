using System.Collections.Generic;
using System.Text;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class AssertionVerifierLogBuilder
    {
        public static string Build(
            string suiteName,
            string checkName,
            bool succeeded,
            string summary,
            IReadOnlyList<string>? details = null)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"suite: {suiteName}");
            builder.AppendLine($"check: {checkName}");
            builder.AppendLine($"result: {(succeeded ? "pass" : "fail")}");
            builder.AppendLine();
            builder.AppendLine("[summary]");
            builder.AppendLine(summary);

            if (details != null && details.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("[details]");
                for (var index = 0; index < details.Count; index += 1)
                {
                    builder.AppendLine(details[index]);
                }
            }

            return builder.ToString();
        }
    }
}
