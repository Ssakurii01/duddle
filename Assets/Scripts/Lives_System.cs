using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lives system for Monkey Jump:
/// - Builds 3 stylish hearts (gold badge + jungle ring + glowing red heart)
///   at the top-center of the screen.
/// - Hearts pulse like a heartbeat and gently float.
/// - When the player falls, removes 1 heart, respawns at the camera center
///   with a 3-2-1 countdown.
/// - When all hearts are gone, returns false from LoseLifeAndRespawn so
///   Game_Controller triggers normal game over (which shows the leaderboard).
/// </summary>
public class Lives_System : MonoBehaviour
{
    public static Lives_System Instance;

    public const int MaxLives = 3;
    public static int CurrentLives;
    public static bool IsRespawning;

    [Header("Heart colors")]
    public Color HeartFull       = new Color(1.00f, 0.25f, 0.35f);  // vibrant red
    public Color HeartEmpty      = new Color(0.30f, 0.30f, 0.30f, 0.55f); // grey
    public Color HeartGlow       = new Color(1f, 0.40f, 0.45f, 0.45f);
    public Color BadgeGold       = new Color(1.00f, 0.82f, 0.18f);
    public Color BadgeRing       = new Color(1.00f, 0.95f, 0.55f);
    public Color BadgeInner      = new Color(0.16f, 0.42f, 0.22f);  // jungle green
    public Color BadgeInnerEmpty = new Color(0.20f, 0.20f, 0.22f);
    public Color CountdownColor  = new Color(1f, 0.85f, 0.2f);

    [Header("Respawn")]
    public float CountdownSeconds = 3f;
    public float RespawnUpBoost   = 12f;
    public float RespawnAboveCamera = 1.5f;

    // -------- runtime --------
    Text[] heartTexts;
    Text[] heartGlows;
    Image[] badgeOuter;
    Image[] badgeInner;
    RectTransform[] heartParents;
    Vector2[] heartBasePos;

    Text countdownText;
    RectTransform countdownRT;

    void Awake()
    {
        Instance = this;
        CurrentLives = MaxLives;
        IsRespawning = false;
    }

    void Start()
    {
        BuildHeartsUI();
        BuildCountdownUI();
        UpdateHeartsVisual(animate: false);
    }

    void Update()
    {
        if (heartTexts == null) return;

        float now = Time.unscaledTime;
        for (int i = 0; i < heartTexts.Length; i++)
        {
            if (heartTexts[i] == null || heartParents[i] == null) continue;

            // Floating bob for the whole heart group
            float bobY = Mathf.Sin(now * 1.6f + i * 0.7f) * 3.5f;
            heartParents[i].anchoredPosition = heartBasePos[i] + new Vector2(0, bobY);

            // Heartbeat pulse — only for FULL hearts
            if (i < CurrentLives)
            {
                float beat = 1f + Mathf.Sin(now * 4.5f + i * 0.4f) * 0.10f;
                heartTexts[i].transform.localScale = Vector3.one * beat;

                // Glow pulse — opposite phase, larger
                if (heartGlows[i] != null)
                {
                    float glowScale = 1f + Mathf.Sin(now * 3f + i * 0.4f) * 0.18f;
                    heartGlows[i].transform.localScale = Vector3.one * glowScale * 1.3f;
                    Color gc = heartGlows[i].color;
                    gc.a = 0.30f + Mathf.Sin(now * 3f + i * 0.4f) * 0.20f;
                    heartGlows[i].color = gc;
                }
            }
        }
    }

    // -------- Public API --------

    public bool LoseLifeAndRespawn(GameObject player)
    {
        if (CurrentLives <= 1)
        {
            CurrentLives = 0;
            UpdateHeartsVisual(animate: true);
            return false;
        }

        CurrentLives--;
        UpdateHeartsVisual(animate: true);
        StartCoroutine(RespawnSequence(player));
        return true;
    }

    // -------- Respawn flow --------

    IEnumerator RespawnSequence(GameObject player)
    {
        IsRespawning = true;

        // Burst particles at the heart we just lost
        if (heartParents != null && CurrentLives < heartParents.Length && heartParents[CurrentLives] != null)
        {
            // Camera-space-ish — use Jump_Particles burst at world center for now
            Camera_Shake.Shake(0.3f, 0.25f);
        }

        // Freeze player physics
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Teleport to safe spot above camera center
        Camera cam = Camera.main;
        Vector3 spawn = new Vector3(0f, cam.transform.position.y + RespawnAboveCamera, 0f);
        player.transform.position = spawn;
        Game_Controller.ComboCount = 0;
        Game_Controller.ScoreMultiplier = 1f;

        // Countdown 3 → 2 → 1
        int count = Mathf.RoundToInt(CountdownSeconds);
        for (int i = count; i >= 1; i--)
        {
            yield return StartCoroutine(ShowCount(i));
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = new Vector2(0f, RespawnUpBoost);
        }

        IsRespawning = false;
    }

