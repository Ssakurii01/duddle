using System.Collections;
using System.Collections.Generic;
using Luxodd.Game.Scripts.Game.Leaderboard;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Jungle-themed leaderboard panel with animations.
/// Pops up automatically on game over. Built fully in code — no prefab.
/// </summary>
public class Leaderboard_UI : MonoBehaviour
{
    // -------- Jungle theme colors --------
    static readonly Color BackdropDim   = new Color(0f, 0.05f, 0f, 0.75f);
    static readonly Color CardOuter     = new Color(0.10f, 0.30f, 0.15f, 1f);
    static readonly Color CardInner     = new Color(0.16f, 0.42f, 0.22f, 1f);
    static readonly Color TitleBg       = new Color(0.45f, 0.65f, 0.20f, 1f);
    static readonly Color TitleText     = new Color(1f, 0.92f, 0.30f, 1f);
    static readonly Color CrownColor    = new Color(1f, 0.85f, 0.10f, 1f);
    static readonly Color RankCircle    = new Color(1f, 0.78f, 0.18f, 1f);
    static readonly Color RankText      = new Color(0.20f, 0.30f, 0.10f, 1f);
    static readonly Color AvatarBg      = new Color(0.22f, 0.55f, 0.25f, 1f);
    static readonly Color RowName       = new Color(1f, 1f, 1f, 1f);
    static readonly Color RowScore      = new Color(1f, 0.90f, 0.30f, 1f);
    static readonly Color RowLabel      = new Color(0.85f, 0.95f, 0.70f, 0.85f);
    static readonly Color RowSeparator  = new Color(1f, 1f, 1f, 0.15f);
    static readonly Color HighlightRow  = new Color(0.95f, 0.75f, 0.20f, 0.20f);
    static readonly Color SnakeColor    = new Color(0.5f, 0.85f, 0.3f, 0.7f);
    static readonly Color CloseRed      = new Color(0.85f, 0.25f, 0.25f, 1f);

    GameObject panel;
    bool visible = false;

    // For animations
    Transform crownT;
    Transform snakeLT;
    Transform snakeRT;
    readonly List<RectTransform> rowRTs = new List<RectTransform>();
    readonly List<Vector2> rowTargetPos = new List<Vector2>();
    readonly List<Text> scoreTexts = new List<Text>();
    readonly List<int> scoreValues = new List<int>();
    readonly List<Text> nameTexts = new List<Text>();
    readonly List<Image> rowImages = new List<Image>();

    // Real-time settings (tweak from Inspector if you make these public)
    [Tooltip("Refresh from Luxodd server every N seconds while the panel is visible. 0 = off.")]
    public float AutoRefreshSeconds = 5f;
    Coroutine autoRefreshCoroutine;
    bool fetching = false;
    Text statusText;     // small "Loading..." / "Offline" hint at the top

    // -------- Static helper --------

    public static void ShowAutomatic()
    {
        Leaderboard_UI existing = Object.FindFirstObjectByType<Leaderboard_UI>();
        if (existing != null)
        {
            existing.Show();
            return;
        }

        GameObject host = new GameObject("Leaderboard_UI_Host");
        Leaderboard_UI ui = host.AddComponent<Leaderboard_UI>();
        ui.Show();
    }

    public void Toggle()
    {
        if (panel == null) Build();
        visible = !visible;
        panel.SetActive(visible);
        if (visible)
        {
            StartCoroutine(PlayInAnimations());
            RefreshData();
            StartAutoRefresh();
        }
        else StopAutoRefresh();
    }

    public void Show()
    {
        if (panel == null) Build();
        visible = true;
        panel.SetActive(true);
        StartCoroutine(PlayInAnimations());
        RefreshData();
        StartAutoRefresh();
    }

    public void Hide()
    {
        if (panel == null) return;
        visible = false;
        panel.SetActive(false);
        StopAutoRefresh();
    }

    void StartAutoRefresh()
    {
        StopAutoRefresh();
        if (AutoRefreshSeconds > 0f)
            autoRefreshCoroutine = StartCoroutine(AutoRefreshLoop());
    }

