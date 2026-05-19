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

        // Calculate what the camera can see in portrait
        float camHeight = cam.orthographicSize * 2f;
        float camAspect = cam.aspect; // Will be 9/16 = 0.5625 in portrait
        float camWidth = camHeight * camAspect;

        // Find all SpriteRenderers and scale backgrounds to fill
        SpriteRenderer[] allSprites = Object.FindObjectsOfType<SpriteRenderer>();
        foreach (SpriteRenderer sr in allSprites)
        {
            if (sr.sprite == null) continue;

            string objName = sr.gameObject.name.ToLower();
            // Match background objects by common names
            if (objName.Contains("background") || objName.Contains("bck") ||
                objName.Contains("bg") || objName == "back")
            {
                ScaleSpriteToFillCamera(sr, cam, camWidth, camHeight);
            }
        }
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
