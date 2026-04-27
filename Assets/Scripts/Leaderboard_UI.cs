using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows / hides a leaderboard panel on the Main Menu.
///
/// How to use in the editor:
///   1. Add a "Leaderboard" Button to Main_Canvas (UI > Button - Legacy).
///   2. Attach this script to that same Button GameObject.
///   3. In the Button's OnClick() event, drag the same GameObject in
///      and pick Leaderboard_UI > Toggle().
///
/// The panel itself is built in code the first time you click — no prefab needed.
/// </summary>
public class Leaderboard_UI : MonoBehaviour
{
    [Header("Look & feel")]
    public Color PanelColor = new Color(0f, 0f, 0f, 0.78f);
    public Color TitleColor = new Color(1f, 0.85f, 0.2f);
    public Color RowColor   = Color.white;
    public Color HighlightColor = new Color(1f, 0.85f, 0.2f);
    public int   TitleSize  = 48;
    public int   RowSize    = 28;

    private GameObject panel;     // root of the built panel
    private bool visible = false;

    /// <summary>Wire this to a Button's OnClick from the Inspector.</summary>
    public void Toggle()
    {
        if (panel == null) Build();
        visible = !visible;
        panel.SetActive(visible);
        if (visible) Refresh();
    }

    public void Show()
    {
        if (panel == null) Build();
        visible = true;
        panel.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        if (panel == null) return;
        visible = false;
        panel.SetActive(false);
    }

    // ---------- build the panel once ----------

    void Build()
    {
        // Find a Canvas to parent the panel to. We pick the first one in the
        // scene so it works whether you have Main_Canvas or Background_Canvas.
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Leaderboard_UI: no Canvas in scene; cannot build panel.");
            return;
        }

        // Root panel — full-screen darkened backdrop, centered card on top
        panel = new GameObject("Leaderboard_Panel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);   // dim the menu behind

        // Tap anywhere on the dim backdrop to close
        Button backdropBtn = panel.AddComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;
        backdropBtn.onClick.AddListener(Hide);

        // The card — centered box that holds the rows
        GameObject card = new GameObject("Card");
        card.transform.SetParent(panel.transform, false);
        RectTransform cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(700, 720);
        cardRT.anchoredPosition = Vector2.zero;

        Image cardBg = card.AddComponent<Image>();
        cardBg.color = PanelColor;
        // Block clicks on the card so they don't fall through to the backdrop
        card.AddComponent<CanvasGroup>().blocksRaycasts = true;

        // Title — "LEADERBOARD"
        AddText(card.transform, "Title", "LEADERBOARD",
                TitleSize, TitleColor, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(680, 80));

        // Rows — built empty here, filled by Refresh()
        for (int i = 0; i < Leaderboard_Manager.MaxEntries; i++)
        {
            float y = -110f - i * 52f;
            AddText(card.transform, "Row_" + i, "",
                    RowSize, RowColor, TextAnchor.MiddleLeft,
                    new Vector2(0.5f, 1f), new Vector2(0, y), new Vector2(620, 48));
        }

        // Close button at the bottom
        GameObject closeObj = new GameObject("Close_Button");
        closeObj.transform.SetParent(card.transform, false);
        RectTransform closeRT = closeObj.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(0.5f, 0f);
        closeRT.anchorMax = new Vector2(0.5f, 0f);
        closeRT.pivot     = new Vector2(0.5f, 0f);
        closeRT.sizeDelta = new Vector2(220, 60);
        closeRT.anchoredPosition = new Vector2(0, 30);

        Image closeBg = closeObj.AddComponent<Image>();
        closeBg.color = new Color(1f, 1f, 1f, 0.18f);
        Button closeBtn = closeObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(Hide);

        AddText(closeObj.transform, "Label", "Close",
                28, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 60));
    }

    // ---------- refresh rows from Leaderboard_Manager ----------

    void Refresh()
    {
        if (panel == null) return;

        Transform card = panel.transform.Find("Card");
        if (card == null) return;

        var entries = Leaderboard_Manager.Get_Entries();

        for (int i = 0; i < Leaderboard_Manager.MaxEntries; i++)
        {
            Transform row = card.Find("Row_" + i);
            if (row == null) continue;
            Text t = row.GetComponent<Text>();
            if (t == null) continue;

            if (i < entries.Count)
            {
                Leaderboard_Manager.Entry e = entries[i];
                // Pad rank/name so columns line up with a monospace-ish layout
                string rank  = (i + 1).ToString().PadLeft(2);
                string name  = e.Name.PadRight(16).Substring(0, 16);
                string score = e.Score.ToString("N0").PadLeft(8);
                t.text  = rank + ".  " + name + "  " + score;
                t.color = (i == 0) ? HighlightColor : RowColor;
            }
            else
            {
                t.text  = (i + 1).ToString().PadLeft(2) + ".  ---";
                t.color = new Color(RowColor.r, RowColor.g, RowColor.b, 0.4f);
            }
        }
    }

    // ---------- helper: spawn a Text child with the given layout ----------

    static Text AddText(Transform parent, string name, string content,
                        int size, Color color, TextAnchor align,
                        Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot     = anchor;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;

        Text t = obj.AddComponent<Text>();
        // Built-in legacy font — guaranteed to exist so we don't need a font asset.
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }
}
