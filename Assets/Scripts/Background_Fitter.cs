using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drop on any GameObject that holds a background SpriteRenderer or UI Image
/// to force-fill the entire camera view on every scene load / window resize.
///
/// Use this when ScreenSetup's auto-detection misses your background because
/// the GameObject is named something like "Forest" or "Jungle" instead of
/// "Background".
/// </summary>
// Execute very late so we override Parallax_Layer (which writes the
// background's world position in LateUpdate). Without this, parallax would
// move the background out from under the camera every frame.
[DefaultExecutionOrder(500)]
[ExecuteAlways]
public class Background_Fitter : MonoBehaviour
{
    public enum FitMode
    {
        Cover,   // fill the entire view, crop overflow
        Contain  // fit inside, may leave bars
    }

    [Tooltip("Cover = fill screen (may crop edges). Contain = fit inside (may leave bars).")]
    public FitMode Mode = FitMode.Cover;

    [Tooltip("Re-fit every frame so the background follows the camera and screen resizes.")]
    public bool FitEveryFrame = true;

    void Start()  { Fit(); }
    void OnEnable() { Fit(); }
    void LateUpdate()
    {
        if (FitEveryFrame) Fit();
    }

    public void Fit()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            FitSpriteRenderer(sr);
            return;
        }

        Image img = GetComponent<Image>();
        if (img != null)
        {
            FitImage(img);
        }
    }

    void FitSpriteRenderer(SpriteRenderer sr)
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth  = camHeight * cam.aspect;

        Vector3 size = sr.sprite.bounds.size; // local-space size at scale 1
        if (size.x <= 0f || size.y <= 0f) return;

        float sx = camWidth  / size.x;
        float sy = camHeight / size.y;
        float s  = Mode == FitMode.Cover ? Mathf.Max(sx, sy) : Mathf.Min(sx, sy);

        transform.localScale = new Vector3(s, s, transform.localScale.z);

        // Center on the camera so it never drifts away.
        Vector3 p = transform.position;
        p.x = cam.transform.position.x;
        p.y = cam.transform.position.y;
        transform.position = p;
    }

    void FitImage(Image img)
    {
        // For UI: stretch to fill the parent canvas.
        RectTransform rt = img.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        img.preserveAspect = Mode == FitMode.Contain;
    }
}
