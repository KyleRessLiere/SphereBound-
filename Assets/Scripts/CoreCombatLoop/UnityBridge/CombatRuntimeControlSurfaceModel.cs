using System;
using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatRuntimeControlSurfaceModel
    {
        private readonly IReadOnlyList<CombatRuntimeMoveButtonModel> moveButtons;
        private readonly IReadOnlyList<CombatRuntimeAbilityButtonModel> abilityButtons;

        public CombatRuntimeControlSurfaceModel(
            IEnumerable<CombatRuntimeMoveButtonModel> moveButtons,
            bool canEndTurn,
            IReadOnlyList<CombatRuntimeAbilityButtonModel> abilityButtons)
        {
            if (moveButtons == null)
            {
                throw new ArgumentNullException(nameof(moveButtons));
            }

            if (abilityButtons == null)
            {
                throw new ArgumentNullException(nameof(abilityButtons));
            }

            this.moveButtons = new List<CombatRuntimeMoveButtonModel>(moveButtons).AsReadOnly();
            this.abilityButtons = abilityButtons;
            CanEndTurn = canEndTurn;
        }

        public IReadOnlyList<CombatRuntimeMoveButtonModel> MoveButtons
        {
            get { return moveButtons; }
        }

        public bool CanEndTurn { get; }

        public IReadOnlyList<CombatRuntimeAbilityButtonModel> AbilityButtons
        {
            get { return abilityButtons; }
        }
    }
}
