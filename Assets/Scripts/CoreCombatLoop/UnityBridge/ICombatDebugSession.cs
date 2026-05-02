using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public interface ICombatDebugSession : ICombatSessionObserver
    {
        IReadOnlyList<ICombatEvent> StartCombat();

        CombatActionResult ResolveMove(int unitId, GridPosition destination);

        CombatActionResult ResolveAttack(int attackerUnitId, int targetUnitId);

        IReadOnlyList<ICombatEvent> EndPlayerTurnAndRunEnemyTurn();
    }
}
