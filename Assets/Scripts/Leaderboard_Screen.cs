using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds a full-screen leaderboard inside its own scene.
/// Just drop this script on an empty GameObject in a new scene called
/// "Leaderboard". Everything (Canvas, title, rows, Back button) is created
/// automatically when the scene starts.
/// </summary>
public class Leaderboard_Screen : MonoBehaviour
{
    [Header("Look")]
    public Color BackgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
    public Color CardColor       = new Color(1f, 1f, 1f, 0.95f);
    public Color TitleColor      = new Color(0.15f, 0.15f, 0.2f);
    public Color RowColor        = new Color(0.15f, 0.15f, 0.2f);
    public Color HighlightColor  = new Color(0.95f, 0.6f, 0.1f);

    [Header("Navigation")]
    public string MenuSceneName = "Main_Menu";

    void Start()
    {
        Leaderboard_Manager.Load();
        BuildScreen();
    }

    void BuildScreen()
    {
        // Root canvas — full screen
        GameObject canvasObj = new GameObject("Leaderboard_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Background — fills the screen
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvas.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = BackgroundColor;
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Card — the white panel in the center
        GameObject card = new GameObject("Card");
        card.transform.SetParent(canvas.transform, false);
        Image cardImg = card.AddComponent<Image>();
        cardImg.color = CardColor;
        RectTransform cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.pivot     = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(900, 850);
        cardRT.anchoredPosition = Vector2.zero;

        // Title — "LEADERBOARD"
        AddText(card.transform, "Title", "LEADERBOARD",
                64, TitleColor, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(880, 90));

        // Rows
        var entries = Leaderboard_Manager.Get_Entries();
        for (int i = 0; i < Leaderboard_Manager.MaxEntries; i++)
        {
            float y = -160f - i * 58f;
            string content;
            Color color;

            if (i < entries.Count)
            {
                Leaderboard_Manager.Entry e = entries[i];
                string rank  = (i + 1).ToString().PadLeft(2);
                string name  = e.Name.PadRight(16).Substring(0, 16);
                string score = e.Score.ToString("N0").PadLeft(10);
                content = rank + ".  " + name + "  " + score;
                color = (i == 0) ? HighlightColor : RowColor;
            }
            else
            {
                content = (i + 1).ToString().PadLeft(2) + ".  ---";
                color = new Color(RowColor.r, RowColor.g, RowColor.b, 0.4f);
            }

            AddText(card.transform, "Row_" + i, content,
                    32, color, TextAnchor.MiddleLeft,
                    new Vector2(0.5f, 1f), new Vector2(0, y), new Vector2(800, 50));
        }

        // Back button at the bottom of the card
        GameObject backObj = new GameObject("Back_Button");
        backObj.transform.SetParent(card.transform, false);
        Image backBg = backObj.AddComponent<Image>();
        backBg.color = HighlightColor;
        Button backBtn = backObj.AddComponent<Button>();
        backBtn.onClick.AddListener(OnBackClicked);

        RectTransform backRT = backObj.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0.5f, 0f);
        backRT.anchorMax = new Vector2(0.5f, 0f);
        backRT.pivot     = new Vector2(0.5f, 0f);
        backRT.sizeDelta = new Vector2(280, 80);
        backRT.anchoredPosition = new Vector2(0, 40);

        AddText(backObj.transform, "Label", "Back to Menu",
                32, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 80));
    }

    void OnBackClicked()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

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