    void StopAutoRefresh()
    {
        if (autoRefreshCoroutine != null)
        {
            StopCoroutine(autoRefreshCoroutine);
            autoRefreshCoroutine = null;
        }
    }

    IEnumerator AutoRefreshLoop()
    {
        while (visible)
        {
            yield return new WaitForSecondsRealtime(AutoRefreshSeconds);
            if (!visible) yield break;
            RefreshData();
        }
    }

    // -------- Update: continuous wiggles --------

    void Update()
    {
        if (!visible) return;

        // Crown wiggle
        if (crownT != null)
        {
            float t = Time.unscaledTime;
            float angle = Mathf.Sin(t * 3f) * 8f;
            float scale = 1f + Mathf.Sin(t * 4f) * 0.05f;
            crownT.localRotation = Quaternion.Euler(0, 0, angle);
            crownT.localScale = new Vector3(scale, scale, 1f);
        }

        // Snake sway
        float swayT = Time.unscaledTime;
        if (snakeLT != null)
            snakeLT.localPosition = new Vector3(-395f + Mathf.Sin(swayT * 2.5f) * 6f,
                                                Mathf.Sin(swayT * 1.7f) * 4f, 0f);
        if (snakeRT != null)
            snakeRT.localPosition = new Vector3(395f - Mathf.Sin(swayT * 2.5f + 1f) * 6f,
                                                Mathf.Sin(swayT * 1.7f + 1f) * 4f, 0f);
    }

    // -------- Build the panel --------

