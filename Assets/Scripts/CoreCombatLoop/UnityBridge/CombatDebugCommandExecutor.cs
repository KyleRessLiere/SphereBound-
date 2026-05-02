using System;
using System.Collections.Generic;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public static class CombatDebugCommandExecutor
    {
        public static CombatDebugCommandResult Execute(ICombatDebugSession? session, CombatDebugCommandRequest request)
        {
            if (session == null)
            {
                return new CombatDebugCommandResult(
                    false,
                    CombatDebugCommandFailureReason.SessionUnavailable,
                    Array.Empty<ICombatEvent>(),
                    CombatFailureReason.None);
            }

            switch (request.CommandType)
            {
                case CombatDebugCommandType.Move:
                {
                    var result = session.ResolveMove(request.ActingUnitId, request.MoveDestination);
                    return new CombatDebugCommandResult(
                        result.Succeeded,
                        CombatDebugCommandFailureReason.None,
                        result.Events,
                        result.FailureReason);
                }

                case CombatDebugCommandType.Attack:
                {
                    var result = session.ResolveAttack(request.ActingUnitId, request.TargetUnitId);
                    return new CombatDebugCommandResult(
                        result.Succeeded,
                        CombatDebugCommandFailureReason.None,
                        result.Events,
                        result.FailureReason);
                }

                case CombatDebugCommandType.EndTurn:
                {
                    var events = session.EndPlayerTurnAndRunEnemyTurn();
                    return new CombatDebugCommandResult(
                        true,
                        CombatDebugCommandFailureReason.None,
                        events,
                        CombatFailureReason.None);
                }

                default:
                    return new CombatDebugCommandResult(
                        false,
                        CombatDebugCommandFailureReason.UnsupportedSession,
                        Array.Empty<ICombatEvent>(),
                        CombatFailureReason.None);
            }
        }
    }
}
