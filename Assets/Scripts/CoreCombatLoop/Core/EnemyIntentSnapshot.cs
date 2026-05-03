using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class EnemyIntentSnapshot
    {
        public EnemyIntentSnapshot(
            int enemyUnitId,
            string enemyDisplayName,
            EnemyIntentType intentType,
            string actionName,
            int? targetUnitId,
            string? targetDisplayName,
            GridPosition? targetPosition,
            int? countdownValue,
            string summaryText)
        {
            if (string.IsNullOrWhiteSpace(enemyDisplayName))
            {
                throw new ArgumentException("Enemy display name is required.", nameof(enemyDisplayName));
            }

            if (string.IsNullOrWhiteSpace(actionName))
            {
                throw new ArgumentException("Action name is required.", nameof(actionName));
            }

            if (string.IsNullOrWhiteSpace(summaryText))
            {
                throw new ArgumentException("Summary text is required.", nameof(summaryText));
            }

            EnemyUnitId = enemyUnitId;
            EnemyDisplayName = enemyDisplayName;
            IntentType = intentType;
            ActionName = actionName;
            TargetUnitId = targetUnitId;
            TargetDisplayName = targetDisplayName;
            TargetPosition = targetPosition;
            CountdownValue = countdownValue;
            SummaryText = summaryText;
        }

        public int EnemyUnitId { get; }

        public string EnemyDisplayName { get; }

        public EnemyIntentType IntentType { get; }

        public string ActionName { get; }

        public int? TargetUnitId { get; }

        public string? TargetDisplayName { get; }

        public GridPosition? TargetPosition { get; }

        public int? CountdownValue { get; }

        public string SummaryText { get; }
    }
}
