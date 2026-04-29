#if UNITY_EDITOR
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Luxodd.Game.Editor
{
    [InitializeOnLoad]
    public static class NewtonsoftPackageChecker
    {
        private static ListRequest _listRequest;
        private const string PackageName = "com.unity.nuget.newtonsoft-json";
        private const string Define = "NEWTONSOFT_JSON";

        static NewtonsoftPackageChecker()
        {
            _listRequest = Client.List(true);
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (_listRequest == null || !_listRequest.IsCompleted) return;

            EditorApplication.update -= OnEditorUpdate;

            bool isInstalled = false;
            foreach (var package in _listRequest.Result)
            {
                if (package.name == PackageName)
                {
                    isInstalled = true;
                    break;
                }
            }

            if (isInstalled)
            {
                AddDefineSymbolIfMissing(Define);
            }
            else
            {
                ShowInstallPrompt();
            }
        }

        private static void AddDefineSymbolIfMissing(string define)
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);
            
            var symbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            if (!symbols.Contains(define))
            {
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, symbols + ";" + define);
                Debug.Log($"✅ Added scripting define symbol: {define}");
            }
        }

        private static void ShowInstallPrompt()
        {
            if (EditorUtility.DisplayDialog(
                    "Missing Dependency: Newtonsoft.Json",
                    "This plugin requires the 'Newtonsoft.Json' package.\nInstall it automatically now?",
                    "Install",
                    "Cancel"))
            {
                Client.Add(PackageName);
                Debug.Log($"📦 Installing package: {PackageName}");
            }
        }
    }
}
#endif