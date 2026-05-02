namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public readonly struct CombatRuntimeMoveButtonModel
    {
        public CombatRuntimeMoveButtonModel(CombatRuntimeDirection direction, string label, bool isInteractable)
        {
            Direction = direction;
            Label = label;
            IsInteractable = isInteractable;
        }

        public CombatRuntimeDirection Direction { get; }

        public string Label { get; }

        public bool IsInteractable { get; }
    }
}
