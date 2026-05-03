using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;
using Spherebound.CoreCombatLoop.Scenarios;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class ScenarioAbilityBehaviorValidationCatalog
    {
        private const int ShapeLeftEnemyUnitId = 3;
        private const int ShapeRightEnemyUnitId = 4;
        private const int ShapeForwardEnemyUnitId = 5;
        private const int ShapeDiagonalSafeEnemyUnitId = 6;
        private const string PushAbilityId = "validation-push";

        public static IReadOnlyList<ScenarioAbilityBehaviorValidationCase> All()
        {
            return new[]
            {
                CreateBasicAttackHitCase(),
                CreateBasicAttackMissCase(),
                CreateAbilityShapeValidationCase(),
                CreateMultiTurnCombatResolutionCase(),
                CreateBehaviorMovementInteractionCase(),
                CreateForcedMovementValidationCase(),
            };
        }

        private static ScenarioAbilityBehaviorValidationCase CreateBasicAttackHitCase()
        {
            var expectedEvents = new[]
            {
                nameof(TurnStarted),
                nameof(BehaviorIntentSelected),
                nameof(AbilityRequested),
                nameof(ActionStarted),
                nameof(AttackRequested),
                nameof(DamageRequested),
                nameof(UnitDamaged),
                nameof(ActionSpent),
                nameof(ActionEnded),
                nameof(BehaviorIntentSelected),
                nameof(TurnEnded),
                nameof(TurnStarted),
                nameof(BehaviorIntentSelected),
                nameof(TurnEnded),
                nameof(TurnStarted),
            };

            return new ScenarioAbilityBehaviorValidationCase(
                "VerifyBasicAttackHitScenario",
                new ScenarioDefinition(
                    "validation-basic-attack-hit",
                    ScenarioAbilityBehaviorValidationVerifier.BasicAttackHitScenarioName,
                    () => CreateBehaviorState(
                        new GridPosition(1, 1),
                        5,
                        new GridPosition(1, 2),
                        3,
                        CombatBehaviorIntent.UseAbility(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.BasicAttackAbilityId, CombatScenarioFactory.EnemyUnitId),
                        CombatBehaviorIntent.EndTurn(CombatScenarioFactory.PlayerUnitId),
                        enemyBehavior: new PassTurnBehavior("enemy-pass-hit")),
                    new[]
                    {
                        ScenarioStep.StartCombat(),
                        ScenarioStep.RunBehaviorTurnCycle(),
                    },
                    new[]
                    {
                        new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 5, expectedPosition: new GridPosition(1, 1), expectedLifeState: UnitLifeState.Alive),
                        new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedHealth: 2, expectedPosition: new GridPosition(1, 2), expectedLifeState: UnitLifeState.Alive),
                    },
                    true,
                    expectedEvents),
                ScenarioEventValidationMode.ExactSequence,
                expectedEvents);
        }

        private static ScenarioAbilityBehaviorValidationCase CreateBasicAttackMissCase()
        {
            var expectedEvents = new[]
            {
                nameof(TurnStarted),
                nameof(AbilityRequested),
                nameof(ActionStarted),
                nameof(ActionFailed),
                nameof(ActionSpent),
                nameof(ActionEnded),
            };

            return new ScenarioAbilityBehaviorValidationCase(
                "VerifyBasicAttackMissScenario",
                new ScenarioDefinition(
                    "validation-basic-attack-miss",
                    ScenarioAbilityBehaviorValidationVerifier.BasicAttackMissScenarioName,
                    CombatScenarioFactory.CreateInitialState,
                    new[]
                    {
                        ScenarioStep.StartCombat(),
                        ScenarioStep.Attack(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId, "Player attempts an out-of-range attack"),
                    },
                    new[]
                    {
                        new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 5, expectedPosition: CombatScenarioFactory.PlayerStartingPosition, expectedLifeState: UnitLifeState.Alive),
                        new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedHealth: 3, expectedPosition: CombatScenarioFactory.EnemyStartingPosition, expectedLifeState: UnitLifeState.Alive),
                    },
                    false,
                    expectedEvents),
                ScenarioEventValidationMode.ExactSequence,
                expectedEvents,
                ValidateMissDoesNotDamage);
        }

        private static ScenarioAbilityBehaviorValidationCase CreateAbilityShapeValidationCase()
        {
            var expectedEvents = new[]
            {
                nameof(TurnStarted),
                nameof(BehaviorIntentSelected),
                nameof(AbilityRequested),
                nameof(ActionStarted),
                nameof(AttackRequested),
                nameof(DamageRequested),
                nameof(UnitDamaged),
                nameof(UnitDying),
                nameof(UnitDeath),
                nameof(UnitRemoved),
                nameof(AttackRequested),
                nameof(DamageRequested),
                nameof(UnitDamaged),
                nameof(UnitDying),
                nameof(UnitDeath),
                nameof(UnitRemoved),
                nameof(AttackRequested),
                nameof(DamageRequested),
                nameof(UnitDamaged),
                nameof(UnitDying),
                nameof(UnitDeath),
                nameof(UnitRemoved),
                nameof(AttackRequested),
                nameof(DamageRequested),
                nameof(UnitDamaged),
                nameof(UnitDying),
                nameof(UnitDeath),
                nameof(UnitRemoved),
                nameof(ActionSpent),
                nameof(ActionEnded),
                nameof(BehaviorIntentSelected),
                nameof(TurnEnded),
                nameof(TurnStarted),
                nameof(TurnEnded),
            };

            return new ScenarioAbilityBehaviorValidationCase(
                "VerifyAbilityShapeValidationScenario",
                new ScenarioDefinition(
                    "validation-ability-shape",
                    ScenarioAbilityBehaviorValidationVerifier.AbilityShapeValidationScenarioName,
                    CreateAbilityShapeState,
                    new[]
                    {
                        ScenarioStep.StartCombat(),
                        ScenarioStep.RunBehaviorTurnCycle(),
                    },
                    new[]
                    {
                        new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 5, expectedPosition: new GridPosition(2, 1), expectedLifeState: UnitLifeState.Alive),
                        new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedLifeState: UnitLifeState.Dead),
                        new ScenarioExpectation(ShapeLeftEnemyUnitId, expectedLifeState: UnitLifeState.Dead),
                        new ScenarioExpectation(ShapeRightEnemyUnitId, expectedLifeState: UnitLifeState.Dead),
                        new ScenarioExpectation(ShapeForwardEnemyUnitId, expectedLifeState: UnitLifeState.Dead),
                        new ScenarioExpectation(ShapeDiagonalSafeEnemyUnitId, expectedHealth: 1, expectedPosition: new GridPosition(1, 3), expectedLifeState: UnitLifeState.Alive),
                    },
                    true,
                    expectedEvents),
                ScenarioEventValidationMode.OrderedSubsequence,
                expectedEvents,
                ValidateShapeOnlyHitsExpectedTargets);
        }

        private static ScenarioAbilityBehaviorValidationCase CreateMultiTurnCombatResolutionCase()
        {
            var expectedEvents = new[]
            {
                nameof(TurnStarted),
                nameof(AbilityRequested),
                nameof(ActionStarted),
                nameof(AttackRequested),
                nameof(DamageRequested),
                nameof(UnitDamaged),
                nameof(ActionSpent),
                nameof(ActionEnded),
                nameof(TurnEnded),
                nameof(TurnStarted),
                nameof(BehaviorIntentSelected),
                nameof(TurnEnded),
                nameof(TurnStarted),
                nameof(AbilityRequested),
                nameof(ActionStarted),
                nameof(AttackRequested),
                nameof(DamageRequested),
                nameof(UnitDamaged),
                nameof(UnitDying),
                nameof(UnitDeath),
                nameof(UnitRemoved),
                nameof(ActionSpent),
                nameof(ActionEnded),
            };

            return new ScenarioAbilityBehaviorValidationCase(
                "VerifyMultiTurnCombatResolutionScenario",
                new ScenarioDefinition(
                    "validation-multi-turn-resolution",
                    ScenarioAbilityBehaviorValidationVerifier.MultiTurnCombatResolutionScenarioName,
                    () => CreatePlayerVsEnemyState(
                        new GridPosition(1, 1),
                        5,
                        new GridPosition(1, 2),
                        2,
                        playerBehavior: CombatBehaviorAssignment.Manual(new ManualCombatBehavior("player-manual-resolution")),
                        enemyBehavior: CombatBehaviorAssignment.Scenario(new PassTurnBehavior("enemy-pass-resolution"))),
                    new[]
                    {
                        ScenarioStep.StartCombat(),
                        ScenarioStep.Attack(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId, "First attack"),
                        ScenarioStep.EndPlayerTurn("Enemy passes and turn returns"),
                        ScenarioStep.Attack(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.EnemyUnitId, "Second attack kills"),
                    },
                    new[]
                    {
                        new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 5, expectedPosition: new GridPosition(1, 1), expectedLifeState: UnitLifeState.Alive),
                        new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedLifeState: UnitLifeState.Dead),
                    },
                    true,
                    expectedEvents),
                ScenarioEventValidationMode.OrderedSubsequence,
                expectedEvents,
                ValidateMultiTurnResolution);
        }

        private static ScenarioAbilityBehaviorValidationCase CreateBehaviorMovementInteractionCase()
        {
            var expectedEvents = new[]
            {
                nameof(TurnStarted),
                nameof(BehaviorIntentSelected),
                nameof(TurnEnded),
                nameof(TurnStarted),
                nameof(BehaviorIntentSelected),
                nameof(MoveRequested),
                nameof(ActionStarted),
                nameof(UnitMoved),
                nameof(ActionEnded),
                nameof(TurnEnded),
                nameof(TurnStarted),
                nameof(BehaviorIntentSelected),
                nameof(TurnEnded),
                nameof(TurnStarted),
                nameof(BehaviorIntentSelected),
                nameof(AbilityRequested),
                nameof(ActionStarted),
                nameof(AttackRequested),
                nameof(DamageRequested),
                nameof(UnitDamaged),
                nameof(ActionEnded),
            };

            return new ScenarioAbilityBehaviorValidationCase(
                "VerifyBehaviorMovementInteractionScenario",
                new ScenarioDefinition(
                    "validation-behavior-movement-interaction",
                    ScenarioAbilityBehaviorValidationVerifier.BehaviorMovementInteractionScenarioName,
                    () => CreatePlayerVsEnemyState(
                        new GridPosition(1, 1),
                        5,
                        new GridPosition(1, 3),
                        3,
                        playerBehavior: CombatBehaviorAssignment.Scenario(new PassTurnBehavior("player-pass-behavior-validation")),
                        enemyBehavior: CombatBehaviorAssignment.Scenario(new MoveTowardTargetBehavior("enemy-move-then-attack", CombatScenarioFactory.PlayerUnitId))),
                    new[]
                    {
                        ScenarioStep.StartCombat(),
                        ScenarioStep.RunBehaviorTurnCycle(),
                        ScenarioStep.RunBehaviorTurnCycle(),
                    },
                    new[]
                    {
                        new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 4, expectedPosition: new GridPosition(1, 1), expectedLifeState: UnitLifeState.Alive),
                        new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedHealth: 3, expectedPosition: new GridPosition(1, 2), expectedLifeState: UnitLifeState.Alive),
                    },
                    true,
                    expectedEvents),
                ScenarioEventValidationMode.OrderedSubsequence,
                expectedEvents,
                ValidateBehaviorMovesThenAttacks);
        }

        private static ScenarioAbilityBehaviorValidationCase CreateForcedMovementValidationCase()
        {
            var expectedEvents = new[]
            {
                nameof(TurnStarted),
                nameof(BehaviorIntentSelected),
                nameof(AbilityRequested),
                nameof(ActionStarted),
                nameof(MoveRequested),
                nameof(UnitMoved),
                nameof(ActionSpent),
                nameof(ActionEnded),
                nameof(BehaviorIntentSelected),
                nameof(TurnEnded),
                nameof(TurnStarted),
                nameof(BehaviorIntentSelected),
                nameof(TurnEnded),
                nameof(TurnStarted),
            };

            return new ScenarioAbilityBehaviorValidationCase(
                "VerifyForcedMovementValidationScenario",
                new ScenarioDefinition(
                    "validation-forced-movement",
                    ScenarioAbilityBehaviorValidationVerifier.ForcedMovementValidationScenarioName,
                    CreateForcedMovementState,
                    new[]
                    {
                        ScenarioStep.StartCombat(),
                        ScenarioStep.RunBehaviorTurnCycle(),
                    },
                    new[]
                    {
                        new ScenarioExpectation(CombatScenarioFactory.PlayerUnitId, expectedHealth: 5, expectedPosition: new GridPosition(1, 1), expectedLifeState: UnitLifeState.Alive),
                        new ScenarioExpectation(CombatScenarioFactory.EnemyUnitId, expectedHealth: 3, expectedPosition: new GridPosition(1, 3), expectedLifeState: UnitLifeState.Alive),
                    },
                    true,
                    expectedEvents),
                ScenarioEventValidationMode.ExactSequence,
                expectedEvents);
        }

        private static CombatState CreateAbilityShapeState()
        {
            return new CombatState(
                new BoardDimensions(CombatScenarioFactory.BoardWidth, CombatScenarioFactory.BoardHeight),
                CombatTurnSide.Player,
                CombatScenarioFactory.PlayerActionsPerTurn,
                new[]
                {
                    new CombatUnitState(
                        CombatScenarioFactory.PlayerUnitId,
                        CombatUnitSide.Player,
                        5,
                        new GridPosition(2, 1),
                        UnitLifeState.Alive,
                        CombatScenarioFactory.PlayerDefinition,
                        CombatBehaviorAssignment.Scenario(
                            new ScriptedCombatBehavior(
                                "player-front-cross-shape",
                                new[]
                                {
                                    CombatBehaviorIntent.UseAbility(CombatScenarioFactory.PlayerUnitId, CombatScenarioFactory.FrontCrossAbilityId, targetPosition: new GridPosition(2, 2)),
                                    CombatBehaviorIntent.EndTurn(CombatScenarioFactory.PlayerUnitId),
                                }))),
                    new CombatUnitState(
                        CombatScenarioFactory.EnemyUnitId,
                        CombatUnitSide.Enemy,
                        1,
                        new GridPosition(2, 2),
                        UnitLifeState.Alive,
                        CombatScenarioFactory.EnemyDefinition,
                        CombatBehaviorAssignment.Scenario(new PassTurnBehavior("enemy-pass-shape-primary"))),
                    new CombatUnitState(ShapeLeftEnemyUnitId, CombatUnitSide.Enemy, 1, new GridPosition(1, 2), UnitLifeState.Alive, CombatScenarioFactory.EnemyDefinition),
                    new CombatUnitState(ShapeRightEnemyUnitId, CombatUnitSide.Enemy, 1, new GridPosition(3, 2), UnitLifeState.Alive, CombatScenarioFactory.EnemyDefinition),
                    new CombatUnitState(ShapeForwardEnemyUnitId, CombatUnitSide.Enemy, 1, new GridPosition(2, 3), UnitLifeState.Alive, CombatScenarioFactory.EnemyDefinition),
                    new CombatUnitState(ShapeDiagonalSafeEnemyUnitId, CombatUnitSide.Enemy, 1, new GridPosition(1, 3), UnitLifeState.Alive, CombatScenarioFactory.EnemyDefinition),
                });
        }

        private static CombatState CreateForcedMovementState()
        {
            var pushAbility = new AbilityDefinition(
                PushAbilityId,
                "Validation Push",
                "push adjacent enemy",
                CombatActionType.Ability,
                actionCost: 1,
                AbilityTargetingMode.AdjacentUnit,
                AbilityTargetRule.Enemy | AbilityTargetRule.OccupiedTile,
                new AbilityTilePattern(AbilityTilePatternAnchor.TargetPosition, new[] { new GridOffset(0, 0) }),
                new[] { AbilityEffectDefinition.ForcedMovement(new GridOffset(0, 1)) });

            var playerDefinition = new CombatUnitDefinition(
                "validation-push-player",
                "Validation Push Player",
                CombatScenarioFactory.PlayerStartingHealth,
                CombatScenarioFactory.PlayerActionsPerTurn,
                CombatScenarioFactory.PlayerDefinition.Movement,
                new[]
                {
                    pushAbility,
                },
                PushAbilityId);

            return new CombatState(
                new BoardDimensions(CombatScenarioFactory.BoardWidth, CombatScenarioFactory.BoardHeight),
                CombatTurnSide.Player,
                CombatScenarioFactory.PlayerActionsPerTurn,
                new[]
                {
                    new CombatUnitState(
                        CombatScenarioFactory.PlayerUnitId,
                        CombatUnitSide.Player,
                        5,
                        new GridPosition(1, 1),
                        UnitLifeState.Alive,
                        playerDefinition,
                        CombatBehaviorAssignment.Scenario(
                            new ScriptedCombatBehavior(
                                "player-push-validation",
                                new[]
                                {
                                    CombatBehaviorIntent.UseAbility(CombatScenarioFactory.PlayerUnitId, PushAbilityId, CombatScenarioFactory.EnemyUnitId),
                                    CombatBehaviorIntent.EndTurn(CombatScenarioFactory.PlayerUnitId),
                                }))),
                    new CombatUnitState(
                        CombatScenarioFactory.EnemyUnitId,
                        CombatUnitSide.Enemy,
                        3,
                        new GridPosition(1, 2),
                        UnitLifeState.Alive,
                        CombatScenarioFactory.EnemyDefinition,
                        CombatBehaviorAssignment.Scenario(new PassTurnBehavior("enemy-pass-push"))),
                });
        }

        private static CombatState CreateBehaviorState(
            GridPosition playerPosition,
            int playerHealth,
            GridPosition enemyPosition,
            int enemyHealth,
            CombatBehaviorIntent firstPlayerIntent,
            CombatBehaviorIntent secondPlayerIntent,
            ICombatBehaviorDefinition enemyBehavior)
        {
            return CreatePlayerVsEnemyState(
                playerPosition,
                playerHealth,
                enemyPosition,
                enemyHealth,
                CombatBehaviorAssignment.Scenario(new ScriptedCombatBehavior("player-validation-script", new[] { firstPlayerIntent, secondPlayerIntent })),
                CombatBehaviorAssignment.Scenario(enemyBehavior));
        }

        private static CombatState CreatePlayerVsEnemyState(
            GridPosition playerPosition,
            int playerHealth,
            GridPosition enemyPosition,
            int enemyHealth,
            CombatBehaviorAssignment playerBehavior,
            CombatBehaviorAssignment enemyBehavior)
        {
            return new CombatState(
                new BoardDimensions(CombatScenarioFactory.BoardWidth, CombatScenarioFactory.BoardHeight),
                CombatTurnSide.Player,
                CombatScenarioFactory.PlayerActionsPerTurn,
                new[]
                {
                    new CombatUnitState(
                        CombatScenarioFactory.PlayerUnitId,
                        CombatUnitSide.Player,
                        playerHealth,
                        playerPosition,
                        UnitLifeState.Alive,
                        CombatScenarioFactory.PlayerDefinition,
                        playerBehavior),
                    new CombatUnitState(
                        CombatScenarioFactory.EnemyUnitId,
                        CombatUnitSide.Enemy,
                        enemyHealth,
                        enemyPosition,
                        UnitLifeState.Alive,
                        CombatScenarioFactory.EnemyDefinition,
                        enemyBehavior),
                });
        }

        private static void ValidateMissDoesNotDamage(ScenarioRunResult result, List<string> failures)
        {
            if (result.Events.Any(combatEvent => combatEvent is UnitDamaged || combatEvent is DamageRequested))
            {
                failures.Add("Basic Attack Miss should not emit damage events.");
            }
        }

        private static void ValidateShapeOnlyHitsExpectedTargets(ScenarioRunResult result, List<string> failures)
        {
            var damagedUnitIds = result.Events
                .OfType<UnitDamaged>()
                .Select(unitDamaged => unitDamaged.UnitId)
                .OrderBy(unitId => unitId)
                .ToArray();

            var expectedDamagedUnitIds = new[]
            {
                CombatScenarioFactory.EnemyUnitId,
                ShapeLeftEnemyUnitId,
                ShapeRightEnemyUnitId,
                ShapeForwardEnemyUnitId,
            }.OrderBy(unitId => unitId).ToArray();

            if (!damagedUnitIds.SequenceEqual(expectedDamagedUnitIds))
            {
                failures.Add("Ability Shape Validation should damage only the intended front-cross targets.");
            }
        }

        private static void ValidateMultiTurnResolution(ScenarioRunResult result, List<string> failures)
        {
            var damageEventCount = result.Events.Count(combatEvent => combatEvent is UnitDamaged);
            if (damageEventCount != 2)
            {
                failures.Add($"Multi-Turn Combat Resolution should apply damage exactly twice, but observed {damageEventCount} damage events.");
            }
        }

        private static void ValidateBehaviorMovesThenAttacks(ScenarioRunResult result, List<string> failures)
        {
            var behaviorIntents = result.Events.OfType<BehaviorIntentSelected>().ToList();
            if (behaviorIntents.Count < 4)
            {
                failures.Add("Behavior + Movement Interaction should emit the expected behavior decisions across two cycles.");
                return;
            }

            if (behaviorIntents[1].IntentType != CombatBehaviorIntentType.Move)
            {
                failures.Add("Behavior + Movement Interaction should move before attacking.");
            }

            if (behaviorIntents[3].IntentType != CombatBehaviorIntentType.UseAbility)
            {
                failures.Add("Behavior + Movement Interaction should switch to ability use once adjacent.");
            }
        }
    }
}
