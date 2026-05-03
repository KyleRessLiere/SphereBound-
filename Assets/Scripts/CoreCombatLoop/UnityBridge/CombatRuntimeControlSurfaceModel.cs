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
            IReadOnlyList<CombatRuntimeAbilityButtonModel> abilityButtons)
        {
            if (abilityButtons == null)
            {
                throw new ArgumentNullException(nameof(abilityButtons));
            }

            this.abilityButtons = abilityButtons;
            CanMove = canMove;
            CanEndTurn = canEndTurn;
        }

        public bool CanMove { get; }

        public bool CanEndTurn { get; }

        public IReadOnlyList<CombatRuntimeAbilityButtonModel> AbilityButtons
        {
            get { return abilityButtons; }
        }
    }
}
