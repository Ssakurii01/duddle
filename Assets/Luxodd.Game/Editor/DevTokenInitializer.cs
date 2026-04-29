#if UNITY_EDITOR
using System;
using Luxodd.Game.Scripts.Network;
using UnityEditor;
using UnityEngine;

namespace Luxodd.Game.Editor
{
    [InitializeOnLoad]
    public static class DevTokenInitializer
    {
        private const string EditorKey = "MyPlugin_TokenPromptShown";

        static DevTokenInitializer()
        {
            //if (!SessionState.GetBool(EditorKey, false))
            {
                //LoggerHelper.Log($"[{DateTime.Now}][{nameof(DevTokenInitializer)}][{nameof(DevTokenInitializer)}] OK");
                SessionState.SetBool(EditorKey, true);
                EditorApplication.delayCall += TryShowPrompt;
            }
        }

        private static void TryShowPrompt()
        {
            var config = LoadServerConfig();
            //LoggerHelper.Log($"[{DateTime.Now}][TryShowPrompt][] OK");
            if (config != null && string.IsNullOrEmpty(config.DeveloperDebugToken))
            {
                bool shouldEnter = EditorUtility.DisplayDialog(
                    "Developer Debug Token",
                    "Would you like to enter a Developer Token for local testing?",
                    "Enter Token",
                    "Skip");

                if (shouldEnter)
                {
                    DevTokenPromptWindow.ShowWindow(config);
                }
            }
        }

        private static NetworkSettingsDescriptor LoadServerConfig()
        {
            return Resources.Load<NetworkSettingsDescriptor>(nameof(NetworkSettingsDescriptor));
        }
    }
}
#endif