    IEnumerator ShowCount(int n)
    {
        if (countdownText == null) yield break;

        countdownText.gameObject.SetActive(true);
        countdownText.text = n.ToString();
        countdownText.color = CountdownColor;

        float t = 0f;
        float dur = 1f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / dur);
            float scale = Mathf.Lerp(2.2f, 0.85f, Mathf.Sqrt(p));
            countdownRT.localScale = Vector3.one * scale;
            float a = (p < 0.7f) ? 1f : Mathf.Lerp(1f, 0f, (p - 0.7f) / 0.3f);
            Color c = countdownText.color; c.a = a; countdownText.color = c;
            yield return null;
        }
    }

    // -------- UI build --------

    Canvas FindOverlayCanvas()
    {
        Canvas[] all = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in all)
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) return c;

        GameObject canvasObj = new GameObject("Lives_Canvas");
        Canvas nc = canvasObj.AddComponent<Canvas>();
        nc.renderMode = RenderMode.ScreenSpaceOverlay;
        nc.sortingOrder = 50;
        CanvasScaler s = canvasObj.AddComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1080, 1920);
        s.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();
        return nc;
    }

    void BuildHeartsUI()
    {
        Canvas canvas = FindOverlayCanvas();
        if (canvas == null) return;

        GameObject container = new GameObject("Hearts_UI");
        container.transform.SetParent(canvas.transform, false);
        RectTransform crt = container.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 1f);
        crt.anchorMax = new Vector2(0.5f, 1f);
        crt.pivot     = new Vector2(0.5f, 1f);
        crt.sizeDelta = new Vector2(420, 110);
        crt.anchoredPosition = new Vector2(0f, -25f);

        heartTexts = new Text[MaxLives];
        heartGlows = new Text[MaxLives];
        badgeOuter = new Image[MaxLives];
        badgeInner = new Image[MaxLives];
        heartParents = new RectTransform[MaxLives];
        heartBasePos = new Vector2[MaxLives];

        for (int i = 0; i < MaxLives; i++)
        {
            // ---- parent group ----
            GameObject group = new GameObject("Heart_" + i);
            group.transform.SetParent(container.transform, false);
            RectTransform gRT = group.AddComponent<RectTransform>();
            gRT.anchorMin = new Vector2(0.5f, 0.5f);
            gRT.anchorMax = new Vector2(0.5f, 0.5f);
            gRT.pivot     = new Vector2(0.5f, 0.5f);
            gRT.sizeDelta = new Vector2(96, 96);
            Vector2 pos = new Vector2((i - 1) * 110f, 0f);
            gRT.anchoredPosition = pos;
            heartParents[i] = gRT;
            heartBasePos[i] = pos;

            // ---- outer gold badge (circle) ----
            GameObject outer = new GameObject("BadgeOuter");
            outer.transform.SetParent(group.transform, false);
            Image outerImg = outer.AddComponent<Image>();
            outerImg.color = BadgeGold;
            outerImg.sprite = BuildCircleSprite();
            RectTransform oRT = outer.GetComponent<RectTransform>();
            oRT.anchorMin = new Vector2(0.5f, 0.5f);
            oRT.anchorMax = new Vector2(0.5f, 0.5f);
            oRT.pivot     = new Vector2(0.5f, 0.5f);
            oRT.sizeDelta = new Vector2(86, 86);
            badgeOuter[i] = outerImg;

            // ---- gold ring (lighter, smaller) ----
            GameObject ring = new GameObject("BadgeRing");
            ring.transform.SetParent(group.transform, false);
            Image ringImg = ring.AddComponent<Image>();
            ringImg.color = BadgeRing;
            ringImg.sprite = BuildCircleSprite();
            RectTransform rRT = ring.GetComponent<RectTransform>();
            rRT.anchorMin = new Vector2(0.5f, 0.5f);
            rRT.anchorMax = new Vector2(0.5f, 0.5f);
            rRT.pivot     = new Vector2(0.5f, 0.5f);
            rRT.sizeDelta = new Vector2(74, 74);

            // ---- inner jungle-green disc ----
            GameObject inner = new GameObject("BadgeInner");
            inner.transform.SetParent(group.transform, false);
            Image innerImg = inner.AddComponent<Image>();
            innerImg.color = BadgeInner;
            innerImg.sprite = BuildCircleSprite();
            RectTransform iRT = inner.GetComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0.5f, 0.5f);
            iRT.anchorMax = new Vector2(0.5f, 0.5f);
            iRT.pivot     = new Vector2(0.5f, 0.5f);
            iRT.sizeDelta = new Vector2(64, 64);
            badgeInner[i] = innerImg;

            // ---- glow heart (behind, soft) ----
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(group.transform, false);
            Text glowT = AddHeartText(glow, "♥", 60, HeartGlow);
            RectTransform glRT = glow.GetComponent<RectTransform>();
            glRT.sizeDelta = new Vector2(80, 80);
            heartGlows[i] = glowT;

            // ---- main heart with outline + shadow ----
            GameObject main = new GameObject("Heart");
            main.transform.SetParent(group.transform, false);
            Text mainT = AddHeartText(main, "♥", 50, HeartFull);
            RectTransform mRT = main.GetComponent<RectTransform>();
            mRT.sizeDelta = new Vector2(80, 80);

            // White outline ring
            Outline outline = main.AddComponent<Outline>();
            outline.effectColor = new Color(0.6f, 0.05f, 0.10f, 0.85f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            // Drop shadow
            Shadow drop = main.AddComponent<Shadow>();
            drop.effectColor = new Color(0f, 0f, 0f, 0.5f);
            drop.effectDistance = new Vector2(3f, -3f);

            heartTexts[i] = mainT;
        }
    }

    void BuildCountdownUI()
    {
        Canvas canvas = FindOverlayCanvas();
        if (canvas == null) return;

        GameObject obj = new GameObject("Respawn_Countdown");
        obj.transform.SetParent(canvas.transform, false);

        countdownRT = obj.AddComponent<RectTransform>();
        countdownRT.anchorMin = new Vector2(0.5f, 0.5f);
        countdownRT.anchorMax = new Vector2(0.5f, 0.5f);
        countdownRT.pivot     = new Vector2(0.5f, 0.5f);
        countdownRT.sizeDelta = new Vector2(400, 400);
        countdownRT.anchoredPosition = Vector2.zero;

        countdownText = obj.AddComponent<Text>();
        countdownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countdownText.text = "";
        countdownText.fontSize = 220;
        countdownText.color = CountdownColor;
        countdownText.alignment = TextAnchor.MiddleCenter;
        countdownText.raycastTarget = false;
        countdownText.horizontalOverflow = HorizontalWrapMode.Overflow;
        countdownText.verticalOverflow = VerticalWrapMode.Overflow;

        Outline ol = obj.AddComponent<Outline>();
        ol.effectColor = new Color(0f, 0f, 0f, 0.8f);
        ol.effectDistance = new Vector2(5f, -5f);

        Shadow sh = obj.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.6f);
        sh.effectDistance = new Vector2(8f, -8f);

        obj.SetActive(false);
    }

    void UpdateHeartsVisual(bool animate)
    {
        if (heartTexts == null) return;
        for (int i = 0; i < heartTexts.Length; i++)
        {
            bool alive = i < CurrentLives;
            heartTexts[i].color = alive ? HeartFull : HeartEmpty;

            if (heartGlows[i] != null)
                heartGlows[i].gameObject.SetActive(alive);

            if (badgeInner[i] != null)
                badgeInner[i].color = alive ? BadgeInner : BadgeInnerEmpty;
            if (badgeOuter[i] != null)
                badgeOuter[i].color = alive ? BadgeGold : new Color(0.55f, 0.50f, 0.30f, 0.7f);

            if (animate && !alive)
                StartCoroutine(BreakHeartFx(heartTexts[i].transform));
        }
    }

    IEnumerator BreakHeartFx(Transform heart)
    {
        Vector3 baseS = heart.localScale;
        // Quick big squeeze + shake
        for (int k = 0; k < 6; k++)
        {
            float angle = (k % 2 == 0) ? 18f : -18f;
            heart.localRotation = Quaternion.Euler(0, 0, angle);
            yield return new WaitForSecondsRealtime(0.04f);
        }
        heart.localRotation = Quaternion.identity;

        float t = 0f;
        while (t < 0.18f)
        {
            t += Time.unscaledDeltaTime;
            heart.localScale = Vector3.Lerp(baseS * 1.4f, baseS, t / 0.18f);
            yield return null;
        }
        heart.localScale = baseS;
    }

    // -------- helpers --------

    static Text AddHeartText(GameObject obj, string ch, int size, Color color)
    {
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);

        Text t = obj.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = ch;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    // Build a runtime circle sprite once and cache it
    static Sprite cachedCircle;
    static Sprite BuildCircleSprite()
    {
        if (cachedCircle != null) return cachedCircle;
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 c = new Vector2(size / 2f, size / 2f);
        float r = size / 2f - 1f;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                float a = Mathf.Clamp01((r - d) * 1.0f);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }
        tex.Apply();
        cachedCircle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return cachedCircle;
    }
}
