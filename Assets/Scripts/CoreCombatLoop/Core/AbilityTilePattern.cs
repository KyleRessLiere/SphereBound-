using System;
using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class AbilityTilePattern
    {
        private readonly IReadOnlyList<GridOffset> offsets;

        public AbilityTilePattern(AbilityTilePatternAnchor anchor, IEnumerable<GridOffset> offsets)
        {
            if (offsets == null)
            {
                throw new ArgumentNullException(nameof(offsets));
            }

            Anchor = anchor;
            this.offsets = new List<GridOffset>(offsets).AsReadOnly();
            if (this.offsets.Count == 0)
            {
                throw new ArgumentException("Ability tile pattern must contain at least one offset.", nameof(offsets));
            }
        }

        public AbilityTilePatternAnchor Anchor { get; }

        public IReadOnlyList<GridOffset> Offsets
        {
            get { return offsets; }
        }
    }
}
