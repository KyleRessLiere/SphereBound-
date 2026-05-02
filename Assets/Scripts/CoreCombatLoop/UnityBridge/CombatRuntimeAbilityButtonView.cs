using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Spherebound.CoreCombatLoop.UnityBridge
{
    public sealed class CombatRuntimeAbilityButtonView : MonoBehaviour
    {
        [SerializeField] private Button button = null!;
        [SerializeField] private TMP_Text labelText = null!;

        public void Bind(CombatRuntimeAbilityButtonModel model, Action onClick)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.interactable = model.IsInteractable;
                button.onClick.AddListener(() => onClick());
            }

            if (labelText != null)
            {
                labelText.text = $"{model.Name}\n{model.Description}\nCost: {model.ActionCost}\nTiles: {model.ResolvedEffectTileText}";
            }
        }
    }
}
