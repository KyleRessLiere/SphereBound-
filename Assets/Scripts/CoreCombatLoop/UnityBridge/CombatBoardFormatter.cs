using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public static class CombatBoardFormatter
    {
        public static string FormatBoard(BridgedCombatSessionSnapshot snapshot)
        {
            var cells = CreateBaseCells(snapshot);
            return RenderCells(snapshot.Board, cells);
        }

        public static string FormatAttackOverlay(
            BridgedCombatSessionSnapshot snapshot,
            GridPosition attackerPosition,
            GridPosition targetPosition,
            bool attackConnected)
        {
            var cells = CreateBaseCells(snapshot);
            foreach (var position in CreateAffectedPath(attackerPosition, targetPosition))
            {
                if (snapshot.Board.Contains(position))
                {
                    cells[position] = 'X';
                }
            }

            if (attackConnected && snapshot.Board.Contains(targetPosition))
            {
                cells[targetPosition] = 'O';
            }

            return RenderCells(snapshot.Board, cells);
        }

        private static Dictionary<GridPosition, char> CreateBaseCells(BridgedCombatSessionSnapshot snapshot)
        {
            var cells = new Dictionary<GridPosition, char>();
            for (var y = 0; y < snapshot.Board.Height; y++)
            {
                for (var x = 0; x < snapshot.Board.Width; x++)
                {
                    cells[new GridPosition(x, y)] = '.';
                }
            }

            foreach (var unit in snapshot.Units.Where(unit => unit.LifeState == UnitLifeState.Alive))
            {
                cells[unit.Position] = unit.Side == CombatUnitSide.Player ? 'P' : 'E';
            }

            return cells;
        }

        private static IReadOnlyList<GridPosition> CreateAffectedPath(GridPosition attackerPosition, GridPosition targetPosition)
        {
            var path = new List<GridPosition>();

            if (attackerPosition.X == targetPosition.X)
            {
                var step = targetPosition.Y > attackerPosition.Y ? 1 : -1;
                for (var y = attackerPosition.Y + step; y != targetPosition.Y + step; y += step)
                {
                    path.Add(new GridPosition(attackerPosition.X, y));
                }

                return path;
            }

            if (attackerPosition.Y == targetPosition.Y)
            {
                var step = targetPosition.X > attackerPosition.X ? 1 : -1;
                for (var x = attackerPosition.X + step; x != targetPosition.X + step; x += step)
                {
                    path.Add(new GridPosition(x, attackerPosition.Y));
                }

                return path;
            }

            path.Add(targetPosition);
            return path;
        }

        private static string RenderCells(BoardDimensions board, IReadOnlyDictionary<GridPosition, char> cells)
        {
            var builder = new StringBuilder();
            for (var y = board.Height - 1; y >= 0; y--)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    if (x > 0)
                    {
                        builder.Append(' ');
                    }

                    builder.Append(cells[new GridPosition(x, y)]);
                }

                if (y > 0)
                {
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}
