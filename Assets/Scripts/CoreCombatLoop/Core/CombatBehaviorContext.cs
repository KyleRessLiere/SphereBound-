using System;
using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatBehaviorContext
    {
        private readonly IReadOnlyList<CombatBehaviorUnitView> units;
        private readonly Dictionary<int, CombatBehaviorUnitView> unitsById;
        private readonly Dictionary<GridPosition, CombatBehaviorUnitView> unitsByPosition;

        private CombatBehaviorContext(
            BoardDimensions board,
            CombatTurnSide activeTurn,
            int actingUnitId,
            int remainingActions,
            IReadOnlyList<CombatBehaviorUnitView> units,
            Dictionary<int, CombatBehaviorUnitView> unitsById,
            Dictionary<GridPosition, CombatBehaviorUnitView> unitsByPosition)
        {
            Board = board;
            ActiveTurn = activeTurn;
            ActingUnitId = actingUnitId;
            RemainingActions = remainingActions;
            this.units = units;
            this.unitsById = unitsById;
            this.unitsByPosition = unitsByPosition;
        }

        public BoardDimensions Board { get; }

        public CombatTurnSide ActiveTurn { get; }

        public int ActingUnitId { get; }

        public int RemainingActions { get; }

        public IReadOnlyList<CombatBehaviorUnitView> Units
        {
            get { return units; }
        }

        public static CombatBehaviorContext FromState(CombatState state, int actingUnitId)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var views = new List<CombatBehaviorUnitView>();
            var unitsById = new Dictionary<int, CombatBehaviorUnitView>();
            var unitsByPosition = new Dictionary<GridPosition, CombatBehaviorUnitView>();

            foreach (var unit in state.UnitsById.Values)
            {
                var view = new CombatBehaviorUnitView(
                    unit.Id,
                    unit.Side,
                    unit.Position,
                    unit.CurrentHealth,
                    unit.IsAlive,
                    unit.Definition);

                views.Add(view);
                unitsById[view.UnitId] = view;
                if (view.IsAlive)
                {
                    unitsByPosition[view.Position] = view;
                }
            }

            var remainingActions = state.ActiveTurn == CombatTurnSide.Player
                ? state.RemainingPlayerActions
                : 1;

            return new CombatBehaviorContext(
                state.Board,
                state.ActiveTurn,
                actingUnitId,
                remainingActions,
                views.AsReadOnly(),
                unitsById,
                unitsByPosition);
        }

        public bool TryGetActingUnit(out CombatBehaviorUnitView unit)
        {
            if (unitsById.TryGetValue(ActingUnitId, out var foundUnit))
            {
                unit = foundUnit;
                return true;
            }

            unit = null!;
            return false;
        }

        public bool TryGetUnit(int unitId, out CombatBehaviorUnitView unit)
        {
            if (unitsById.TryGetValue(unitId, out var foundUnit))
            {
                unit = foundUnit;
                return true;
            }

            unit = null!;
            return false;
        }

        public bool TryGetUnitAtPosition(GridPosition position, out CombatBehaviorUnitView unit)
        {
            if (unitsByPosition.TryGetValue(position, out var foundUnit))
            {
                unit = foundUnit;
                return true;
            }

            unit = null!;
            return false;
        }
    }
}
