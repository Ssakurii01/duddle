#if UNITY_EDITOR
using Luxodd.Game.Scripts.Network;
using UnityEditor;
using UnityEngine;

namespace Luxodd.Game.Editor
{
    public class DevTokenPromptWindow : EditorWindow
    {
        private string inputToken = "";
        private NetworkSettingsDescriptor config;

        public static void ShowWindow(NetworkSettingsDescriptor config)
        {
            var window = GetWindow<DevTokenPromptWindow>("Set Developer Token");
            window.config = config;
            window.inputToken = config.DeveloperDebugToken;
            window.minSize = new Vector2(460, 128);
            window.Show();
        }

        private void OnGUI()
        {
            if (config == null)
            {
                EditorGUILayout.HelpBox("No config file provided.", MessageType.Error);
                return;
            }

            GUILayout.Label("Enter Developer Debug Token:", EditorStyles.boldLabel);
            inputToken = EditorGUILayout.TextField("Token", inputToken);

            GUILayout.Space(10);
            if (GUILayout.Button("Save"))
            {
                config.DeveloperDebugToken = inputToken;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                Close();
            }
        }
        
        [MenuItem("Luxodd Unity Plugin/First Setup/Set Developer Token")]
        public static void OpenTokenWindow()
        {
            var config = Resources.Load<NetworkSettingsDescriptor>(nameof(NetworkSettingsDescriptor));
            if (config != null)
                DevTokenPromptWindow.ShowWindow(config);
            else
                EditorUtility.DisplayDialog("Error", "ServerConfig asset not found in Resources.", "OK");
        }
    }
}
#endif