namespace Spherebound.CoreCombatLoop.Core
{
    public readonly struct BoardDimensions
    {
        public BoardDimensions(int width, int height)
        {
            if (width <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(width), "Board width must be positive.");
            }

            if (height <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(height), "Board height must be positive.");
            }

            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public bool Contains(GridPosition position)
        {
            return position.X >= 0
                && position.X < Width
                && position.Y >= 0
                && position.Y < Height;
        }
    }
}
