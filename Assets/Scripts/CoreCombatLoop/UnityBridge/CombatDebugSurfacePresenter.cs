using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatDebugSurfacePresenter
    {
        private int? lastLoggedRemainingActions;
        private AttackOverlayContext? pendingAttackOverlay;
        private bool pendingAttackFailed;

        public IReadOnlyList<CombatDebugSurfaceLog> HandleEvent(
            ICombatEvent combatEvent,
            BridgedCombatSessionSnapshot? snapshotBefore,
            BridgedCombatSessionSnapshot snapshotAfter,
            int remainingPlayerActions)
        {
            CachePreRefreshEventState(combatEvent, snapshotBefore);

            var logs = new List<CombatDebugSurfaceLog>();
            if (!lastLoggedRemainingActions.HasValue || lastLoggedRemainingActions.Value != remainingPlayerActions)
            {
                lastLoggedRemainingActions = remainingPlayerActions;
                logs.Add(new CombatDebugSurfaceLog("Actions", $"remainingActions={remainingPlayerActions}"));
            }

            if (combatEvent is UnitMoved)
            {
                logs.Add(new CombatDebugSurfaceLog("Board", CombatBoardFormatter.FormatBoard(snapshotAfter)));
                return logs;
            }

            if (combatEvent is ActionEnded actionEnded
                && actionEnded.ActionType == CombatActionType.Attack
                && pendingAttackOverlay.HasValue)
            {
                if (!pendingAttackFailed)
                {
                    var overlay = pendingAttackOverlay.Value;
                    logs.Add(new CombatDebugSurfaceLog(
                        "AttackBoard",
                        CombatBoardFormatter.FormatAttackOverlay(
                            snapshotAfter,
                            overlay.AttackerPosition,
                            overlay.TargetPosition,
                            overlay.AttackConnected)));
                }

                pendingAttackOverlay = null;
                pendingAttackFailed = false;
            }

            return logs;
        }

        public void Reset()
        {
            lastLoggedRemainingActions = null;
            pendingAttackOverlay = null;
            pendingAttackFailed = false;
        }

        private void CachePreRefreshEventState(ICombatEvent combatEvent, BridgedCombatSessionSnapshot? snapshotBefore)
        {
            if (combatEvent is AttackRequested attackRequested
                && snapshotBefore != null
                && TryGetUnitSnapshot(snapshotBefore, attackRequested.AttackerUnitId, out var attacker)
                && TryGetUnitSnapshot(snapshotBefore, attackRequested.TargetUnitId, out var target))
            {
                pendingAttackOverlay = new AttackOverlayContext(attacker.Position, target.Position, false);
                pendingAttackFailed = false;
                return;
            }

            if (combatEvent is DamageRequested && pendingAttackOverlay.HasValue)
            {
                var overlay = pendingAttackOverlay.Value;
                pendingAttackOverlay = new AttackOverlayContext(
                    overlay.AttackerPosition,
                    overlay.TargetPosition,
                    true);
                return;
            }

            if (combatEvent is ActionFailed actionFailed
                && actionFailed.ActionType == CombatActionType.Attack
                && pendingAttackOverlay.HasValue)
            {
                pendingAttackFailed = true;
            }
        }

        private static bool TryGetUnitSnapshot(BridgedCombatSessionSnapshot snapshot, int unitId, out BridgedUnitSnapshot unit)
        {
            foreach (var currentUnit in snapshot.Units)
            {
                if (currentUnit.UnitId == unitId)
                {
                    unit = currentUnit;
                    return true;
                }
            }

            unit = null!;
            return false;
        }

        private readonly struct AttackOverlayContext
        {
            public AttackOverlayContext(GridPosition attackerPosition, GridPosition targetPosition, bool attackConnected)
            {
                AttackerPosition = attackerPosition;
                TargetPosition = targetPosition;
                AttackConnected = attackConnected;
            }

            public GridPosition AttackerPosition { get; }

            public GridPosition TargetPosition { get; }

            public bool AttackConnected { get; }
        }
    }
}
