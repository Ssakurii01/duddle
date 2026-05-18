using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main-menu setup helper.
/// Loads a sprite from Assets/Resources/ as the menu background, hides any
/// old text "Title" so the wooden sign in the image is the only title,
/// and parks the Play button safely below the title area.
/// Drop this on any GameObject in the Main_Menu scene.
/// </summary>
public class Menu_Setup : MonoBehaviour
{
    [Header("Background image")]
    [Tooltip("Sprite file name (no extension) inside Assets/Resources/. " +
             "Default: MenuBackground -> looks for Assets/Resources/MenuBackground.png")]
    public string BackgroundSpriteName = "MenuBackground";

    [Tooltip("If true, the image stretches to fully cover the screen (some edges may crop).")]
    public bool CoverFullScreen = true;

    [Header("Title (existing text)")]
    [Tooltip("Hide any old GameObject named 'Title' so it doesn't overlap the wooden sign in the image.")]
    public bool HideOldTextTitle = true;

    [Header("Play button")]
    [Tooltip("Where to put the Play button (relative to screen center). Negative Y = below center.")]
    public Vector2 PlayButtonAnchoredPosition = new Vector2(0f, -380f);
    public Vector2 PlayButtonSize             = new Vector2(320f, 130f);

    [Tooltip("Empty space reserved under the title — Play button is moved at least this far below center.")]
    public float TitleSafeAreaBelowCenter = 350f;

    [Header("Auto-start timer ('Auto starts in N seconds')")]
    [Tooltip("Move the timer text to a fixed spot next to the Play button so it never gets clipped on wide WebGL screens.")]
    public bool RepositionTimer = true;
    [Tooltip("Pixels of empty space between the right edge of the Play button and the left edge of the timer.")]
    public float TimerGapFromButton = 30f;
    [Tooltip("Extra fine-tune offset added on top of the auto-computed right-of-button position.")]
    public Vector2 TimerExtraOffset = Vector2.zero;
    public Vector2 TimerSize         = new Vector2(560f, 110f);
    public int     TimerFontSize     = 32;
    [Tooltip("Text alignment. Left makes the timer read like it's labeling the button on its right.")]
    public TextAnchor TimerAlignment = TextAnchor.MiddleLeft;
    [Tooltip("Fallback position used only when the Play button can't be found. Negative Y = below center.")]
    public Vector2 TimerFallbackPosition = new Vector2(420f, -380f);

    void Start()
    {
        ApplyBackground();
        if (HideOldTextTitle) HideTitle();
        AdjustPlayButton();
        if (RepositionTimer) AdjustTimer();
    }

    // ---------- Background ----------

    const string BgCanvasName = "MenuBackground_Fullscreen_Canvas";
    const string BgImageName  = "MenuBackground_Fullscreen_Image";

    void ApplyBackground()
    {
        Sprite sprite = Resources.Load<Sprite>(BackgroundSpriteName);
        if (sprite == null)
        {
            Debug.LogWarning("[Menu_Setup] Background sprite not found at Resources/" +
                             BackgroundSpriteName + ". Save the image to Assets/Resources/" +
                             BackgroundSpriteName + ".png first.");
            return;
        }

        // Build (or reuse) a dedicated ScreenSpaceOverlay canvas sized to the
        // full window. Whatever the original Background's parent canvas was —
        // Screen Space Camera, fixed-aspect, etc. — it gets bypassed.
        Canvas bgCanvas = GetOrCreateFullscreenBgCanvas();

        // Find / create our background image inside that canvas
        Transform existing = bgCanvas.transform.Find(BgImageName);
        GameObject bg;
        if (existing != null)
        {
            bg = existing.gameObject;
        }
        else
        {
            bg = new GameObject(BgImageName);
            bg.transform.SetParent(bgCanvas.transform, false);
            bg.AddComponent<Image>();
        }

        Image img = bg.GetComponent<Image>();
        if (img == null) img = bg.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = !CoverFullScreen;
        img.raycastTarget = false;

        RectTransform rt = bg.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;

        // Disable any old UI-Image-based "Background" that sat on a smaller
        // canvas — otherwise it would render on top of our fullscreen image.
        // We deliberately skip SpriteRenderer-based world backgrounds.
        HideOverlappingBackgrounds(bg);
    }

    Canvas GetOrCreateFullscreenBgCanvas()
    {
        Canvas[] all = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in all)
        {
            if (c != null && c.gameObject.name == BgCanvasName)
                return c;
        }

        GameObject obj = new GameObject(BgCanvasName);
        Canvas canvas = obj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1000; // render behind every other canvas

