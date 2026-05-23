using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drop on a background SpriteRenderer (or UI Image) to keep it filling the
/// camera view as the player climbs.
///
/// • Sprite renderers: scaled once on Start to cover the camera, then
///   re-centered on the camera every LateUpdate so they follow the player
///   as the camera moves up. Set ScaleEveryFrame if the camera's
///   orthographicSize changes during play and you need to react.
/// • UI Images: just stretched edge-to-edge inside the parent canvas.
///
/// Runs only at play time so it cannot accidentally rewrite the scene from
/// the Editor.
/// </summary>
[DefaultExecutionOrder(500)] // after Parallax_Layer's LateUpdate so we win
public class Background_Fitter : MonoBehaviour
{
    public enum FitMode
    {
        Cover,   // fill the entire view, crop overflow
        Contain  // fit inside, may leave bars
    }

    [Tooltip("Cover = fill screen (may crop edges). Contain = fit inside (may leave bars).")]
    public FitMode Mode = FitMode.Cover;

    [Tooltip("Recompute scale every frame. Leave OFF unless the camera's orthographicSize changes during play.")]
    public bool ScaleEveryFrame = false;

    [Tooltip("Re-center the sprite on the camera every frame (so it follows the player up).")]
    public bool FollowCamera = true;

    void Start()
    {
        // One-time scale + center on Start. Avoids fighting other systems
        // (Sky_Gradient, Parallax_Layer) in the editor.
        ApplyScale();
        Recenter();
    }

    void LateUpdate()
    {
        if (ScaleEveryFrame) ApplyScale();
        if (FollowCamera)    Recenter();
    }

    void ApplyScale()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            FitSpriteRendererScale(sr);
            return;
        }
        Image img = GetComponent<Image>();
        if (img != null)
        {
            FitImage(img);
        }
    }

    void Recenter()
    {
        if (GetComponent<SpriteRenderer>() == null) return; // UI Images stay where the canvas puts them
        Camera cam = Camera.main;
        if (cam == null) return;
        Vector3 p = transform.position;
        p.x = cam.transform.position.x;
        p.y = cam.transform.position.y;
        transform.position = p;
    }

    void FitSpriteRendererScale(SpriteRenderer sr)
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
    }

    void FitImage(Image img)
    {
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