    void Build()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        Transform parent;
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            parent = canvas.transform;
        }
        else
        {
            GameObject canvasObj = new GameObject("Leaderboard_Canvas");
            Canvas c = canvasObj.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 100;
            CanvasScaler s = canvasObj.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1080, 1920);
            s.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
            parent = canvasObj.transform;
        }

        // Backdrop
        panel = new GameObject("Leaderboard_Panel");
        panel.transform.SetParent(parent, false);
        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        Image dim = panel.AddComponent<Image>();
        dim.color = BackdropDim;
        Button block = panel.AddComponent<Button>();
        block.transition = Selectable.Transition.None;

        // Card
        GameObject card = AddPanel(panel.transform, "Card", CardOuter,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880, 1100));

        AddPanel(card.transform, "Inner", CardInner,
            new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(820, 940));

        // Snakes
        snakeLT = BuildSnake(card.transform, true).transform;
        snakeRT = BuildSnake(card.transform, false).transform;

        // Crown
        Text crown = AddText(card.transform, "Crown", "♛",
                100, CrownColor, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0, 60), new Vector2(140, 140));
        crownT = crown.transform;

        // Title bar
        GameObject titleBar = AddPanel(card.transform, "TitleBar", TitleBg,
            new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(700, 110));

        AddText(titleBar.transform, "TitleText", "LEADERBOARD",
                52, TitleText, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 110));

        // Close (X) button
        GameObject closeObj = new GameObject("Close");
        closeObj.transform.SetParent(card.transform, false);
        Image closeBg = closeObj.AddComponent<Image>();
        closeBg.color = CloseRed;
        Button closeBtn = closeObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(Hide);

        RectTransform closeRT = closeObj.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1f, 1f);
        closeRT.anchorMax = new Vector2(1f, 1f);
        closeRT.pivot = new Vector2(1f, 1f);
        closeRT.sizeDelta = new Vector2(80, 80);
        closeRT.anchoredPosition = new Vector2(-30, -30);

        AddText(closeObj.transform, "X", "X",
                46, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80, 80));

        // Rows
        rowRTs.Clear();
        rowTargetPos.Clear();
        scoreTexts.Clear();
        scoreValues.Clear();
        nameTexts.Clear();
        rowImages.Clear();

        var entries = Leaderboard_Manager.Get_Entries();
        const int rowsToShow = 5;
        float rowHeight = 130f;
        float rowsTopY = -210f;

        for (int i = 0; i < rowsToShow; i++)
        {
            float y = rowsTopY - i * rowHeight;
            BuildRow(card.transform, i, y, i < entries.Count ? entries[i] : default, i < entries.Count);
        }

        // ---------- Status text (top, small) ----------
        statusText = AddText(card.transform, "Status", "",
                18, new Color(1f, 1f, 1f, 0.65f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0, -118), new Vector2(700, 22));

        // ---------- Action buttons: Play Again + Main Menu ----------
        BuildActionButtons(card.transform);

        // Also auto-wire any existing scene buttons named "Play"/"Again"/"Menu"
        AutoWireSceneButtons();

        block.onClick.AddListener(Hide);
    }

    // ------------------------------------------------------------------
    //  Real-time data refresh — Luxodd server first, local fallback.
    // ------------------------------------------------------------------

    public void RefreshData()
    {
        if (fetching) return;

        // Prefer Luxodd if a connected bridge exists
        if (Luxodd_Bridge.Instance != null && Luxodd_Bridge.Instance.IsConnected)
        {
            fetching = true;
            SetStatus("Loading...");
            Luxodd_Bridge.Instance.FetchLeaderboard(response =>
            {
                fetching = false;
                if (response != null && response.Leaderboard != null)
                {
                    DisplayServer(response);
                    SetStatus("Live");
                }
                else
                {
                    DisplayLocal();
                    SetStatus("Offline (local scores)");
                }
            });
        }
        else
        {
            // No bridge / not connected — show local scores
            DisplayLocal();
            SetStatus(Luxodd_Bridge.Instance == null ? "" : "Offline (local scores)");
        }
    }

    void SetStatus(string s)
    {
        if (statusText != null) statusText.text = s;
    }

    void DisplayServer(LeaderboardDataResponse response)
    {
        var board = response.Leaderboard ?? new List<LeaderboardData>();
        string currentUser = response.CurrentUserData != null ? response.CurrentUserData.PlayerName : "";

        for (int i = 0; i < nameTexts.Count; i++)
        {
            bool hasEntry = i < board.Count;
            if (hasEntry)
            {
                LeaderboardData e = board[i];
                if (nameTexts[i]  != null) nameTexts[i].text  = string.IsNullOrEmpty(e.PlayerName) ? "Player" : e.PlayerName;
                if (scoreTexts[i] != null) scoreTexts[i].text = e.TotalScore.ToString("N0");
                scoreValues[i] = e.TotalScore;
                ApplyHighlight(i, !string.IsNullOrEmpty(currentUser) && e.PlayerName == currentUser);
            }
            else
            {
                if (nameTexts[i]  != null) nameTexts[i].text  = "---";
                if (scoreTexts[i] != null) scoreTexts[i].text = "0";
                scoreValues[i] = 0;
                ApplyHighlight(i, false);
            }
        }
    }

    void DisplayLocal()
    {
        var entries = Leaderboard_Manager.Get_Entries();
        for (int i = 0; i < nameTexts.Count; i++)
        {
            bool hasEntry = i < entries.Count;
            if (hasEntry)
            {
                Leaderboard_Manager.Entry e = entries[i];
                if (nameTexts[i]  != null) nameTexts[i].text  = string.IsNullOrEmpty(e.Name) ? "Player" : e.Name;
                if (scoreTexts[i] != null) scoreTexts[i].text = e.Score.ToString("N0");
                scoreValues[i] = e.Score;
                ApplyHighlight(i, i == 0); // top row always highlighted in local view
            }
            else
            {
                if (nameTexts[i]  != null) nameTexts[i].text  = "---";
                if (scoreTexts[i] != null) scoreTexts[i].text = "0";
                scoreValues[i] = 0;
                ApplyHighlight(i, false);
            }
        }
    }

    void ApplyHighlight(int index, bool on)
    {
        if (index < 0 || index >= rowImages.Count || rowImages[index] == null) return;
        rowImages[index].color = on ? HighlightRow : new Color(0, 0, 0, 0);
    }

    void BuildActionButtons(Transform card)
    {
        // Play Again — green, on the left
        GameObject playAgain = MakeButton(
            card, "PlayAgain_Button", "PLAY AGAIN",
            new Color(0.30f, 0.75f, 0.35f), new Color(0.20f, 0.55f, 0.22f),
            new Vector2(-170f, 80f), new Vector2(310f, 95f));
        playAgain.GetComponent<Button>().onClick.AddListener(OnPlayAgain);

        // Main Menu — orange, on the right
        GameObject menu = MakeButton(
            card, "Menu_Button", "MAIN MENU",
            new Color(0.95f, 0.55f, 0.20f), new Color(0.65f, 0.30f, 0.10f),
            new Vector2(170f, 80f), new Vector2(310f, 95f));
        menu.GetComponent<Button>().onClick.AddListener(OnMainMenu);
    }

    GameObject MakeButton(Transform parent, string name, string label,
                          Color fillColor, Color outlineColor,
                          Vector2 anchoredPosFromBottom, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = fillColor;

        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        cb.pressedColor     = new Color(0.85f, 0.85f, 0.85f, 1f);
        cb.selectedColor    = Color.white;
        btn.colors = cb;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPosFromBottom;

        Text label_t = AddText(obj.transform, "Label", label,
                28, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, size);

        Outline ol = label_t.gameObject.AddComponent<Outline>();
        ol.effectColor = outlineColor;
        ol.effectDistance = new Vector2(2f, -2f);

        return obj;
    }

    /// <summary>
    /// Bonus: if the user already has Play Again / Main Menu buttons in
    /// the scene (e.g. on Background_Canvas), wire them up automatically.
    /// </summary>
    void AutoWireSceneButtons()
    {
        Button[] all = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button b in all)
        {
            if (b == null) continue;
            // Skip the buttons we just built on the leaderboard
            if (b.gameObject.transform.IsChildOf(panel.transform)) continue;

            string n = b.name.ToLower();
            string txt = "";
            Text t = b.GetComponentInChildren<Text>();
            if (t != null) txt = t.text.ToLower();

            bool isPlayAgain = n.Contains("play") || n.Contains("again") || txt.Contains("play");
            bool isMenu      = n.Contains("menu") || txt.Contains("menu");

            if (isPlayAgain && !isMenu)
            {
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(OnPlayAgain);
            }
            else if (isMenu)
            {
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(OnMainMenu);
            }
        }
    }

    void OnPlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("In_Game");
    }

    void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main_Menu");
    }

    void BuildRow(Transform parent, int index, float y, Leaderboard_Manager.Entry entry, bool hasEntry)
    {
        bool highlight = index == 0 && hasEntry;

        GameObject row = AddPanel(parent, "Row_" + index,
            highlight ? HighlightRow : new Color(0, 0, 0, 0),
            new Vector2(0.5f, 1f), new Vector2(0, y), new Vector2(740, 110));

        // Track for animation + later updates
        RectTransform rowRT = row.GetComponent<RectTransform>();
        rowRTs.Add(rowRT);
        rowTargetPos.Add(new Vector2(0, y));
        rowImages.Add(row.GetComponent<Image>());

        // Separator line at bottom
        AddPanel(row.transform, "Sep", RowSeparator,
            new Vector2(0.5f, 0f), Vector2.zero, new Vector2(700, 2));

        // Rank circle (yellow with number)
        GameObject rankObj = AddPanel(row.transform, "Rank", RankCircle,
            new Vector2(0f, 0.5f), new Vector2(50, 0), new Vector2(70, 70));
        AddText(rankObj.transform, "RankText", (index + 1).ToString(),
                42, RankText, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70, 70));

        // Avatar circle (green with monkey-like symbol)
        GameObject avatarObj = AddPanel(row.transform, "Avatar", AvatarBg,
            new Vector2(0f, 0.5f), new Vector2(140, 0), new Vector2(80, 80));
        AddText(avatarObj.transform, "Monkey", "@",
                48, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80, 80));

        // ----- LEFT side: Player name area -----
        // Tiny label "PLAYER'S NAME" — clearly ABOVE the name
        AddText(row.transform, "NameLabel", "PLAYER'S NAME",
                14, RowLabel, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(195, 24), new Vector2(280, 18));

        // Big player name — clearly BELOW the label
        string name = hasEntry ? entry.Name : "---";
        Text nameText = AddText(row.transform, "Name", name,
                30, RowName, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(195, -12), new Vector2(280, 36));
        nameTexts.Add(nameText);

        // ----- RIGHT side: Score area -----
        // Tiny label "SCORE" — clearly ABOVE the number
        AddText(row.transform, "ScoreLabel", "SCORE",
                14, RowLabel, TextAnchor.MiddleRight,
                new Vector2(1f, 0.5f), new Vector2(-30, 24), new Vector2(180, 18));

        // Big score number — clearly BELOW the label
        Text scoreText = AddText(row.transform, "Score", "0",
                32, RowScore, TextAnchor.MiddleRight,
                new Vector2(1f, 0.5f), new Vector2(-30, -12), new Vector2(180, 36));

        scoreTexts.Add(scoreText);
        scoreValues.Add(hasEntry ? entry.Score : 0);
    }

    GameObject BuildSnake(Transform parent, bool left)
    {
        string side = left ? "Snake_L" : "Snake_R";
        float xPos = left ? -395f : 395f;
        Text snake = AddText(parent, side, "~\n~\n~\n~\n~\n~\n~\n~\n~\n~\n~\n~",
                40, SnakeColor, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(xPos, 0), new Vector2(60, 1000));
        return snake.gameObject;
    }

    // -------- Animations --------

    IEnumerator PlayInAnimations()
    {
        // 1. Card pop-in
        Transform card = panel != null ? panel.transform.Find("Card") : null;
        if (card == null) yield break;

        card.localScale = Vector3.zero;
        float t = 0f;
        float popDur = 0.45f;
        while (t < popDur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / popDur);
            card.localScale = Vector3.one * EaseOutBack(p);
            yield return null;
        }
        card.localScale = Vector3.one;

        // 2. Stagger rows: each slides in from the right with a bounce
        for (int i = 0; i < rowRTs.Count; i++)
        {
            RectTransform rt = rowRTs[i];
            Vector2 target = rowTargetPos[i];
            rt.anchoredPosition = new Vector2(target.x + 800, target.y);
            CanvasGroup cg = rt.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            StartCoroutine(SlideRowIn(rt, target, cg));
            yield return new WaitForSecondsRealtime(0.07f);
        }

        // 3. Score count-up (after rows are in)
        yield return new WaitForSecondsRealtime(0.25f);

        for (int i = 0; i < scoreTexts.Count; i++)
        {
            StartCoroutine(CountUp(scoreTexts[i], scoreValues[i], 0.7f));
        }
    }

    IEnumerator SlideRowIn(RectTransform rt, Vector2 target, CanvasGroup cg)
    {
        float t = 0f;
        float dur = 0.5f;
        Vector2 start = rt.anchoredPosition;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / dur);
            float e = EaseOutBack(p);
            rt.anchoredPosition = Vector2.LerpUnclamped(start, target, e);
            cg.alpha = Mathf.Clamp01(p * 1.5f);
            yield return null;
        }
        rt.anchoredPosition = target;
        cg.alpha = 1f;
    }

    IEnumerator CountUp(Text text, int target, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            // ease-out
            float e = 1f - Mathf.Pow(1f - p, 3f);
            int val = Mathf.RoundToInt(target * e);
            text.text = val.ToString("N0");
            yield return null;
        }
        text.text = target.ToString("N0");
    }

    static float EaseOutBack(float p)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float pp = p - 1f;
        return 1f + c3 * pp * pp * pp + c1 * pp * pp;
    }

    // -------- Helpers --------

    static GameObject AddPanel(Transform parent, string name, Color color,
                               Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        return obj;
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
        rt.pivot = anchor;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;

        Text t = obj.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }
}
