using UnityEditor;
using UnityEngine;

namespace Spherebound.CoreCombatLoop.UnityBridge.Editor
{
    [CustomEditor(typeof(UnityCombatListenerBridge))]
    public sealed class UnityCombatListenerBridgeEditor : UnityEditor.Editor
    {
        private SerializedProperty? initializeDefaultSessionOnEnableProperty;
        private SerializedProperty? autoStartCombatOnEnableProperty;
        private SerializedProperty? createPlaceholderObjectsProperty;
        private SerializedProperty? debugActingUnitIdProperty;
        private SerializedProperty? debugMoveDestinationProperty;
        private SerializedProperty? debugAttackTargetUnitIdProperty;
        private SerializedProperty? debugUnitsProperty;

        public void OnEnable()
        {
            initializeDefaultSessionOnEnableProperty = serializedObject.FindProperty("initializeDefaultSessionOnEnable");
            autoStartCombatOnEnableProperty = serializedObject.FindProperty("autoStartCombatOnEnable");
            createPlaceholderObjectsProperty = serializedObject.FindProperty("createPlaceholderObjects");
            debugActingUnitIdProperty = serializedObject.FindProperty("debugActingUnitId");
            debugMoveDestinationProperty = serializedObject.FindProperty("debugMoveDestination");
            debugAttackTargetUnitIdProperty = serializedObject.FindProperty("debugAttackTargetUnitId");
            debugUnitsProperty = serializedObject.FindProperty("debugUnits");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawBehaviorSection();
            EditorGUILayout.Space();
            DrawCommandSection((UnityCombatListenerBridge)target);
            EditorGUILayout.Space();
            DrawOutputSection((UnityCombatListenerBridge)target);
            EditorGUILayout.Space();
            DrawDebugUnitsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBehaviorSection()
        {
            EditorGUILayout.LabelField("Behavior", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(initializeDefaultSessionOnEnableProperty);
            EditorGUILayout.PropertyField(autoStartCombatOnEnableProperty);
            EditorGUILayout.PropertyField(createPlaceholderObjectsProperty);
            EditorGUILayout.PropertyField(debugActingUnitIdProperty);
            EditorGUILayout.PropertyField(debugMoveDestinationProperty);
            EditorGUILayout.PropertyField(debugAttackTargetUnitIdProperty);
        }

        private static void DrawCommandSection(UnityCombatListenerBridge bridge)
        {
            EditorGUILayout.LabelField("Commands", EditorStyles.boldLabel);

            if (GUILayout.Button("Initialize Default Session"))
            {
                bridge.InitializeDefaultSession();
                EditorUtility.SetDirty(bridge);
            }

            if (GUILayout.Button("Start Observed Combat"))
            {
                bridge.StartObservedCombat();
                EditorUtility.SetDirty(bridge);
            }

            if (GUILayout.Button("Debug Move"))
            {
                bridge.ExecuteDebugMove();
                EditorUtility.SetDirty(bridge);
            }

            if (GUILayout.Button("Debug Attack"))
            {
                bridge.ExecuteDebugAttack();
                EditorUtility.SetDirty(bridge);
            }

            if (GUILayout.Button("Debug End Turn"))
            {
                bridge.ExecuteDebugEndTurn();
                EditorUtility.SetDirty(bridge);
            }

            if (GUILayout.Button("Debug Restart Combat"))
            {
                bridge.RestartObservedCombat();
                EditorUtility.SetDirty(bridge);
            }
        }

        private static void DrawOutputSection(UnityCombatListenerBridge bridge)
        {
            EditorGUILayout.LabelField("Debug Output", EditorStyles.boldLabel);
            DrawReadOnlyTextArea("Actions", bridge.LastActionCountLog, 1);
            DrawReadOnlyTextArea("Board", bridge.LastBoardOutput, 7);
            DrawReadOnlyTextArea("Attack Overlay", bridge.LastAttackOverlayOutput, 7);
            DrawReadOnlyTextArea("File Config", bridge.LastFileOutputConfigPath, 1);
            DrawReadOnlyTextArea("Output File", bridge.LastFileOutputPath, 2);
        }

        private void DrawDebugUnitsSection()
        {
            EditorGUILayout.LabelField("Mirrored Units", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(debugUnitsProperty, true);
        }

        private static void DrawReadOnlyTextArea(string label, string value, int minLines)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(
                    string.IsNullOrEmpty(value) ? "(empty)" : value,
                    GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * minLines));
            }
        }
    }
}
