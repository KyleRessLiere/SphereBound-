using System;
using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatUnitDefinition
    {
        private readonly IReadOnlyList<AbilityDefinition> abilities;

        public CombatUnitDefinition(
            string id,
            string name,
            int baseHealth,
            int actionsPerTurn,
            MovementCapabilityDefinition movement,
            IEnumerable<AbilityDefinition> abilities,
            string? defaultAttackAbilityId = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Unit definition id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Unit definition name is required.", nameof(name));
            }

            if (baseHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseHealth), "Base health must be positive.");
            }

            if (actionsPerTurn <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(actionsPerTurn), "Actions per turn must be positive.");
            }

            Id = id;
            Name = name;
            BaseHealth = baseHealth;
            ActionsPerTurn = actionsPerTurn;
            Movement = movement ?? throw new ArgumentNullException(nameof(movement));
            if (abilities == null)
            {
                throw new ArgumentNullException(nameof(abilities));
            }

            this.abilities = new List<AbilityDefinition>(abilities).AsReadOnly();
            DefaultAttackAbilityId = defaultAttackAbilityId;
        }

        public string Id { get; }

        public string Name { get; }

        public int BaseHealth { get; }

        public int ActionsPerTurn { get; }

        public MovementCapabilityDefinition Movement { get; }

        public IReadOnlyList<AbilityDefinition> Abilities
        {
            get { return abilities; }
        }

        public string? DefaultAttackAbilityId { get; }

        public bool TryGetAbility(string abilityId, out AbilityDefinition ability)
        {
            for (var index = 0; index < abilities.Count; index += 1)
            {
                var candidate = abilities[index];
                if (candidate.Id == abilityId)
                {
                    ability = candidate;
                    return true;
                }
            }

            ability = null!;
            return false;
        }
    }
}
