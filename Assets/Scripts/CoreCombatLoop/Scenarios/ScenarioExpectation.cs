using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.Scenarios
{
    public sealed class ScenarioExpectation
    {
        public ScenarioExpectation(
            int unitId,
            int? expectedHealth = null,
            GridPosition? expectedPosition = null,
            UnitLifeState? expectedLifeState = null)
        {
            UnitId = unitId;
            ExpectedHealth = expectedHealth;
            ExpectedPosition = expectedPosition;
            ExpectedLifeState = expectedLifeState;
        }

        public int UnitId { get; }

        public int? ExpectedHealth { get; }

        public GridPosition? ExpectedPosition { get; }

        public UnitLifeState? ExpectedLifeState { get; }
    }
}
