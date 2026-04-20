#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool to quickly configure a background for parallax scrolling.
///
/// Menu: Tools > Parallax > ...
///
/// - "Setup 2 Layer Parallax" duplicates the selected sprite once (base + distant).
/// - "Setup 3 Layer Parallax" duplicates it twice (base + distant + near foreground).
/// - "Add Parallax_Layer to Selected" just attaches the component with a guessed factor.
/// - "Remove Parallax Duplicates" cleans up everything the tool created.
///
/// Also adds a right-click shortcut in the Hierarchy:
///   GameObject > Parallax > Setup 3 Layer Parallax
/// </summary>
public static class Parallax_Setup_Editor
{
    const string MenuRoot = "Tools/Parallax/";
    const string DuplicateSuffix = "_Parallax";

    // ---------------------------------------------------------------------
    // Menu entries
    // ---------------------------------------------------------------------

    [MenuItem(MenuRoot + "Setup 2 Layer Parallax", false, 10)]
    static void Setup2() => RunSetup(2);

    [MenuItem(MenuRoot + "Setup 3 Layer Parallax", false, 11)]
    static void Setup3() => RunSetup(3);

    [MenuItem(MenuRoot + "Add Parallax_Layer to Selected", false, 30)]
    static void AddOnly()
    {
        GameObject go = GetSource();
        if (go == null) { Warn("Select a sprite first."); return; }

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) { Warn("Selected object has no SpriteRenderer."); return; }

        Undo.SetCurrentGroupName("Add Parallax_Layer");
        int group = Undo.GetCurrentGroup();

        Parallax_Layer layer = go.GetComponent<Parallax_Layer>();
        if (layer == null) layer = Undo.AddComponent<Parallax_Layer>(go);
        else Undo.RecordObject(layer, "Set factor");

        layer.ParallaxFactor = GuessFactor(go.name);
        layer.Repeat_Vertical = true;
        EditorUtility.SetDirty(layer);

        Undo.CollapseUndoOperations(group);
        EditorUtility.DisplayDialog("Parallax",
            "Attached Parallax_Layer to '" + go.name + "' with factor " +
            layer.ParallaxFactor.ToString("0.00") + ".", "OK");
    }

    [MenuItem(MenuRoot + "Remove Parallax Duplicates", false, 50)]
    static void RemoveDuplicates()
    {
        int removed = 0;
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name.Contains(DuplicateSuffix))
            {
                Undo.DestroyObjectImmediate(go);
                removed++;
            }
        }
        EditorUtility.DisplayDialog("Parallax",
            "Removed " + removed + " duplicate layer(s).", "OK");
    }

    // Hierarchy right-click shortcut
    [MenuItem("GameObject/Parallax/Setup 3 Layer Parallax", false, 0)]
    static void Hierarchy3() => RunSetup(3);

    // ---------------------------------------------------------------------
    // Core logic
    // ---------------------------------------------------------------------

    static void RunSetup(int layerCount)
    {
        GameObject source = GetSource();
        if (source == null) { Warn("Select a background sprite first, or name one 'Background'."); return; }

        SpriteRenderer sr = source.GetComponent<SpriteRenderer>();
        if (sr == null) { Warn("'" + source.name + "' has no SpriteRenderer."); return; }

        Undo.SetCurrentGroupName("Setup Parallax Layers");
        int group = Undo.GetCurrentGroup();

        // Tag the base as layer 1.0
        Parallax_Layer baseLayer = source.GetComponent<Parallax_Layer>();
        if (baseLayer == null) baseLayer = Undo.AddComponent<Parallax_Layer>(source);
        else Undo.RecordObject(baseLayer, "Set base factor");

        baseLayer.ParallaxFactor = 1.0f;
        baseLayer.Repeat_Vertical = true;
        EditorUtility.SetDirty(baseLayer);

        int baseOrder = sr.sortingOrder;

        // Distant layer (always added)
        CreateDuplicate(
            source,
            "Distant",
            parallaxFactor: 0.7f,
            tint: new Color(0.78f, 0.78f, 0.88f, 0.85f),
            positionOffset: new Vector3(0f, -3f, 0.5f),
            sortingOrder: baseOrder - 2);

        // Near layer (only for 3-layer)
        if (layerCount >= 3)
        {
            CreateDuplicate(
                source,
                "Near",
                parallaxFactor: 0.4f,
                tint: new Color(0.60f, 0.62f, 0.75f, 0.90f),
                positionOffset: new Vector3(1.5f, -5f, -0.3f),
                sortingOrder: baseOrder - 1);
        }

        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = source;

        EditorUtility.DisplayDialog(
            "Parallax Setup Complete",
            "Source: " + source.name + "\n\n" +
            "Base layer:    factor 1.00  (camera-locked)\n" +
            "Distant layer: factor 0.70  (appears far away)\n" +
            (layerCount >= 3 ? "Near layer:    factor 0.40  (appears close)\n" : "") +
            "\nTip: replace the sprite on each duplicate with a different " +
            "layer art (mountains, trees, clouds) for real depth.",
            "Got it");
    }

    static GameObject CreateDuplicate(
        GameObject source, string role, float parallaxFactor,
        Color tint, Vector3 positionOffset, int sortingOrder)
    {
        GameObject dup = Object.Instantiate(source, source.transform.parent);
        dup.name = source.name + DuplicateSuffix + "_" + role;
        dup.transform.position = source.transform.position + positionOffset;
        dup.transform.localScale = source.transform.localScale;
        Undo.RegisterCreatedObjectUndo(dup, "Create parallax layer");

        SpriteRenderer dupSR = dup.GetComponent<SpriteRenderer>();
        if (dupSR != null)
        {
            dupSR.color = tint;
            dupSR.sortingOrder = sortingOrder;
        }

        // Clear child objects that would be duplicated weirdly (none expected, but be safe)
        // — leave as-is; user can trim manually if needed.

        Parallax_Layer layer = dup.GetComponent<Parallax_Layer>();
        if (layer == null) layer = dup.AddComponent<Parallax_Layer>();
        layer.ParallaxFactor = parallaxFactor;
        layer.Repeat_Vertical = true;
        EditorUtility.SetDirty(layer);

        return dup;
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    static GameObject GetSource()
    {
        if (Selection.activeGameObject != null) return Selection.activeGameObject;

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name == "Background" && go.GetComponent<SpriteRenderer>() != null)
                return go;
        }
        return null;
    }

    static float GuessFactor(string name)
    {
        string n = name.ToLowerInvariant();
        if (n.Contains("sky") || n.Contains("background") || n.Contains("bg")) return 1.0f;
        if (n.Contains("cloud") || n.Contains("mountain") || n.Contains("distant") || n.Contains("far")) return 0.7f;
        if (n.Contains("hill") || n.Contains("tree") || n.Contains("mid")) return 0.5f;
        if (n.Contains("grass") || n.Contains("bush") || n.Contains("near") || n.Contains("foreground")) return 0.3f;
        return 0.5f;
    }

    static void Warn(string msg)
    {
        EditorUtility.DisplayDialog("Parallax Setup", msg, "OK");
    }
}
#endif
