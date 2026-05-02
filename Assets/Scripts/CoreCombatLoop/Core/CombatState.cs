using System;
using System.Collections.Generic;

namespace Spherebound.CoreCombatLoop.Core
{
    public sealed class CombatState
    {
        private readonly Dictionary<int, CombatUnitState> unitsById;

        public CombatState(BoardDimensions board, CombatTurnSide activeTurn, int remainingPlayerActions, IEnumerable<CombatUnitState> units)
        {
            if (remainingPlayerActions < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remainingPlayerActions), "Remaining player actions cannot be negative.");
            }

            if (units == null)
            {
                throw new ArgumentNullException(nameof(units));
            }

            Board = board;
            ActiveTurn = activeTurn;
            RemainingPlayerActions = remainingPlayerActions;
            unitsById = new Dictionary<int, CombatUnitState>();

            foreach (var unit in units)
            {
                if (unit == null)
                {
                    throw new ArgumentException("Combat state cannot contain a null unit.", nameof(units));
                }

                if (!Board.Contains(unit.Position))
                {
                    throw new ArgumentException($"Unit {unit.Id} has an out-of-bounds position {unit.Position}.", nameof(units));
                }

                if (unitsById.ContainsKey(unit.Id))
                {
                    throw new ArgumentException($"Duplicate unit id {unit.Id} is not allowed.", nameof(units));
                }

                if (TryGetUnitAtPosition(unit.Position, out var occupyingUnit))
                {
                    throw new ArgumentException(
                        $"Units {occupyingUnit.Id} and {unit.Id} cannot share the same position {unit.Position}.",
                        nameof(units));
                }

                unitsById.Add(unit.Id, unit);
            }

            PlayerActionsPerTurn = DeterminePlayerActionsPerTurn();
        }

        public BoardDimensions Board { get; }

        public CombatTurnSide ActiveTurn { get; set; }

        public int RemainingPlayerActions { get; set; }

        public int PlayerActionsPerTurn { get; }

        public IReadOnlyDictionary<int, CombatUnitState> UnitsById
        {
            get { return unitsById; }
        }

        public bool ContainsUnit(int unitId)
        {
            return unitsById.ContainsKey(unitId);
        }

        public bool TryGetUnit(int unitId, out CombatUnitState unit)
        {
            if (unitsById.TryGetValue(unitId, out var foundUnit))
            {
                unit = foundUnit;
                return true;
            }

            unit = null!;
            return false;
        }

        public bool TryGetUnitAtPosition(GridPosition position, out CombatUnitState unit)
        {
            foreach (var entry in unitsById)
            {
                var candidate = entry.Value;
                if (candidate.Position.X == position.X && candidate.Position.Y == position.Y)
                {
                    unit = candidate;
                    return true;
                }
            }

            unit = null!;
            return false;
        }

        public bool RemoveUnit(int unitId)
        {
            return unitsById.Remove(unitId);
        }

        private int DeterminePlayerActionsPerTurn()
        {
            foreach (var entry in unitsById)
            {
                var unit = entry.Value;
                if (unit.Side == CombatUnitSide.Player)
                {
                    return unit.Definition.ActionsPerTurn;
                }
            }

            return 0;
        }
    }
}
