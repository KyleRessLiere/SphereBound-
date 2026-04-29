namespace Spherebound.CoreCombatLoop.Core
{
    public readonly struct GridPosition
    {
        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }

        public bool IsOrthogonallyAdjacentTo(GridPosition other)
        {
            var dx = X - other.X;
            if (dx < 0)
            {
                dx = -dx;
            }

            var dy = Y - other.Y;
            if (dy < 0)
            {
                dy = -dy;
            }

            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
