using System;
using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatRuntimeEnemyIntentPanelModel
    {
        private readonly IReadOnlyList<EnemyIntentSnapshot> enemyIntents;

        public CombatRuntimeEnemyIntentPanelModel(IReadOnlyList<EnemyIntentSnapshot> enemyIntents)
        {
            this.enemyIntents = enemyIntents ?? throw new ArgumentNullException(nameof(enemyIntents));
        }

        public IReadOnlyList<EnemyIntentSnapshot> EnemyIntents
        {
            get { return enemyIntents; }
        }
    }
}
