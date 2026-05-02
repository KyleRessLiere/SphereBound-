using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Spherebound.CoreCombatLoop.UnityBridge.Editor
{
    public static class UnityCombatRuntimeUiBuilder
    {
        [MenuItem("Tools/Spherebound/Create Runtime Combat UI")]
        public static void CreateRuntimeCombatUi()
        {
            var bridge = Object.FindFirstObjectByType<UnityCombatListenerBridge>();
            if (bridge == null)
            {
                var bridgeObject = new GameObject("UnityCombatListenerBridge");
                Undo.RegisterCreatedObjectUndo(bridgeObject, "Create combat listener bridge");
                bridge = bridgeObject.AddComponent<UnityCombatListenerBridge>();
            }

            EnsureEventSystemExists();

            var canvas = CreateCanvas();
            var uiRoot = CreateUiRoot(canvas.transform);
            var movePanel = CreateMovePanel(uiRoot.transform, out var upButton, out var downButton, out var leftButton, out var rightButton, out var endTurnButton);
            RemoveLegacyAbilityListObjects();
            var abilitySelectorPanel = CreateAbilitySelectorPanel(uiRoot.transform, out var previousAbilityButton, out var nextAbilityButton, out var selectedAbilityButton);

            var controller = uiRoot.GetComponent<UnityCombatRuntimeUiController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<UnityCombatRuntimeUiController>(uiRoot);
            }

            AssignControllerReferences(controller, bridge, upButton, downButton, leftButton, rightButton, endTurnButton, previousAbilityButton, nextAbilityButton, selectedAbilityButton);
            EditorSceneManager.MarkSceneDirty(uiRoot.scene);
            Selection.activeObject = uiRoot;
        }

        public static void CreateRuntimeCombatUiInSampleScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            CreateRuntimeCombatUi();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureEventSystemExists()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas()
        {
            var existingCanvas = Object.FindFirstObjectByType<Canvas>();
            if (existingCanvas != null)
            {
                return existingCanvas;
            }

            var canvasObject = new GameObject("CombatRuntimeCanvas");
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create combat runtime canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateUiRoot(Transform canvasTransform)
        {
            var existingRoot = GameObject.Find("CombatRuntimeUiRoot");
            if (existingRoot != null)
            {
                return existingRoot;
            }

            var root = new GameObject("CombatRuntimeUiRoot", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(root, "Create runtime UI root");
            root.transform.SetParent(canvasTransform, false);
            var rectTransform = (RectTransform)root.transform;
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return root;
        }

        private static GameObject CreateMovePanel(
            Transform parent,
            out Button upButton,
            out Button downButton,
            out Button leftButton,
            out Button rightButton,
            out Button endTurnButton)
        {
            var panel = CreatePanel("MovePanel", parent, new Vector2(180f, 220f), new Vector2(100f, 140f));
            upButton = CreateButton("UpButton", panel.transform, "Up", new Vector2(0f, 60f));
            downButton = CreateButton("DownButton", panel.transform, "Down", new Vector2(0f, -60f));
            leftButton = CreateButton("LeftButton", panel.transform, "Left", new Vector2(-60f, 0f));
            rightButton = CreateButton("RightButton", panel.transform, "Right", new Vector2(60f, 0f));
            endTurnButton = CreateButton("EndTurnButton", panel.transform, "End Turn", new Vector2(0f, -120f), new Vector2(140f, 30f));
            return panel;
        }

        private static GameObject CreateAbilitySelectorPanel(
            Transform parent,
            out Button previousAbilityButton,
            out Button nextAbilityButton,
            out CombatRuntimeAbilityButtonView selectedAbilityButton)
        {
            var existingPanel = GameObject.Find("AbilitySelectorPanel");
            if (existingPanel != null)
            {
                previousAbilityButton = FindRequiredButton(existingPanel.transform, "PreviousAbilityButton", "<");
                nextAbilityButton = FindRequiredButton(existingPanel.transform, "NextAbilityButton", ">");
                selectedAbilityButton = FindRequiredAbilityButton(existingPanel.transform, "SelectedAbilityButton");
                return existingPanel;
            }

            var panel = CreatePanel("AbilitySelectorPanel", parent, new Vector2(420f, 180f), new Vector2(-220f, 0f));
            previousAbilityButton = CreateButton("PreviousAbilityButton", panel.transform, "<", new Vector2(-150f, 0f), new Vector2(50f, 120f));
            nextAbilityButton = CreateButton("NextAbilityButton", panel.transform, ">", new Vector2(150f, 0f), new Vector2(50f, 120f));
            selectedAbilityButton = CreateSelectedAbilityButton(panel.transform);
            return panel;
        }

        private static CombatRuntimeAbilityButtonView CreateSelectedAbilityButton(Transform parent)
        {
            var existing = GameObject.Find("SelectedAbilityButton");
            if (existing != null && existing.TryGetComponent<CombatRuntimeAbilityButtonView>(out var existingView))
            {
                ConfigureButtonVisuals(existing.GetComponent<Image>());
                var existingLabel = EnsureTmpLabel(existing.transform, "Label");
                ConfigureAbilityButtonLabel(existingLabel);
                AssignAbilityButtonViewReferences(existingView, existing.GetComponent<Button>(), existingLabel);
                return existingView;
            }

            var buttonObject = new GameObject("SelectedAbilityButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CombatRuntimeAbilityButtonView));
            Undo.RegisterCreatedObjectUndo(buttonObject, "Create selected ability button");
            buttonObject.transform.SetParent(parent, false);
            var rectTransform = (RectTransform)buttonObject.transform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(260f, 120f);
            ConfigureButtonVisuals(buttonObject.GetComponent<Image>());

            var text = EnsureTmpLabel(buttonObject.transform, "Label");
            ConfigureAbilityButtonLabel(text);
            text.text = "Ability Name\nDescription\nCost: 1\nTiles: (1, 2)";

            var view = buttonObject.GetComponent<CombatRuntimeAbilityButtonView>();
            AssignAbilityButtonViewReferences(view, buttonObject.GetComponent<Button>(), text);
            return view;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 size, Vector2 anchoredPosition)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
            {
                return existing;
            }

            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(panel, "Create panel");
            panel.transform.SetParent(parent, false);
            var rectTransform = (RectTransform)panel.transform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;
            var image = panel.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.4f);
            return panel;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2? size = null)
        {
            var existing = GameObject.Find(name);
            if (existing != null && existing.TryGetComponent<Button>(out var existingButton))
            {
                ConfigureButtonVisuals(existing.GetComponent<Image>());
                var existingLabel = EnsureTmpLabel(existing.transform, "Text");
                ConfigureStandardButtonLabel(existingLabel);
                existingLabel.text = label;
                return existingButton;
            }

            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            Undo.RegisterCreatedObjectUndo(buttonObject, "Create button");
            buttonObject.transform.SetParent(parent, false);
            var rectTransform = (RectTransform)buttonObject.transform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size ?? new Vector2(90f, 40f);
            ConfigureButtonVisuals(buttonObject.GetComponent<Image>());

            var text = EnsureTmpLabel(buttonObject.transform, "Text");
            ConfigureStandardButtonLabel(text);
            text.text = label;

            return buttonObject.GetComponent<Button>();
        }

        private static void ConfigureButtonVisuals(Image? image)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        }

        private static void ConfigureStandardButtonLabel(TMP_Text? text)
        {
            if (text == null)
            {
                return;
            }

            text.color = Color.black;
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void ConfigureAbilityButtonLabel(TMP_Text? text)
        {
            if (text == null)
            {
                return;
            }

            text.color = Color.black;
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
        }

        private static void AssignControllerReferences(
            UnityCombatRuntimeUiController controller,
            UnityCombatListenerBridge bridge,
            Button upButton,
            Button downButton,
            Button leftButton,
            Button rightButton,
            Button endTurnButton,
            Button previousAbilityButton,
            Button nextAbilityButton,
            CombatRuntimeAbilityButtonView selectedAbilityButton)
        {
            var serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("bridge").objectReferenceValue = bridge;
            serializedObject.FindProperty("upButton").objectReferenceValue = upButton;
            serializedObject.FindProperty("downButton").objectReferenceValue = downButton;
            serializedObject.FindProperty("leftButton").objectReferenceValue = leftButton;
            serializedObject.FindProperty("rightButton").objectReferenceValue = rightButton;
            serializedObject.FindProperty("endTurnButton").objectReferenceValue = endTurnButton;
            serializedObject.FindProperty("previousAbilityButton").objectReferenceValue = previousAbilityButton;
            serializedObject.FindProperty("nextAbilityButton").objectReferenceValue = nextAbilityButton;
            serializedObject.FindProperty("selectedAbilityButton").objectReferenceValue = selectedAbilityButton;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignAbilityButtonViewReferences(CombatRuntimeAbilityButtonView view, Button button, TMP_Text text)
        {
            var serializedObject = new SerializedObject(view);
            serializedObject.FindProperty("button").objectReferenceValue = button;
            serializedObject.FindProperty("labelText").objectReferenceValue = text;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TextMeshProUGUI EnsureTmpLabel(Transform parent, string childName)
        {
            var existingChild = parent.Find(childName);
            GameObject labelObject;
            if (existingChild != null)
            {
                labelObject = existingChild.gameObject;
            }
            else
            {
                labelObject = new GameObject(childName, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(labelObject, "Create TMP label");
                labelObject.transform.SetParent(parent, false);
            }

            var textRect = (RectTransform)labelObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = childName == "Label" ? new Vector2(10f, 10f) : Vector2.zero;
            textRect.offsetMax = childName == "Label" ? new Vector2(-10f, -10f) : Vector2.zero;

            var legacyText = labelObject.GetComponent<Text>();
            if (legacyText != null)
            {
                Object.DestroyImmediate(legacyText);
            }

            var tmpText = labelObject.GetComponent<TextMeshProUGUI>();
            if (tmpText == null)
            {
                tmpText = Undo.AddComponent<TextMeshProUGUI>(labelObject);
            }

            return tmpText;
        }

        private static Button FindRequiredButton(Transform parent, string name, string label)
        {
            var existing = parent.Find(name);
            if (existing != null && existing.TryGetComponent<Button>(out var existingButton))
            {
                ConfigureButtonVisuals(existing.GetComponent<Image>());
                var existingLabel = EnsureTmpLabel(existing, "Text");
                ConfigureStandardButtonLabel(existingLabel);
                existingLabel.text = label;
                return existingButton;
            }

            return CreateButton(name, parent, label, Vector2.zero);
        }

        private static CombatRuntimeAbilityButtonView FindRequiredAbilityButton(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null && existing.TryGetComponent<CombatRuntimeAbilityButtonView>(out var existingView))
            {
                ConfigureButtonVisuals(existing.GetComponent<Image>());
                var existingLabel = EnsureTmpLabel(existing, "Label");
                ConfigureAbilityButtonLabel(existingLabel);
                AssignAbilityButtonViewReferences(existingView, existing.GetComponent<Button>(), existingLabel);
                return existingView;
            }

            return CreateSelectedAbilityButton(parent);
        }

        private static void RemoveLegacyAbilityListObjects()
        {
            DestroyIfPresent("AbilityPanel");
            DestroyIfPresent("AbilityButtonTemplate");
        }

        private static void DestroyIfPresent(string objectName)
        {
            var existing = GameObject.Find(objectName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }
    }
}
