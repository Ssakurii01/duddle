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

    void Start()
    {
        ApplyBackground();
        if (HideOldTextTitle) HideTitle();
        AdjustPlayButton();
    }

    // ---------- Background ----------

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

        // Try to find an existing Background GameObject; otherwise create one.
        GameObject bg = GameObject.Find("Background");
        if (bg == null)
        {
            // Find a Canvas to put the background on
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform, false);
            // Put behind everything
            bg.transform.SetAsFirstSibling();
            bg.AddComponent<Image>();
        }

        Image img = bg.GetComponent<Image>();
        if (img == null) img = bg.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = !CoverFullScreen;
        img.raycastTarget = false;

        RectTransform rt = bg.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Stretch to fill the parent (canvas)
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
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
}
