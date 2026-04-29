using UnityEditor;
using UnityEngine;

namespace Luxodd.Game.Scripts.Input.Editor
{
    public static class UnityPluginLabelsMenu 
    {
        //[MenuItem("Unity Plugin/SDK Labels Menu")]
        public static void ApplyLabels()
        {
            Apply("Assets/Luxodd.Game/Editor/DefaultResources/arcade_panel.png", "unity-plugin-default-panel");
            Apply("Assets/Luxodd.Game/Editor/DefaultAssets/ArcadeInputConfigAsset.asset", "unity-plugin-default-inputconfig");
            Apply("Assets/Luxodd.Game/Editor/DefaultAssets/ArcadeBindingAsset.asset", "unity-plugin-default-bindings");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Arcade SDK: Default asset labels applied.");
        }
        
        private static void Apply(string assetPath, string label)
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (obj == null)
            {
                Debug.LogWarning($"Arcade SDK: asset not found at path: {assetPath}");
                return;
            }

            var labels = AssetDatabase.GetLabels(obj);
            if (System.Array.IndexOf(labels, label) >= 0) return;

            var newLabels = new string[labels.Length + 1];
            labels.CopyTo(newLabels, 0);
            newLabels[^1] = label;

            AssetDatabase.SetLabels(obj, newLabels);
            EditorUtility.SetDirty(obj);
        }
    }
}
