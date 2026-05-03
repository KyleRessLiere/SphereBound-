using System;
using System.Collections.Generic;
using System.Linq;

namespace Spherebound.CoreCombatLoop.Core
{
    public static class CombatEnemyIntentPanelBuilder
    {
        public static IReadOnlyList<EnemyIntentSnapshot> Build(CombatState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var enemyIntents = state.UnitsById.Values
                .Where(unit => unit.Side == CombatUnitSide.Enemy && unit.IsAlive)
                .OrderBy(unit => unit.Id)
                .Select(unit => BuildEnemyIntentSnapshot(state, unit))
                .Where(intent => intent != null)
                .Cast<EnemyIntentSnapshot>()
                .ToList();

            return enemyIntents.AsReadOnly();
        }

        private static EnemyIntentSnapshot? BuildEnemyIntentSnapshot(CombatState state, CombatUnitState unit)
        {
            if (unit.BehaviorAssignment?.Behavior == null)
            {
                return null;
            }

            var context = CombatBehaviorContext.FromState(state, unit.Id);
            return unit.BehaviorAssignment.Behavior.DescribeIntent(context);
        }
    }
}
