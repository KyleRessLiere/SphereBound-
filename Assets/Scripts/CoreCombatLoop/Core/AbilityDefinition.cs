using System;
using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class AbilityDefinition
    {
        private readonly IReadOnlyList<AbilityEffectDefinition> effects;

        public AbilityDefinition(
            string id,
            string name,
            string description,
            CombatActionType actionType,
            int actionCost,
            AbilityTargetingMode targetingMode,
            AbilityTargetRule targetRule,
            AbilityTilePattern tilePattern,
            IEnumerable<AbilityEffectDefinition> effects,
            bool requiresAffectedUnit = true)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Ability id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Ability name is required.", nameof(name));
            }

            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (actionCost <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(actionCost), "Ability action cost must be positive.");
            }

            if (tilePattern == null)
            {
                throw new ArgumentNullException(nameof(tilePattern));
            }

            if (effects == null)
            {
                throw new ArgumentNullException(nameof(effects));
            }

            Id = id;
            Name = name;
            Description = description;
            ActionType = actionType;
            ActionCost = actionCost;
            TargetingMode = targetingMode;
            TargetRule = targetRule;
            TilePattern = tilePattern;
            this.effects = new List<AbilityEffectDefinition>(effects).AsReadOnly();
            if (this.effects.Count == 0)
            {
                throw new ArgumentException("Ability must define at least one effect payload.", nameof(effects));
            }

            RequiresAffectedUnit = requiresAffectedUnit;
        }

        public string Id { get; }

        public string Name { get; }

        public string Description { get; }

        public CombatActionType ActionType { get; }

        public int ActionCost { get; }

        public AbilityTargetingMode TargetingMode { get; }

        public AbilityTargetRule TargetRule { get; }

        public AbilityTilePattern TilePattern { get; }

        public IReadOnlyList<AbilityEffectDefinition> Effects
        {
            get { return effects; }
        }

        public bool RequiresAffectedUnit { get; }
    }
}
