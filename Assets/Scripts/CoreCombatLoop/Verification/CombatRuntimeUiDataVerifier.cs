using System;
using System.Collections.Generic;
using System.Linq;
using Spherebound.CoreCombatLoop.Core;
using Spherebound.CoreCombatLoop.UnityBridge;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class CombatRuntimeUiDataVerifier
    {
        public static IReadOnlyList<string> RunAll()
        {
            var completedChecks = new List<string>();

            VerifyPlayerDefinitionExposesMultipleUiAbilities(completedChecks);
            VerifyControlSurfaceIncludesAbilityMetadata(completedChecks);
            VerifyControlSurfaceIncludesResolvedEffectTiles(completedChecks);
            VerifyUiAbilityRequestUsesDefinitionBackedAbility(completedChecks);
            VerifyAbilitySelectionCycling(completedChecks);
            VerifyEnemyIntentPanelModelIncludesLivingEnemies(completedChecks);

            return completedChecks;
        }

        private static void VerifyPlayerDefinitionExposesMultipleUiAbilities(List<string> completedChecks)
        {
            Ensure(CombatScenarioFactory.PlayerDefinition.Abilities.Count >= 3, "Player definition should expose multiple test abilities for the runtime UI.");
            Ensure(CombatScenarioFactory.PlayerDefinition.Abilities.Any(ability => ability.Id == CombatScenarioFactory.ForwardLineAbilityId), "Player definition should expose Forward Line.");
            Ensure(CombatScenarioFactory.PlayerDefinition.Abilities.Any(ability => ability.Id == CombatScenarioFactory.FrontCrossAbilityId), "Player definition should expose Front Cross.");
            completedChecks.Add(nameof(VerifyPlayerDefinitionExposesMultipleUiAbilities));
        }

        private static void VerifyControlSurfaceIncludesAbilityMetadata(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();
            var model = CombatRuntimeControlSurfaceBuilder.Build(state);

            Ensure(model.AbilityButtons.Count >= 3, "Runtime control surface should expose multiple ability buttons.");
            var forwardLineButton = model.AbilityButtons.Single(button => button.AbilityId == CombatScenarioFactory.ForwardLineAbilityId);
            Ensure(forwardLineButton.Name == "Forward Line", "Ability button should expose definition-backed name.");
            Ensure(forwardLineButton.Description == "two tiles forward", "Ability button should expose definition-backed description.");
            Ensure(forwardLineButton.ActionCost == 1, "Ability button should expose action cost.");
            completedChecks.Add(nameof(VerifyControlSurfaceIncludesAbilityMetadata));
        }

        private static void VerifyControlSurfaceIncludesResolvedEffectTiles(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();
            var model = CombatRuntimeControlSurfaceBuilder.Build(state);

            var forwardLineButton = model.AbilityButtons.Single(button => button.AbilityId == CombatScenarioFactory.ForwardLineAbilityId);
            Ensure(forwardLineButton.ResolvedEffectTiles.Count == 2, "Forward Line should expose two resolved effect tiles.");
            Ensure(forwardLineButton.ResolvedEffectTileText.Contains("(2, 1)"), "Resolved effect tile text should include the first forward tile.");

            var frontCrossButton = model.AbilityButtons.Single(button => button.AbilityId == CombatScenarioFactory.FrontCrossAbilityId);
            Ensure(frontCrossButton.ResolvedEffectTiles.Count >= 3, "Front Cross should expose multiple resolved effect tiles.");
            completedChecks.Add(nameof(VerifyControlSurfaceIncludesResolvedEffectTiles));
        }

        private static void VerifyUiAbilityRequestUsesDefinitionBackedAbility(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();
            Ensure(state.TryGetUnit(CombatScenarioFactory.PlayerUnitId, out var player), "Initial state should contain a player.");
            Ensure(CombatScenarioFactory.PlayerDefinition.TryGetAbility(CombatScenarioFactory.ForwardLineAbilityId, out var forwardLine), "Forward Line should exist on the player definition.");

            var request = CombatRuntimeAbilityRequestResolver.CreateRuntimeRequest(state, player, forwardLine);
            Ensure(request.AbilityId == CombatScenarioFactory.ForwardLineAbilityId, "Runtime request should preserve the definition-backed ability id.");
            Ensure(request.TargetPosition.HasValue, "Directional runtime request should expose a target position.");
            completedChecks.Add(nameof(VerifyUiAbilityRequestUsesDefinitionBackedAbility));
        }

        private static void VerifyAbilitySelectionCycling(List<string> completedChecks)
        {
            Ensure(CombatRuntimeAbilitySelection.ClampIndex(-2, 3) == 0, "Ability selection should clamp negative indexes to zero.");
            Ensure(CombatRuntimeAbilitySelection.ClampIndex(5, 3) == 2, "Ability selection should clamp oversized indexes to the final ability.");
            Ensure(CombatRuntimeAbilitySelection.Cycle(0, -1, 3) == 2, "Ability cycling should wrap backward from the first ability.");
            Ensure(CombatRuntimeAbilitySelection.Cycle(2, 1, 3) == 0, "Ability cycling should wrap forward from the last ability.");
            completedChecks.Add(nameof(VerifyAbilitySelectionCycling));
        }

        private static void VerifyEnemyIntentPanelModelIncludesLivingEnemies(List<string> completedChecks)
        {
            var state = CombatScenarioFactory.CreateInitialState();
            var intents = CombatEnemyIntentPanelBuilder.Build(state);
            Ensure(intents.Count == 1, "Enemy intent panel model should include the living default enemy.");
            Ensure(intents[0].EnemyDisplayName == "Enemy Grunt", "Enemy intent panel model should preserve display names.");
            completedChecks.Add(nameof(VerifyEnemyIntentPanelModelIncludesLivingEnemies));
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
