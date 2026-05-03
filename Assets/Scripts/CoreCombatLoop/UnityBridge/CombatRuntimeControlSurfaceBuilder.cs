using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public static class CombatRuntimeControlSurfaceBuilder
    {
        public static CombatRuntimeControlSurfaceModel Build(CombatState state)
        {
            if (!TryGetPlayerUnit(state, out var player))
            {
                return new CombatRuntimeControlSurfaceModel(canMove: false, canEndTurn: false, abilityButtons: new List<CombatRuntimeAbilityButtonModel>().AsReadOnly());
            }

            var canIssuePlayerCommands = player.IsAlive && state.ActiveTurn == CombatTurnSide.Player;
            var canMove = canIssuePlayerCommands && state.RemainingPlayerActions >= player.Definition.Movement.ActionCost;
            var abilityButtons = BuildAbilityButtons(state, player, canIssuePlayerCommands);

            return new CombatRuntimeControlSurfaceModel(canMove, canIssuePlayerCommands, abilityButtons);
        }

        private static IReadOnlyList<CombatRuntimeAbilityButtonModel> BuildAbilityButtons(CombatState state, CombatUnitState player, bool canIssuePlayerCommands)
        {
            var buttons = new List<CombatRuntimeAbilityButtonModel>();
            var abilities = player.Definition.Abilities;
            for (var index = 0; index < abilities.Count; index += 1)
            {
                var ability = abilities[index];
                var request = CombatRuntimeAbilityRequestResolver.CreateRuntimeRequest(state, player, ability);
                var resolvedTiles = CombatRuntimeAbilityRequestResolver.ResolveEffectTiles(state, player, ability);
                var isInteractable = canIssuePlayerCommands && state.RemainingPlayerActions >= ability.ActionCost;
                buttons.Add(new CombatRuntimeAbilityButtonModel(
                    ability.Id,
                    ability.Name,
                    ability.Description,
                    ability.ActionCost,
                    resolvedTiles,
                    player.Id,
                    request.TargetUnitId,
                    request.TargetPosition,
                    isInteractable));
            }

            return buttons.AsReadOnly();
        }

        private static bool TryGetPlayerUnit(CombatState state, out CombatUnitState player)
        {
            foreach (var unitEntry in state.UnitsById)
            {
                if (unitEntry.Value.Side == CombatUnitSide.Player)
                {
                    player = unitEntry.Value;
                    return true;
                }
            }

            player = null!;
            return false;
        }
    }
}
