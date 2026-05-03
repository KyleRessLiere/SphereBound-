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
            RemoveLegacyMoveObjects();
            RemoveLegacyAbilityListObjects();
            var abilitySelectorPanel = CreateAbilitySelectorPanel(uiRoot.transform, out var moveButton, out var previousAbilityButton, out var nextAbilityButton, out var selectedAbilityButton, out var endTurnButton);

            var controller = uiRoot.GetComponent<UnityCombatRuntimeUiController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<UnityCombatRuntimeUiController>(uiRoot);
            }

            AssignControllerReferences(controller, bridge, moveButton, endTurnButton, previousAbilityButton, nextAbilityButton, selectedAbilityButton);
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

        [MenuItem("Tools/Spherebound/Create Tactical Board View")]
        public static void CreateTacticalBoardView()
        {
            var bridge = Object.FindFirstObjectByType<UnityCombatListenerBridge>();
            if (bridge == null)
            {
                var bridgeObject = new GameObject("UnityCombatListenerBridge");
                Undo.RegisterCreatedObjectUndo(bridgeObject, "Create combat listener bridge");
                bridge = bridgeObject.AddComponent<UnityCombatListenerBridge>();
            }

            var runtimeUi = Object.FindFirstObjectByType<UnityCombatRuntimeUiController>();
            if (runtimeUi == null)
            {
                CreateRuntimeCombatUi();
                runtimeUi = Object.FindFirstObjectByType<UnityCombatRuntimeUiController>();
            }

            EnsureEventSystemExists();
            var boardCamera = EnsureBoardCamera();
            var boardRoot = EnsureBoardViewRoot();
            var tacticalBoardView = boardRoot.GetComponent<UnityCombatTacticalBoardView>();
            if (tacticalBoardView == null)
            {
                tacticalBoardView = Undo.AddComponent<UnityCombatTacticalBoardView>(boardRoot);
            }

            tacticalBoardView.Configure(bridge, runtimeUi, boardCamera);
            EditorUtility.SetDirty(tacticalBoardView);
            EditorSceneManager.MarkSceneDirty(boardRoot.scene);
            Selection.activeObject = boardRoot;
        }

        public static void CreateTacticalBoardViewInSampleScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            CreateTacticalBoardView();
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

        private static Camera EnsureBoardCamera()
        {
            var existingCamera = Object.FindFirstObjectByType<Camera>();
            if (existingCamera != null)
            {
                existingCamera.transform.position = new Vector3(0f, 8f, -7f);
                existingCamera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
                return existingCamera;
            }

            var cameraObject = new GameObject("TacticalBoardCamera");
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create tactical board camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 8f, -7f);
            camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            camera.clearFlags = CameraClearFlags.Skybox;
            return camera;
        }

        private static GameObject EnsureBoardViewRoot()
        {
            var existingRoot = GameObject.Find("CombatTacticalBoardView");
            if (existingRoot != null)
            {
                return existingRoot;
            }

            var root = new GameObject("CombatTacticalBoardView");
            Undo.RegisterCreatedObjectUndo(root, "Create tactical board view");
            return root;
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

        private static GameObject CreateAbilitySelectorPanel(
            Transform parent,
            out Button moveButton,
            out Button previousAbilityButton,
            out Button nextAbilityButton,
            out CombatRuntimeAbilityButtonView selectedAbilityButton,
            out Button endTurnButton)
        {
            var panel = CreatePanel("AbilitySelectorPanel", parent, new Vector2(420f, 260f), new Vector2(-220f, 0f));
            ConfigureRect((RectTransform)panel.transform, new Vector2(-220f, 0f), new Vector2(420f, 260f));
            moveButton = FindRequiredButton(panel.transform, "MoveButton", "Move", new Vector2(0f, 90f), new Vector2(140f, 36f));
            previousAbilityButton = FindRequiredButton(panel.transform, "PreviousAbilityButton", "<", new Vector2(-150f, 0f), new Vector2(50f, 120f));
            nextAbilityButton = FindRequiredButton(panel.transform, "NextAbilityButton", ">", new Vector2(150f, 0f), new Vector2(50f, 120f));
            selectedAbilityButton = FindRequiredAbilityButton(panel.transform, "SelectedAbilityButton", Vector2.zero, new Vector2(260f, 120f));
            endTurnButton = FindRequiredButton(panel.transform, "EndTurnButton", "End Turn", new Vector2(0f, -90f), new Vector2(140f, 36f));
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
                ConfigureRect((RectTransform)existing.transform, anchoredPosition, size ?? new Vector2(90f, 40f));
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
            Button moveButton,
            Button endTurnButton,
            Button previousAbilityButton,
            Button nextAbilityButton,
            CombatRuntimeAbilityButtonView selectedAbilityButton)
        {
            var serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("bridge").objectReferenceValue = bridge;
            serializedObject.FindProperty("moveButton").objectReferenceValue = moveButton;
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

        private static Button FindRequiredButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
        {
            var existing = parent.Find(name);
            if (existing != null && existing.TryGetComponent<Button>(out var existingButton))
            {
                ConfigureRect((RectTransform)existing, anchoredPosition, size);
                ConfigureButtonVisuals(existing.GetComponent<Image>());
                var existingLabel = EnsureTmpLabel(existing, "Text");
                ConfigureStandardButtonLabel(existingLabel);
                existingLabel.text = label;
                return existingButton;
            }

            return CreateButton(name, parent, label, anchoredPosition, size);
        }

        private static CombatRuntimeAbilityButtonView FindRequiredAbilityButton(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            var existing = parent.Find(name);
            if (existing != null && existing.TryGetComponent<CombatRuntimeAbilityButtonView>(out var existingView))
            {
                ConfigureRect((RectTransform)existing, anchoredPosition, size);
                ConfigureButtonVisuals(existing.GetComponent<Image>());
                var existingLabel = EnsureTmpLabel(existing, "Label");
                ConfigureAbilityButtonLabel(existingLabel);
                AssignAbilityButtonViewReferences(existingView, existing.GetComponent<Button>(), existingLabel);
                return existingView;
            }

            var created = CreateSelectedAbilityButton(parent);
            ConfigureRect((RectTransform)created.transform, anchoredPosition, size);
            return created;
        }

        private static void RemoveLegacyAbilityListObjects()
        {
            DestroyIfPresent("AbilityPanel");
            DestroyIfPresent("AbilityButtonTemplate");
        }

        private static void RemoveLegacyMoveObjects()
        {
            DestroyIfPresent("MovePanel");
            DestroyIfPresent("UpButton");
            DestroyIfPresent("DownButton");
            DestroyIfPresent("LeftButton");
            DestroyIfPresent("RightButton");
        }

        private static void ConfigureRect(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 size)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
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
