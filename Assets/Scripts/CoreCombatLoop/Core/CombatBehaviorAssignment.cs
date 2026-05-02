using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatBehaviorAssignment
    {
        public CombatBehaviorAssignment(CombatBehaviorSourceKind sourceKind, ICombatBehaviorDefinition behavior)
        {
            SourceKind = sourceKind;
            Behavior = behavior ?? throw new ArgumentNullException(nameof(behavior));
        }

        public CombatBehaviorSourceKind SourceKind { get; }

        public ICombatBehaviorDefinition Behavior { get; }

        public static CombatBehaviorAssignment Default(ICombatBehaviorDefinition behavior)
        {
            return new CombatBehaviorAssignment(CombatBehaviorSourceKind.Default, behavior);
        }

        public static CombatBehaviorAssignment Scenario(ICombatBehaviorDefinition behavior)
        {
            return new CombatBehaviorAssignment(CombatBehaviorSourceKind.Scenario, behavior);
        }

        public static CombatBehaviorAssignment Manual(ICombatBehaviorDefinition behavior)
        {
            return new CombatBehaviorAssignment(CombatBehaviorSourceKind.Manual, behavior);
        }
    }
}
