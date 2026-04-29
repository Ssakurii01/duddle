#if UNITY_EDITOR

using System;
using System.IO;
using Luxodd.Game.Scripts;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Luxodd.Game.Editor
{
    public class WebGLPluginVersionInjector : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL) return;

            string templatePath = "Assets/WebGLTemplates/LuxoddTemplate/index.html.template";
            string outputPath = "Assets/WebGLTemplates/LuxoddTemplate/index.html";
            
            string html = File.ReadAllText(templatePath);

            string pluginVersion = PluginVersion.Version;
            html = html.Replace("{{PLUGIN_VERSION}}", pluginVersion);

            File.WriteAllText(outputPath, html);
            
            Debug.Log($"[{DateTime.Now}]  [PLUGIN] Injected plugin version {pluginVersion} into WebGL template.");
        }
    }
}
#endif