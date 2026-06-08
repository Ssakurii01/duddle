using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns a small, always-visible "PRESS ANY BUTTON TO START" hint over the
/// menu so a first-time arcade player can see what to do. Adds a dedicated
/// ScreenSpaceOverlay canvas with the hint text — no manual scene wiring
/// needed; just attach this to any GameObject in the Main_Menu scene
/// (Menu_Setup, Game_Controller, an EventSystem, anything).
/// </summary>
public class Controls_Hint : MonoBehaviour
{
    [Tooltip("Text to show. Use {} as a placeholder for an animated dot.")]
    public string Message = "PRESS  ANY  BUTTON  TO  START";

    [Tooltip("Distance from the bottom of the screen, in canvas pixels.")]
    public float BottomMargin = 80f;

    [Tooltip("Font size.")]
    public int FontSize = 36;

    public Color TextColor = new Color(1f, 0.95f, 0.4f, 1f);
    public Color OutlineColor = new Color(0.10f, 0.10f, 0.10f, 0.85f);

    [Tooltip("How fast the hint pulses (0 = no pulse).")]
    public float PulseSpeed = 2f;

    Text label;

    void Start()
    {
        Build();
    }

    void Update()
    {
        if (label == null || PulseSpeed <= 0f) return;
        float a = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * PulseSpeed));
        Color c = TextColor;
        c.a = a;
        label.color = c;
    }

    void Build()
    {
        Canvas canvas = GetOrCreateHintCanvas();
        if (canvas == null) return;

        GameObject obj = new GameObject("Controls_Hint_Text");
        obj.transform.SetParent(canvas.transform, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(1600f, 80f);
        rt.anchoredPosition = new Vector2(0f, BottomMargin);

        label = obj.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = Message;
        label.fontSize = FontSize;
        label.color = TextColor;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow   = VerticalWrapMode.Overflow;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = OutlineColor;
        outline.effectDistance = new Vector2(2.5f, -2.5f);

        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        shadow.effectDistance = new Vector2(3f, -3f);
    }

    Canvas GetOrCreateHintCanvas()
    {
        const string CanvasName = "Controls_Hint_Canvas";
        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null)
        {
            Canvas c = existing.GetComponent<Canvas>();
            if (c != null) return c;
        }

        GameObject obj = new GameObject(CanvasName);
        Canvas canvas = obj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // above everything but the leaderboard popup

        CanvasScaler s = obj.AddComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920, 1080);
        s.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        obj.AddComponent<GraphicRaycaster>();
        return canvas;
    }
}
