using System;
using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatRuntimeEnemyIntentPreview
    {
        public CombatRuntimeEnemyIntentPreview(
            EnemyIntentSnapshot intentSnapshot,
            IReadOnlyList<GridPosition> moveHighlightTiles,
            IReadOnlyList<GridPosition> effectHighlightTiles)
        {
            IntentSnapshot = intentSnapshot ?? throw new ArgumentNullException(nameof(intentSnapshot));
            MoveHighlightTiles = moveHighlightTiles ?? throw new ArgumentNullException(nameof(moveHighlightTiles));
            EffectHighlightTiles = effectHighlightTiles ?? throw new ArgumentNullException(nameof(effectHighlightTiles));
        }

        public EnemyIntentSnapshot IntentSnapshot { get; }

        public IReadOnlyList<GridPosition> MoveHighlightTiles { get; }

        public IReadOnlyList<GridPosition> EffectHighlightTiles { get; }
    }
}
