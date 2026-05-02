using System.Text;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Verification
{
    public static class VerificationBoardStateFormatter
    {
        public static string FormatBoard(CombatState state)
        {
            var builder = new StringBuilder();
            for (var y = state.Board.Height - 1; y >= 0; y -= 1)
            {
                for (var x = 0; x < state.Board.Width; x += 1)
                {
                    var position = new GridPosition(x, y);
                    builder.Append('[');
                    if (state.TryGetUnitAtPosition(position, out var unit) && unit.IsAlive)
                    {
                        builder.Append(unit.Side == CombatUnitSide.Player ? 'P' : 'E');
                    }
                    else
                    {
                        builder.Append(' ');
                    }

                    builder.Append(']');
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
