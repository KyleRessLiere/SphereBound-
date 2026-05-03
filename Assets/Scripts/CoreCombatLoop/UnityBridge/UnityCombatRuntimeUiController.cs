using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class UnityCombatRuntimeUiController : MonoBehaviour
    {
        [SerializeField] private UnityCombatListenerBridge bridge = null!;
        [SerializeField] private TMP_Text actionCountText = null!;
        [SerializeField] private TMP_Text enemyIntentText = null!;
        [SerializeField] private Button moveButton = null!;
        [SerializeField] private Button endTurnButton = null!;
        [SerializeField] private Button previousAbilityButton = null!;
        [SerializeField] private Button nextAbilityButton = null!;
        [SerializeField] private CombatRuntimeAbilityButtonView selectedAbilityButton = null!;

        private int selectedAbilityIndex;
        private CombatRuntimeAbilityButtonModel? currentAbilityButton;

        public event System.Action? MoveActivationRequested;
        public event System.Action<CombatRuntimeAbilityButtonModel>? AbilityActivationRequested;

        public CombatRuntimeAbilityButtonModel? CurrentAbilityButton => currentAbilityButton;

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
            if (moveButton != null)
            {
                moveButton.onClick.RemoveAllListeners();
                moveButton.onClick.AddListener(HandleMovePressed);
            }

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

        private void RefreshView()
        {
            if (bridge == null)
            {
                return;
            }

            var model = bridge.BuildRuntimeControlSurfaceModel();
            ApplyActionCount(model);
            ApplyEnemyIntentPanel();
            ApplyMoveState(model);
            ApplyEndTurnState(model);
            ApplyAbilitySelection(model);
        }

        private void ApplyActionCount(CombatRuntimeControlSurfaceModel model)
        {
            if (actionCountText == null)
            {
                return;
            }

            actionCountText.text = $"Actions: {model.RemainingPlayerActions}";
        }

        private void ApplyEnemyIntentPanel()
        {
            if (enemyIntentText == null || bridge == null)
            {
                return;
            }

            var intentModel = bridge.BuildRuntimeEnemyIntentPanelModel();
            if (intentModel.EnemyIntents.Count == 0)
            {
                enemyIntentText.text = "Enemy Intent\nNone";
                return;
            }

            var lines = new string[intentModel.EnemyIntents.Count + 1];
            lines[0] = "Enemy Intent";
            for (var index = 0; index < intentModel.EnemyIntents.Count; index += 1)
            {
                var intent = intentModel.EnemyIntents[index];
                lines[index + 1] = $"{intent.EnemyDisplayName}: {intent.SummaryText}";
            }

            enemyIntentText.text = string.Join("\n", lines);
        }

        private void ApplyMoveState(CombatRuntimeControlSurfaceModel model)
        {
            if (moveButton != null)
            {
                moveButton.interactable = model.CanMove;
            }
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
            selectedAbilityButton.Bind(currentAbilityButton, HandleSelectedAbilityPressed);

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

        private void HandleSelectedAbilityPressed()
        {
            if (currentAbilityButton == null)
            {
                return;
            }

            if (AbilityActivationRequested != null)
            {
                AbilityActivationRequested.Invoke(currentAbilityButton);
                return;
            }

            bridge.ExecuteRuntimeAbility(currentAbilityButton);
        }

        private void HandleMovePressed()
        {
            MoveActivationRequested?.Invoke();
        }
    }
}
