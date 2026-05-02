using System;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatDebugSurfaceLog
    {
        public CombatDebugSurfaceLog(
            string category,
            string message)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category is required.", nameof(category));
            }

            Category = category;
            Message = message ?? string.Empty;
        }

        public string Category { get; }

        public string Message { get; }
    }
}
