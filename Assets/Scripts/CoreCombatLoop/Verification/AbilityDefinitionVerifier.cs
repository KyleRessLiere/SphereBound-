using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class AbilityDefinitionVerifier
    {
        public static IReadOnlyList<string> RunAll()
        {
            var completedChecks = new List<string>();

            VerifyPlayerInitializationFromDefinition(completedChecks);
            VerifyBasicAttackAbilityDefinition(completedChecks);
            VerifyAbilityCostHandling(completedChecks);
            VerifyDeterministicAffectedTileResolution(completedChecks);
            VerifyAbilityDamageFlow(completedChecks);
            VerifyForcedMovementFlow(completedChecks);

            return completedChecks;
        }

        private static void VerifyPlayerInitializationFromDefinition(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();
            Ensure(state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player), "Initial scenario should contain a player.");
            Ensure(player.Definition.Id == CombatScenarioFactory.PlayerDefinition.Id, "Player should be created from the reusable player definition.");
            Ensure(player.Definition.Abilities.Count > 0, "Player definition should expose abilities.");
            Ensure(player.Definition.Movement.Range == 1, "Player definition should expose movement capability metadata.");
            completedChecks.Add(nameof(VerifyPlayerInitializationFromDefinition));
        }

        private static void VerifyBasicAttackAbilityDefinition(List<string> completedChecks)
        {
            Ensure(CombatScenarioFactory.PlayerDefinition.TryGetAbility(CombatScenarioFactory.BasicAttackAbilityId, out var basicAttack), "Player definition should expose the basic attack ability.");
            Ensure(basicAttack.ActionCost == 1, "Basic attack ability should cost one action.");
            Ensure(basicAttack.TargetingMode == AbilityTargetingMode.AdjacentUnit, "Basic attack should use adjacent-unit targeting.");
            Ensure(basicAttack.Effects.Any(effect => effect.Kind == AbilityEffectKind.Damage && effect.Amount == 1), "Basic attack should deal one damage.");
            completedChecks.Add(nameof(VerifyBasicAttackAbilityDefinition));
        }

        private static void VerifyAbilityCostHandling(List<string> completedChecks)
        {
            const string expensiveAbilityId = "heavy-slash";
            var expensiveAbility = new AbilityDefinition(
                expensiveAbilityId,
                "Heavy Slash",
                CombatActionType.Attack,
                actionCost: 2,
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
                    AbilityEffectDefinition.Damage(2),
                });

            var playerDefinition = new CombatUnitDefinition(
                "heavy-fighter",
                "Heavy Fighter",
                baseHealth: 5,
                actionsPerTurn: 2,
                new MovementCapabilityDefinition(range: 1, actionCost: 1, orthogonalOnly: true),
                new[]
                {
                    expensiveAbility,
                },
                expensiveAbilityId);

            var state = CreateState(
                playerDefinition,
                playerHealth: 5,
                enemyHealth: 3,
                playerPosition: new GridPosition(1, 1),
                enemyPosition: new GridPosition(1, 2),
                remainingPlayerActions: 1);

            var engine = new CombatEngine();
            var result = engine.ResolveAbility(state, new AbilityUseRequest(CombatScenarioFactory.PlayerUnitId, expensiveAbilityId, CombatScenarioFactory.EnemyUnitId));

            Ensure(!result.Succeeded, "Ability should fail when the player lacks enough actions to pay its cost.");
            Ensure(result.FailureReason == CombatFailureReason.NoActionsRemaining, "Insufficient actions should fail with NoActionsRemaining.");
            Ensure(state.RemainingPlayerActions == 1, "Failed ability should not spend actions.");
            completedChecks.Add(nameof(VerifyAbilityCostHandling));
        }

        private static void VerifyDeterministicAffectedTileResolution(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();
            Ensure(state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player), "Initial scenario should contain a player.");
            Ensure(CombatScenarioFactory.PlayerDefinition.TryGetAbility(CombatScenarioFactory.BasicAttackAbilityId, out var basicAttack), "Player definition should expose the basic attack ability.");

            var resolvedTiles = CombatAbilityResolver.ResolveAffectedTiles(
                state,
                player,
                basicAttack,
                new AbilityUseRequest(player.Id, basicAttack.Id, CombatScenarioFactory.EnemyUnitId),
                out var failureReason);

            Ensure(failureReason == CombatFailureReason.TargetNotAdjacent, "Out-of-range basic attack should fail deterministically before resolution.");

            var adjacentState = CreateState(
                CombatScenarioFactory.PlayerDefinition,
                playerHealth: 5,
                enemyHealth: 3,
                playerPosition: new GridPosition(1, 1),
                enemyPosition: new GridPosition(1, 2),
                remainingPlayerActions: 2);
            Ensure(adjacentState.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var adjacentPlayer), "Adjacent scenario should contain a player.");

            resolvedTiles = CombatAbilityResolver.ResolveAffectedTiles(
                adjacentState,
                adjacentPlayer,
                basicAttack,
                new AbilityUseRequest(adjacentPlayer.Id, basicAttack.Id, CombatScenarioFactory.EnemyUnitId),
                out failureReason);

            Ensure(failureReason == CombatFailureReason.None, "Adjacent basic attack should resolve affected tiles.");
            Ensure(resolvedTiles.Count == 1, "Basic attack should resolve exactly one affected tile.");
            Ensure(resolvedTiles[0].Position.X == 1 && resolvedTiles[0].Position.Y == 2, "Basic attack should resolve the target unit tile.");
            completedChecks.Add(nameof(VerifyDeterministicAffectedTileResolution));
        }

        private static void VerifyAbilityDamageFlow(List<string> completedChecks)
        {
            var state = CreateState(
                CombatScenarioFactory.PlayerDefinition,
                playerHealth: 5,
                enemyHealth: 3,
                playerPosition: new GridPosition(1, 1),
                enemyPosition: new GridPosition(1, 2),
                remainingPlayerActions: 2);
            var engine = new CombatEngine();

            var result = engine.ResolveAbility(
                state,
                new AbilityUseRequest(
                    CombatScenarioFactory.PlayerUnitId,
                    CombatScenarioFactory.BasicAttackAbilityId,
                    CombatScenarioFactory.EnemyUnitId));

            Ensure(result.Succeeded, "Basic attack ability should resolve successfully when adjacent.");
            Ensure(result.Events.Any(evt => evt is AbilityRequested), "Ability resolution should emit AbilityRequested.");
            Ensure(result.Events.Any(evt => evt is AttackRequested), "Basic attack should still emit AttackRequested.");
            Ensure(result.Events.Any(evt => evt is DamageRequested), "Ability damage should emit DamageRequested.");
            Ensure(result.Events.Any(evt => evt is UnitDamaged), "Ability damage should emit UnitDamaged.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy) && enemy.CurrentHealth == 2, "Ability damage should change enemy health through the core.");
            completedChecks.Add(nameof(VerifyAbilityDamageFlow));
        }

        private static void VerifyForcedMovementFlow(List<string> completedChecks)
        {
            const string pushAbilityId = "shield-bash";
            var pushAbility = new AbilityDefinition(
                pushAbilityId,
                "Shield Bash",
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
                    AbilityEffectDefinition.ForcedMovement(new GridOffset(0, 1)),
                });

            var playerDefinition = new CombatUnitDefinition(
                "shield-knight",
                "Shield Knight",
                baseHealth: 5,
                actionsPerTurn: 2,
                new MovementCapabilityDefinition(range: 1, actionCost: 1, orthogonalOnly: true),
                new[]
                {
                    pushAbility,
                },
                pushAbilityId);

            var state = CreateState(
                playerDefinition,
                playerHealth: 5,
                enemyHealth: 3,
                playerPosition: new GridPosition(1, 1),
                enemyPosition: new GridPosition(1, 2),
                remainingPlayerActions: 2);
            var engine = new CombatEngine();

            var result = engine.ResolveAbility(
                state,
                new AbilityUseRequest(
                    CombatScenarioFactory.PlayerUnitId,
                    pushAbilityId,
                    CombatScenarioFactory.EnemyUnitId));

            Ensure(result.Succeeded, "Forced-movement ability should resolve successfully for a valid adjacent target.");
            Ensure(result.Events.Any(evt => evt is MoveRequested moveRequested && moveRequested.UnitId == CombatScenarioFactory.EnemyUnitId), "Forced movement should emit MoveRequested for the affected unit.");
            Ensure(result.Events.Any(evt => evt is UnitMoved unitMoved && unitMoved.UnitId == CombatScenarioFactory.EnemyUnitId), "Forced movement should emit UnitMoved for the affected unit.");
            Ensure(state.TryGetUnit(CombatScenarioFactory.EnemyUnitId, out var enemy), "Enemy should remain present after a non-lethal forced-movement ability.");
            Ensure(enemy.Position.X == 1 && enemy.Position.Y == 3, "Forced movement should update the affected enemy position.");
            completedChecks.Add(nameof(VerifyForcedMovementFlow));
        }

        private static CombatState CreateState(
            CombatUnitDefinition playerDefinition,
            int playerHealth,
            int enemyHealth,
            GridPosition playerPosition,
            GridPosition enemyPosition,
            int remainingPlayerActions)
        {
            return new CombatState(
                new BoardDimensions(6, 6),
                CombatTurnSide.Player,
                remainingPlayerActions,
                new[]
                {
                    new CombatUnitState(
                        CombatScenarioFactory.PlayerUnitId,
                        CombatUnitSide.Player,
                        playerHealth,
                        playerPosition,
                        UnitLifeState.Alive,
                        playerDefinition),
                    new CombatUnitState(
                        CombatScenarioFactory.EnemyUnitId,
                        CombatUnitSide.Enemy,
                        enemyHealth,
                        enemyPosition,
                        UnitLifeState.Alive,
                        CombatScenarioFactory.EnemyDefinition),
                });
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
