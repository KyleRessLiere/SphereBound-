namespace Spherebound.CoreCombatLoop.Core
{
    public readonly struct GridOffset
    {
        public GridOffset(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }
    }
}
