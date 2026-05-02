using System;
using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatRuntimeAbilityButtonModel
    {
        private readonly IReadOnlyList<GridPosition> resolvedEffectTiles;

        public CombatRuntimeAbilityButtonModel(
            string abilityId,
            string name,
            string description,
            int actionCost,
            IEnumerable<GridPosition> resolvedEffectTiles,
            int actingUnitId,
            int? targetUnitId,
            GridPosition? targetPosition,
            bool isInteractable)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                throw new ArgumentException("Ability id is required.", nameof(abilityId));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Ability name is required.", nameof(name));
            }

            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (resolvedEffectTiles == null)
            {
                throw new ArgumentNullException(nameof(resolvedEffectTiles));
            }

            AbilityId = abilityId;
            Name = name;
            Description = description;
            ActionCost = actionCost;
            this.resolvedEffectTiles = new List<GridPosition>(resolvedEffectTiles).AsReadOnly();
            ActingUnitId = actingUnitId;
            TargetUnitId = targetUnitId;
            TargetPosition = targetPosition;
            IsInteractable = isInteractable;
        }

        public string AbilityId { get; }

        public string Name { get; }

        public string Description { get; }

        public int ActionCost { get; }

        public int ActingUnitId { get; }

        public int? TargetUnitId { get; }

        public GridPosition? TargetPosition { get; }

        public IReadOnlyList<GridPosition> ResolvedEffectTiles
        {
            get { return resolvedEffectTiles; }
        }

        public bool IsInteractable { get; }

        public string ResolvedEffectTileText
        {
            get
            {
                if (resolvedEffectTiles.Count == 0)
                {
                    return "none";
                }

                return string.Join(", ", resolvedEffectTiles);
            }
        }
    }
}
