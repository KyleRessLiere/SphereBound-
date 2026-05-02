using System;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class AbilityEffectDefinition
    {
        private AbilityEffectDefinition(AbilityEffectKind kind, int amount, GridOffset forcedMovementOffset)
        {
            Kind = kind;
            Amount = amount;
            ForcedMovementOffset = forcedMovementOffset;
        }

        public AbilityEffectKind Kind { get; }

        public int Amount { get; }

        public GridOffset ForcedMovementOffset { get; }

        public static AbilityEffectDefinition Damage(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage amount must be positive.");
            }

            return new AbilityEffectDefinition(AbilityEffectKind.Damage, amount, new GridOffset(0, 0));
        }

        public static AbilityEffectDefinition Healing(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Healing amount must be positive.");
            }

            return new AbilityEffectDefinition(AbilityEffectKind.Healing, amount, new GridOffset(0, 0));
        }

        public static AbilityEffectDefinition ForcedMovement(GridOffset forcedMovementOffset)
        {
            return new AbilityEffectDefinition(AbilityEffectKind.ForcedMovement, 0, forcedMovementOffset);
        }
    }
}