        CanvasScaler s = obj.AddComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920, 1080);
        // Expand makes the canvas always cover the full screen on any aspect ratio.
        s.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        obj.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void HideOverlappingBackgrounds(GameObject keep)
    {
        // Hide any UI Image whose name screams "background" and that isn't ours.
        Image[] all = FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image i in all)
        {
            if (i == null || i.gameObject == keep) continue;
            string n = i.gameObject.name.ToLower();
            bool looksLikeBg =
                n == "background" ||
                n == "bg" ||
                n == "background_image" ||
                n.StartsWith("background_") && !i.gameObject.name.Contains("Canvas");
            if (!looksLikeBg) continue;

            // Don't touch the parallax layers — they are gameplay scrolling art,
            // not the menu's "Background" placeholder.
            if (n.Contains("parallax")) continue;

            i.gameObject.SetActive(false);
        }
    }

    // ---------- Hide old text title ----------

    void HideTitle()
    {
        // Hide any GameObject named "Title" (case-sensitive)
        GameObject t = GameObject.Find("Title");
        if (t != null) t.SetActive(false);

        // Some projects name it differently — try a few common ones
        string[] candidates = { "TitleText", "Game_Title", "MonkeyJumpTitle" };
        foreach (string n in candidates)
        {
            GameObject other = GameObject.Find(n);
            if (other != null) other.SetActive(false);
        }
    }

    // ---------- Play button ----------

    void AdjustPlayButton()
    {
        GameObject btn = GameObject.Find("Button_Play");
        if (btn == null)
        {
            // try common alternates
            string[] alts = { "PlayButton", "Play_Button", "Play" };
            foreach (string n in alts)
            {
                btn = GameObject.Find(n);
                if (btn != null) break;
            }
        }
        if (btn == null) return;

        RectTransform rt = btn.GetComponent<RectTransform>();
        if (rt == null) return;

        // Force the button into screen-relative center coords
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);

        // Resize
        rt.sizeDelta = PlayButtonSize;

        // Position — clamp Y so it never overlaps the title area
        float clampedY = Mathf.Min(PlayButtonAnchoredPosition.y, -TitleSafeAreaBelowCenter);
        rt.anchoredPosition = new Vector2(PlayButtonAnchoredPosition.x, clampedY);
    }

    // ---------- Auto-start timer ----------

    void AdjustTimer()
    {
        // Find the timer GameObject by common names. The MainMenu_AutoStart
        // script is what owns the actual TMP_Text reference, so prefer the
        // GameObject that script lives on.
        GameObject timerObj = null;

        var auto = FindFirstObjectByType<MainMenu_AutoStart>();
        if (auto != null && auto.TimerText != null)
            timerObj = auto.TimerText.gameObject;

        if (timerObj == null)
        {
            string[] alts = { "Timer", "TimerText", "AutoStartTimer", "Timer_Text" };
            foreach (string n in alts)
            {
                timerObj = GameObject.Find(n);
                if (timerObj != null) break;
            }
        }
        if (timerObj == null) return;

        RectTransform rt = timerObj.GetComponent<RectTransform>();
        if (rt == null) return;

        // Find the Play button so we can sit immediately to the right of it.
        RectTransform btnRT = FindPlayButtonRect();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = TimerSize;

        if (btnRT != null)
        {
            // Pivot on the LEFT edge of the timer so anchoredPosition is the
            // exact spot where the timer should start.
            rt.pivot = new Vector2(0f, 0.5f);

            Vector2 btnPos  = btnRT.anchoredPosition;
            Vector2 btnSize = btnRT.sizeDelta;

            float startX = btnPos.x + btnSize.x * 0.5f + TimerGapFromButton;
            float y      = btnPos.y;

            rt.anchoredPosition = new Vector2(startX, y) + TimerExtraOffset;
        }
        else
        {
            // No Play button found — fall back to the configured absolute spot.
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = TimerFallbackPosition;
        }

        // Apply text alignment / size — works for either TMP or legacy UI Text
        var tmp = timerObj.GetComponent<TMPro.TMP_Text>();
        if (tmp != null)
        {
            tmp.alignment = ToTmpAlignment(TimerAlignment);
            tmp.fontSize = TimerFontSize;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TMPro.TextOverflowModes.Overflow;
        }
        else
        {
            var legacy = timerObj.GetComponent<UnityEngine.UI.Text>();
            if (legacy != null)
            {
                legacy.alignment = TimerAlignment;
                legacy.fontSize = TimerFontSize;
                legacy.horizontalOverflow = HorizontalWrapMode.Overflow;
                legacy.verticalOverflow = VerticalWrapMode.Overflow;
            }
        }
    }

    static RectTransform FindPlayButtonRect()
    {
        string[] names = { "Button_Play", "PlayButton", "Play_Button", "Play" };
        foreach (string n in names)
        {
            GameObject btn = GameObject.Find(n);
            if (btn == null) continue;
            RectTransform rt = btn.GetComponent<RectTransform>();
            if (rt != null) return rt;
        }
        return null;
    }

    static TMPro.TextAlignmentOptions ToTmpAlignment(TextAnchor a)
    {
        switch (a)
        {
            case TextAnchor.UpperLeft:    return TMPro.TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter:  return TMPro.TextAlignmentOptions.Top;
            case TextAnchor.UpperRight:   return TMPro.TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft:   return TMPro.TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter: return TMPro.TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight:  return TMPro.TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft:    return TMPro.TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter:  return TMPro.TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight:   return TMPro.TextAlignmentOptions.BottomRight;
            default:                      return TMPro.TextAlignmentOptions.Left;
        }
    }
}
