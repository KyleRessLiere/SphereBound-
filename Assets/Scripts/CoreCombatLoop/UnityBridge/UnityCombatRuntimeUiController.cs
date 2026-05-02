using UnityEngine;
using UnityEngine.UI;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class UnityCombatRuntimeUiController : MonoBehaviour
    {
        [SerializeField] private UnityCombatListenerBridge bridge = null!;
        [SerializeField] private Button upButton = null!;
        [SerializeField] private Button downButton = null!;
        [SerializeField] private Button leftButton = null!;
        [SerializeField] private Button rightButton = null!;
        [SerializeField] private Button endTurnButton = null!;
        [SerializeField] private Button previousAbilityButton = null!;
        [SerializeField] private Button nextAbilityButton = null!;
        [SerializeField] private CombatRuntimeAbilityButtonView selectedAbilityButton = null!;

        private int selectedAbilityIndex;
        private CombatRuntimeAbilityButtonModel? currentAbilityButton;

        private void OnEnable()
        {
            if (bridge != null)
            {
                bridge.RuntimeStateChanged += RefreshView;
            }

            WireStaticButtons();
            RefreshView();
        }

        private void OnDisable()
        {
            if (bridge != null)
            {
                bridge.RuntimeStateChanged -= RefreshView;
            }
        }

        private void WireStaticButtons()
        {
            BindMoveButton(upButton, CombatRuntimeDirection.Up);
            BindMoveButton(downButton, CombatRuntimeDirection.Down);
            BindMoveButton(leftButton, CombatRuntimeDirection.Left);
            BindMoveButton(rightButton, CombatRuntimeDirection.Right);

            if (endTurnButton != null)
            {
                endTurnButton.onClick.RemoveAllListeners();
                endTurnButton.onClick.AddListener(() => bridge.ExecuteRuntimeEndTurn());
            }

            if (previousAbilityButton != null)
            {
                previousAbilityButton.onClick.RemoveAllListeners();
                previousAbilityButton.onClick.AddListener(() => CycleAbility(-1));
            }

            if (nextAbilityButton != null)
            {
                nextAbilityButton.onClick.RemoveAllListeners();
                nextAbilityButton.onClick.AddListener(() => CycleAbility(1));
            }
        }

        private void BindMoveButton(Button button, CombatRuntimeDirection direction)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => bridge.ExecuteRuntimeMove(direction));
        }

        private void RefreshView()
        {
            if (bridge == null)
            {
                return;
            }

            var model = bridge.BuildRuntimeControlSurfaceModel();
            ApplyMoveButtonState(model);
            ApplyEndTurnState(model);
            ApplyAbilitySelection(model);
        }

        private void ApplyMoveButtonState(CombatRuntimeControlSurfaceModel model)
        {
            SetMoveButtonInteractable(model, CombatRuntimeDirection.Up, upButton);
            SetMoveButtonInteractable(model, CombatRuntimeDirection.Down, downButton);
            SetMoveButtonInteractable(model, CombatRuntimeDirection.Left, leftButton);
            SetMoveButtonInteractable(model, CombatRuntimeDirection.Right, rightButton);
        }

        private void SetMoveButtonInteractable(CombatRuntimeControlSurfaceModel model, CombatRuntimeDirection direction, Button button)
        {
            if (button == null)
            {
                return;
            }

            for (var index = 0; index < model.MoveButtons.Count; index += 1)
            {
                var moveButton = model.MoveButtons[index];
                if (moveButton.Direction == direction)
                {
                    button.interactable = moveButton.IsInteractable;
                    return;
                }
            }

            button.interactable = false;
        }

        private void ApplyEndTurnState(CombatRuntimeControlSurfaceModel model)
        {
            if (endTurnButton != null)
            {
                endTurnButton.interactable = model.CanEndTurn;
            }
        }

        private void ApplyAbilitySelection(CombatRuntimeControlSurfaceModel model)
        {
            selectedAbilityIndex = CombatRuntimeAbilitySelection.ClampIndex(selectedAbilityIndex, model.AbilityButtons.Count);
            currentAbilityButton = null;

            if (selectedAbilityButton == null)
            {
                return;
            }

            if (model.AbilityButtons.Count == 0)
            {
                selectedAbilityButton.gameObject.SetActive(false);
                if (previousAbilityButton != null)
                {
                    previousAbilityButton.interactable = false;
                }

                if (nextAbilityButton != null)
                {
                    nextAbilityButton.interactable = false;
                }

                return;
            }

            selectedAbilityButton.gameObject.SetActive(true);
            currentAbilityButton = model.AbilityButtons[selectedAbilityIndex];
            selectedAbilityButton.Bind(currentAbilityButton, () => bridge.ExecuteRuntimeAbility(currentAbilityButton));

            var canCycle = model.AbilityButtons.Count > 1;
            if (previousAbilityButton != null)
            {
                previousAbilityButton.interactable = canCycle;
            }

            if (nextAbilityButton != null)
            {
                nextAbilityButton.interactable = canCycle;
            }
        }

        private void CycleAbility(int delta)
        {
            if (bridge == null)
            {
                return;
            }

            var model = bridge.BuildRuntimeControlSurfaceModel();
            selectedAbilityIndex = CombatRuntimeAbilitySelection.Cycle(selectedAbilityIndex, delta, model.AbilityButtons.Count);
            ApplyAbilitySelection(model);
        }
    }
}
