using UnityEngine;
using TMPro;

/// <summary>
/// Attach to any UI element (TMP text, Image, anything with a RectTransform) to add
/// toggleable bob / pulse / wiggle / rainbow animations. Uses unscaled time so it keeps
/// working while the game is paused (e.g. during the pre-start freeze).
/// </summary>
public class UI_TextAnimator : MonoBehaviour
{
    [Header("Float (vertical bob)")]
    public bool Float_Enabled = true;
    public float Float_Amplitude = 8f;
    public float Float_Speed = 2f;

    [Header("Pulse (scale)")]
    public bool Pulse_Enabled = true;
    public float Pulse_Amplitude = 0.05f;
    public float Pulse_Speed = 2f;

    [Header("Wiggle (rotation)")]
    public bool Wiggle_Enabled = false;
    public float Wiggle_Amplitude = 3f;
    public float Wiggle_Speed = 3f;

    [Header("Rainbow (color cycle, requires TMP text)")]
    public bool Rainbow_Enabled = false;
    public float Rainbow_Speed = 0.3f;
    public float Rainbow_Saturation = 0.8f;

    private RectTransform rect;
    private TMP_Text tmp;
    private Vector2 baseAnchored;
    private Vector3 baseScale;
    private Quaternion baseRotation;
    private Color baseColor;
    private float seed;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        tmp = GetComponent<TMP_Text>();

        if (rect != null)
        {
            baseAnchored = rect.anchoredPosition;
            baseScale = rect.localScale;
            baseRotation = rect.localRotation;
        }
        if (tmp != null) baseColor = tmp.color;

        seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (rect == null) return;

        float t = Time.unscaledTime + seed;

        Vector2 pos = baseAnchored;
        if (Float_Enabled) pos.y += Mathf.Sin(t * Float_Speed) * Float_Amplitude;
        rect.anchoredPosition = pos;

        Vector3 scale = baseScale;
        if (Pulse_Enabled) scale *= 1f + Mathf.Sin(t * Pulse_Speed) * Pulse_Amplitude;
        rect.localScale = scale;

        Quaternion rot = baseRotation;
        if (Wiggle_Enabled) rot *= Quaternion.Euler(0f, 0f, Mathf.Sin(t * Wiggle_Speed) * Wiggle_Amplitude);
        rect.localRotation = rot;

        if (Rainbow_Enabled && tmp != null)
        {
            float hue = Mathf.Repeat(t * Rainbow_Speed, 1f);
            tmp.color = Color.HSVToRGB(hue, Rainbow_Saturation, 1f);
        }
        else if (!Rainbow_Enabled && tmp != null && tmp.color != baseColor)
        {
            // Don't clobber — some outside code (urgency colors) may own the color.
            // Leave it alone.
        }
    }
}
