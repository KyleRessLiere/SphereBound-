namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public static class CombatRuntimeAbilitySelection
    {
        public static int ClampIndex(int selectedIndex, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            if (selectedIndex < 0)
            {
                return 0;
            }

            if (selectedIndex >= count)
            {
                return count - 1;
            }

            return selectedIndex;
        }

        public static int Cycle(int selectedIndex, int delta, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            var normalized = ClampIndex(selectedIndex, count);
            var nextIndex = normalized + delta;
            if (nextIndex < 0)
            {
                return count - 1;
            }

            if (nextIndex >= count)
            {
                return 0;
            }

            return nextIndex;
        }
    }
}
