using System;
using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatRuntimeControlSurfaceModel
    {
        private readonly IReadOnlyList<CombatRuntimeAbilityButtonModel> abilityButtons;

        public CombatRuntimeControlSurfaceModel(
            bool canMove,
            bool canEndTurn,
            int remainingPlayerActions,
            IReadOnlyList<CombatRuntimeAbilityButtonModel> abilityButtons)
        {
            if (abilityButtons == null)
            {
                throw new ArgumentNullException(nameof(abilityButtons));
            }

            this.abilityButtons = abilityButtons;
            CanMove = canMove;
            CanEndTurn = canEndTurn;
            RemainingPlayerActions = remainingPlayerActions;
        }

        public bool CanMove { get; }

        public bool CanEndTurn { get; }

        public int RemainingPlayerActions { get; }

        public IReadOnlyList<CombatRuntimeAbilityButtonModel> AbilityButtons
        {
            get { return abilityButtons; }
        }
    }
}
