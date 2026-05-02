namespace Spherebound.CoreCombatLoop.Core
{
    public static class CombatScenarioFactory
    {
        public const string BasicAttackAbilityId = "basic-attack";
        public const string ForwardLineAbilityId = "forward-line";
        public const string FrontCrossAbilityId = "front-cross";
        public const int PlayerUnitId = 1;
        public const int EnemyUnitId = 2;
        public const int BoardWidth = 6;
        public const int BoardHeight = 6;
        public const int PlayerStartingHealth = 5;
        public const int EnemyStartingHealth = 3;
        public const int PlayerActionsPerTurn = 2;
        public const string ManualPlayerBehaviorId = "player-manual";
        public const string EnemyMoveTowardPlayerBehaviorId = "enemy-move-toward-player";
        private static readonly MovementCapabilityDefinition DefaultMovementCapability = new MovementCapabilityDefinition(range: 1, actionCost: 1, orthogonalOnly: true);
        private static readonly AbilityDefinition BasicAttackAbility = new AbilityDefinition(
            BasicAttackAbilityId,
            "Basic Attack",
            "adjacent enemy",
            CombatActionType.Attack,
            actionCost: 1,
            AbilityTargetingMode.AdjacentUnit,
            AbilityTargetRule.Enemy | AbilityTargetRule.OccupiedTile,
            new AbilityTilePattern(
                AbilityTilePatternAnchor.TargetPosition,
                new[]
                {
                    new GridOffset(0, 0),
                }),
            new[]
            {
                AbilityEffectDefinition.Damage(1),
            });
        private static readonly AbilityDefinition ForwardLineAbility = new AbilityDefinition(
            ForwardLineAbilityId,
            "Forward Line",
            "two tiles forward",
            CombatActionType.Attack,
            actionCost: 1,
            AbilityTargetingMode.Line,
            AbilityTargetRule.Enemy | AbilityTargetRule.OccupiedTile,
            new AbilityTilePattern(
                AbilityTilePatternAnchor.DirectionFromActorToTarget,
                new[]
                {
                    new GridOffset(0, 1),
                    new GridOffset(0, 2),
                }),
            new[]
            {
                AbilityEffectDefinition.Damage(1),
            });
        private static readonly AbilityDefinition FrontCrossAbility = new AbilityDefinition(
            FrontCrossAbilityId,
            "Front Cross",
            "cross in front of player",
            CombatActionType.Attack,
            actionCost: 1,
            AbilityTargetingMode.DirectionalShape,
            AbilityTargetRule.Enemy | AbilityTargetRule.OccupiedTile,
            new AbilityTilePattern(
                AbilityTilePatternAnchor.DirectionFromActorToTarget,
                new[]
                {
                    new GridOffset(0, 1),
                    new GridOffset(-1, 1),
                    new GridOffset(1, 1),
                    new GridOffset(0, 2),
                }),
            new[]
            {
                AbilityEffectDefinition.Damage(1),
            });
        private static readonly CombatUnitDefinition PlayerDefinitionValue = new CombatUnitDefinition(
            "player-warrior",
            "Player Warrior",
            PlayerStartingHealth,
            PlayerActionsPerTurn,
            DefaultMovementCapability,
            new[]
            {
                BasicAttackAbility,
                ForwardLineAbility,
                FrontCrossAbility,
            },
            BasicAttackAbilityId);
        private static readonly CombatUnitDefinition EnemyDefinitionValue = new CombatUnitDefinition(
            "enemy-grunt",
            "Enemy Grunt",
            EnemyStartingHealth,
            actionsPerTurn: 1,
            DefaultMovementCapability,
            new[]
            {
                BasicAttackAbility,
            },
            BasicAttackAbilityId);

        public static CombatUnitDefinition PlayerDefinition
        {
            get { return PlayerDefinitionValue; }
        }

        public static CombatUnitDefinition EnemyDefinition
        {
            get { return EnemyDefinitionValue; }
        }

        public static CombatUnitDefinition GetDefaultDefinition(CombatUnitSide side)
        {
            return side == CombatUnitSide.Player ? PlayerDefinitionValue : EnemyDefinitionValue;
        }

        public static CombatState CreateInitialState()
        {
            return new CombatState(
                new BoardDimensions(BoardWidth, BoardHeight),
                CombatTurnSide.Player,
                PlayerActionsPerTurn,
                new[]
                {
                    new CombatUnitState(
                        PlayerUnitId,
                        CombatUnitSide.Player,
                        PlayerStartingHealth,
                        new GridPosition(1, 1),
                        UnitLifeState.Alive,
                        PlayerDefinitionValue,
                        CombatBehaviorAssignment.Manual(new ManualCombatBehavior(ManualPlayerBehaviorId))),
                    new CombatUnitState(
                        EnemyUnitId,
                        CombatUnitSide.Enemy,
                        EnemyStartingHealth,
                        new GridPosition(4, 4),
                        UnitLifeState.Alive,
                        EnemyDefinitionValue,
                        CombatBehaviorAssignment.Default(new MoveTowardTargetBehavior(EnemyMoveTowardPlayerBehaviorId, PlayerUnitId))),
                });
        }
    }
}
