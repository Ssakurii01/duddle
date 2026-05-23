using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Configures the game for Luxodd arcade: 1080x1920 portrait, 9:16.
/// Scales backgrounds and adjusts camera to fill portrait view.
/// </summary>
public class ScreenSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        // Use native screen resolution to avoid black bars
        Resolution native = Screen.currentResolution;
        Screen.SetResolution(native.width, native.height, true);
        Application.targetFrameRate = -1;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnSceneReady()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FixScene();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FixScene();
    }

    static void FixScene()
    {
        FixCameraRect();
        FixCanvasScalers();
        StretchBackgroundImages();
        FitBackgroundsToPortrait();
    }

    static void FixCameraRect()
    {
        // Force every camera to render to the full screen — the previous
        // setup occasionally left letterbox bars on landscape WebGL because
        // a viewport rect smaller than (0,0,1,1) was serialized in the scene.
        Camera[] cams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in cams)
        {
            if (cam == null) continue;
            cam.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }

    // Walk every UI canvas's "Background" Image and force it to stretch
    // edge-to-edge, then put the parent canvas in Expand mode so the
    // background image actually covers the full WebGL window regardless of
    // aspect ratio.
    static void StretchBackgroundImages()
    {
        Image[] all = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image img in all)
        {
            if (img == null) continue;

            string n = img.gameObject.name.ToLower();
            bool isBackground =
                n == "background" ||
                n == "bg" ||
                n == "back" ||
                n == "background_image" ||
                n.StartsWith("menubackground_");
            // Don't touch the parallax-scroll layers — they're gameplay art.
            if (!isBackground || n.Contains("parallax")) continue;

            // Stretch the image to cover its parent.
            RectTransform rt = img.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
            }
            img.preserveAspect = false;

            // Make sure the canvas behind it expands to cover the screen.
            Canvas canvas = img.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Canvas root = canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
                CanvasScaler scaler = root.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                }
            }
        }
    }

    static void FixCanvasScalers()
    {
        // Use actual screen dimensions as reference so UI stays within visible area
        float w = Screen.width;
        float h = Screen.height;

        CanvasScaler[] scalers = Object.FindObjectsOfType<CanvasScaler>();
        foreach (CanvasScaler scaler in scalers)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(w, h);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    static void FitBackgroundsToPortrait()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        // Whatever the actual aspect, find the camera's visible world rect.
        float camHeight = cam.orthographicSize * 2f;
        float camAspect = cam.aspect;
        float camWidth  = camHeight * camAspect;

        // Pass 1: try to scale every SpriteRenderer whose name looks like a background.
        SpriteRenderer[] all = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        bool scaledAny = false;
        foreach (SpriteRenderer sr in all)
        {
            if (sr == null || sr.sprite == null) continue;
            if (!LooksLikeBackgroundName(sr.gameObject.name)) continue;
            ScaleSpriteToFillCamera(sr, cam, camWidth, camHeight);
            scaledAny = true;
        }

        // Pass 2 (fallback): if nothing matched the name pattern, scale the
        // sprite with the LOWEST sortingOrder that is not a parallax layer —
        // that's almost certainly the background image of the scene.
        if (!scaledAny)
        {
            SpriteRenderer back = null;
            int lowestOrder = int.MaxValue;
            float biggestArea = 0f;
            foreach (SpriteRenderer sr in all)
            {
                if (sr == null || sr.sprite == null) continue;
                string n = sr.gameObject.name.ToLower();
                if (n.Contains("parallax") || n.Contains("cloud")) continue;
                // Prefer lowest sorting order, tie-break by largest area.
                int order = sr.sortingOrder;
                float area = sr.bounds.size.x * sr.bounds.size.y;
                if (order < lowestOrder || (order == lowestOrder && area > biggestArea))
                {
                    lowestOrder = order;
                    biggestArea = area;
                    back = sr;
                }
            }
            if (back != null)
            {
                Debug.Log("[ScreenSetup] Auto-detected background SpriteRenderer: " + back.gameObject.name +
                          " (sortingOrder=" + back.sortingOrder + ")");
                ScaleSpriteToFillCamera(back, cam, camWidth, camHeight);
            }
        }
    }

    static bool LooksLikeBackgroundName(string raw)
    {
        string n = raw.ToLower();
        if (n.Contains("parallax") || n.Contains("cloud")) return false;
        if (n.Contains("background") || n.Contains("backdrop")) return true;
        if (n.Contains("bck"))     return true;
        if (n.Contains("forest"))  return true;
        if (n.Contains("jungle"))  return true;
        if (n == "back" || n == "bg") return true;
        if (n.StartsWith("bg_") || n.EndsWith("_bg")) return true;
        if (n.StartsWith("back_") || n.EndsWith("_back")) return true;
        return false;
    }

    static void ScaleSpriteToFillCamera(SpriteRenderer sr, Camera cam, float camWidth, float camHeight)
    {
        // Get sprite world size at current scale
        float spriteWorldWidth = sr.sprite.bounds.size.x * sr.transform.lossyScale.x;
        float spriteWorldHeight = sr.sprite.bounds.size.y * sr.transform.lossyScale.y;

        // Calculate scale needed to cover the full camera view
        float scaleToFillWidth = camWidth / spriteWorldWidth;
        float scaleToFillHeight = camHeight / spriteWorldHeight;

        // Use the LARGER scale so the sprite covers everything (no black gaps)
        float fillScale = Mathf.Max(scaleToFillWidth, scaleToFillHeight);

        // Apply scale
        sr.transform.localScale = sr.transform.localScale * fillScale;

        // Center on camera position
        sr.transform.position = new Vector3(
            cam.transform.position.x,
            cam.transform.position.y,
            sr.transform.position.z
        );
    }
}
