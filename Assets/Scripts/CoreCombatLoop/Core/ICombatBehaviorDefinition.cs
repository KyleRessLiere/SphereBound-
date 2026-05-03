namespace Spherebound.CoreCombatLoop.Core
{
    public interface ICombatBehaviorDefinition
    {
        string BehaviorId { get; }

        CombatBehaviorDecision DecideIntent(CombatBehaviorContext context);

        EnemyIntentSnapshot DescribeIntent(CombatBehaviorContext context);
    }
}
