using System;
using Spherebound.CoreCombatLoop.Core;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public interface ICombatSessionObserver
    {
        string SessionId { get; }

        BridgedCombatSessionSnapshot CaptureSnapshot();

        IDisposable Subscribe(Action<ICombatEvent> onEvent);
    }
}
