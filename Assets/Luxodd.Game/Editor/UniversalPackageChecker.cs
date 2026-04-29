#if UNITY_EDITOR

using System.Collections.Generic;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Luxodd.Game.Editor
{
    [InitializeOnLoad]
    public static class UniversalPackageChecker
    {
        private static ListRequest _listRequest;
        
        private static readonly List<PackageInfo> RequiredPackages = new List<PackageInfo>
        {
            new PackageInfo("com.unity.textmeshpro", "TextMesh Pro", "TEXTMESHPRO_INSTALLED"),
            new PackageInfo("com.unity.nuget.newtonsoft-json", "Newtonsoft Json", "NEWTONSOFT_JSON")
        
        };

        static UniversalPackageChecker()
        {
            // _listRequest = Client.List(true);
            // EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (_listRequest == null || !_listRequest.IsCompleted) return;

            EditorApplication.update -= OnEditorUpdate;

            foreach (var required in RequiredPackages)
            {
                bool isInstalled = false;
                foreach (var package in _listRequest.Result)
                {
                    if (package.name == required.Name)
                    {
                        isInstalled = true;
                        break;
                    }
                }

                if (isInstalled)
                {
                    AddDefineSymbolIfMissing(required.DefineSymbol);
                }
                else
                {
                    PromptToInstall(required);
                }
            }
        }

        private static void PromptToInstall(PackageInfo package)
        {
            if (EditorUtility.DisplayDialog(
                    $"Missing Dependency: {package.DisplayName}",
                    $"This plugin requires the '{package.DisplayName}' package.\nInstall it automatically now?",
                    "Install",
                    "Cancel"))
            {
                Client.Add(package.Name);
                LoggerHelper.Log($"📦 Installing package: {package.Name}");
            }
        }

        private static void AddDefineSymbolIfMissing(string define)
        {
            if (string.IsNullOrEmpty(define)) return;

            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);

            var symbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            if (!symbols.Contains(define))
            {
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, symbols + ";" + define);
                LoggerHelper.Log($"✅ Added scripting define symbol: {define}");
            }
        }

        private class PackageInfo
        {
            public string Name { get; }
            public string DisplayName { get; }
            public string DefineSymbol { get; }

            public PackageInfo(string name, string displayName, string defineSymbol = null)
            {
                Name = name;
                DisplayName = displayName;
                DefineSymbol = defineSymbol;
            }
        }
    }
}
#endif