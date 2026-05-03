using System;
using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatRuntimeAbilityPreview
    {
        private readonly IReadOnlyList<GridPosition> highlightTiles;

        public CombatRuntimeAbilityPreview(
            CombatRuntimeAbilityButtonModel abilityButton,
            AbilityUseRequest request,
            GridPosition targetTile,
            IEnumerable<GridPosition> highlightTiles)
        {
            AbilityButton = abilityButton ?? throw new ArgumentNullException(nameof(abilityButton));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            TargetTile = targetTile;
            this.highlightTiles = new List<GridPosition>(highlightTiles ?? throw new ArgumentNullException(nameof(highlightTiles))).AsReadOnly();
        }

        public CombatRuntimeAbilityButtonModel AbilityButton { get; }

        public AbilityUseRequest Request { get; }

        public GridPosition TargetTile { get; }

        public IReadOnlyList<GridPosition> HighlightTiles => highlightTiles;
    }
}
