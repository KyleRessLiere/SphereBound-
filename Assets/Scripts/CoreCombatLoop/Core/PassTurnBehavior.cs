namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class PassTurnBehavior : ICombatBehaviorDefinition
    {
        public const string DefaultBehaviorId = "pass-turn";

        public PassTurnBehavior(string behaviorId = DefaultBehaviorId)
        {
            BehaviorId = behaviorId;
        }

        public string BehaviorId { get; }

        public CombatBehaviorDecision DecideIntent(CombatBehaviorContext context)
        {
            return new CombatBehaviorDecision(BehaviorId, CombatBehaviorIntent.EndTurn(context.ActingUnitId));
        }

        public EnemyIntentSnapshot DescribeIntent(CombatBehaviorContext context)
        {
            return EnemyIntentSummaryBuilder.BuildForIntent(
                context,
                CombatBehaviorIntent.EndTurn(context.ActingUnitId));
        }
    }
}
