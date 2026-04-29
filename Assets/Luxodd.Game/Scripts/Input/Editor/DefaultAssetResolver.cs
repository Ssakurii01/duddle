using UnityEditor;
using UnityEngine;

namespace Luxodd.Game.Scripts.Input.Editor
{
    public static class DefaultAssetResolver
    {
        private const string LabelPanel = "unity-plugin-default-panel";
        private const string LabelConfig = "unity-plugin-default-inputconfig";
        private const string LabelBindings = "unity-plugin-default-bindings";

        public static Texture2D LoadDefaultPanelTexture()
        {
            var tex = LoadByLabel<Texture2D>(LabelPanel);
            if (tex != null) return tex;

            // fallback: by name
            return LoadByName<Texture2D>("arcade_panel");
        }

        public static ArcadeInputConfigAsset LoadDefaultInputConfig()
        {
            var cfg = LoadByLabel<ArcadeInputConfigAsset>(LabelConfig);
            if (cfg != null) return cfg;

            return LoadByName<ArcadeInputConfigAsset>("ArcadeInputConfigAsset");
        }

        public static ArcadeBindingAsset LoadDefaultBindings()
        {
            var b = LoadByLabel<ArcadeBindingAsset>(LabelBindings);
            if (b != null) return b;

            return LoadByName<ArcadeBindingAsset>("ArcadeBindingAsset");
        }

        private static T LoadByLabel<T>(string label) where T : Object
        {
            // Example query: "l:arcadesdk-default-panel t:Texture2D"
            string filter = $"l:{label} t:{typeof(T).Name}";
            var guids = AssetDatabase.FindAssets(filter);
            if (guids == null || guids.Length == 0) return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static T LoadByName<T>(string nameContains) where T : Object
        {
            // Example: "ArcadePanel t:Texture2D"
            string filter = $"{nameContains} t:{typeof(T).Name}";
            var guids = AssetDatabase.FindAssets(filter);
            if (guids == null || guids.Length == 0) return null;

            // Prefer shortest path (often inside your plugin folder)
            string bestPath = null;
            foreach (var guid in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (bestPath == null || p.Length < bestPath.Length)
                    bestPath = p;
            }

            return bestPath != null ? AssetDatabase.LoadAssetAtPath<T>(bestPath) : null;
        }
    }
}