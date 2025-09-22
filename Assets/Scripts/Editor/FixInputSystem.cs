using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace TetrisJenga.EditorUtilities
{
    /// <summary>
    /// Utility to fix Input System configuration
    /// </summary>
    public static class FixInputSystem
    {
        [MenuItem("Tools/Fix Input System/Switch to Old Input (Legacy)")]
        public static void SwitchToOldInput()
        {
            ChangeInputSystem(0); // 0 = Old
        }

        [MenuItem("Tools/Fix Input System/Switch to New Input System")]
        public static void SwitchToNewInput()
        {
            ChangeInputSystem(1); // 1 = New
        }

        [MenuItem("Tools/Fix Input System/Enable Both Input Systems")]
        public static void EnableBothInputSystems()
        {
            ChangeInputSystem(2); // 2 = Both
        }

        private static void ChangeInputSystem(int value)
        {
            var settingsAsset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/ProjectSettings.asset");
            if (settingsAsset != null)
            {
                var serializedObject = new SerializedObject(settingsAsset);
                var propertyPath = "activeInputHandler";
                var property = serializedObject.FindProperty(propertyPath);

                if (property != null)
                {
                    property.intValue = value;
                    serializedObject.ApplyModifiedProperties();

                    string mode = value == 0 ? "Old Input (Legacy)" :
                                 value == 1 ? "New Input System" :
                                 "Both Input Systems";

                    UnityEngine.Debug.Log($"[Input System] Switched to: {mode}");
                    UnityEngine.Debug.Log($"[Input System] You may need to restart Unity for changes to take full effect.");

                    if (value == 0 || value == 2)
                    {
                        UnityEngine.Debug.Log("[Input System] The UnityEngine.Input errors should now be resolved!");
                    }

                    EditorApplication.ExecuteMenuItem("File/Save Project");
                }
                else
                {
                    UnityEngine.Debug.LogError("[Input System] Could not find activeInputHandler property. Trying alternative method...");

                    // Try using EditorPrefs as a fallback
                    EditorPrefs.SetInt("UnityEditor.PlayerSettings.ActiveInputHandler", value);
                    UnityEngine.Debug.Log("[Input System] Set via EditorPrefs. Please restart Unity Editor for changes to take effect.");
                }
            }
            else
            {
                UnityEngine.Debug.LogError("[Input System] Could not load ProjectSettings.asset");
            }
        }

        [MenuItem("Tools/Fix Input System/Check Current Setting")]
        public static void CheckCurrentSetting()
        {
            var settingsAsset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/ProjectSettings.asset");
            if (settingsAsset != null)
            {
                var serializedObject = new SerializedObject(settingsAsset);
                var property = serializedObject.FindProperty("activeInputHandler");

                if (property != null)
                {
                    int currentValue = property.intValue;
                    string mode = currentValue == 0 ? "Old Input (Legacy)" :
                                 currentValue == 1 ? "New Input System" :
                                 currentValue == 2 ? "Both Input Systems" :
                                 "Unknown";

                    UnityEngine.Debug.Log($"[Input System] Current setting: {mode} (value: {currentValue})");

                    if (currentValue == 1)
                    {
                        UnityEngine.Debug.LogWarning("[Input System] Currently set to New Input System only. This is why UnityEngine.Input calls are failing.");
                        UnityEngine.Debug.Log("[Input System] Run 'Tools/Fix Input System/Enable Both Input Systems' to fix the errors.");
                    }
                }
            }
        }
    }
}