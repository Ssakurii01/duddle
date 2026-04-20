using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the main camera's background color based on the camera's current altitude.
/// Defines a series of stops — day → sunset → twilight → night → deep space — and lerps
/// between them smoothly. Auto-attaches on In_Game and Main_Menu scenes.
/// </summary>
[DefaultExecutionOrder(120)] // after Camera_Follow (which may still set color)
public class Sky_Gradient : MonoBehaviour
{
    public static Sky_Gradient Instance { get; private set; }

    [System.Serializable]
    public struct SkyStop
    {
        public float Altitude;
        public Color Color;
    }

    public SkyStop[] Stops = new SkyStop[]
    {
        new SkyStop { Altitude =   0f, Color = new Color(0.60f, 0.85f, 1.00f) },  // bright day blue
        new SkyStop { Altitude =  60f, Color = new Color(1.00f, 0.70f, 0.50f) },  // sunset peach
        new SkyStop { Altitude = 140f, Color = new Color(0.55f, 0.35f, 0.70f) },  // twilight purple
        new SkyStop { Altitude = 240f, Color = new Color(0.10f, 0.10f, 0.30f) },  // night navy
        new SkyStop { Altitude = 400f, Color = new Color(0.02f, 0.02f, 0.08f) },  // deep space
    };

    private Camera cam;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded += (s, m) => TryAttach();
        TryAttach();
    }

    static void TryAttach()
    {
        string n = SceneManager.GetActiveScene().name;
        if (n != "In_Game" && n != "Main_Menu") return;
        if (GameObject.Find("Sky_Gradient") != null) return;

        GameObject host = new GameObject("Sky_Gradient");
        host.AddComponent<Sky_Gradient>();
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cam = Camera.main;

        // Ensure the camera clears to a solid color so our tint shows
        if (cam != null && cam.clearFlags == CameraClearFlags.Skybox)
            cam.clearFlags = CameraClearFlags.SolidColor;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        cam.backgroundColor = GetColorAt(cam.transform.position.y);
    }

    public Color GetColorAt(float altitude)
    {
        if (Stops == null || Stops.Length == 0) return Color.black;
        if (altitude <= Stops[0].Altitude) return Stops[0].Color;
        if (altitude >= Stops[Stops.Length - 1].Altitude) return Stops[Stops.Length - 1].Color;

        for (int i = 0; i < Stops.Length - 1; i++)
        {
            if (altitude >= Stops[i].Altitude && altitude <= Stops[i + 1].Altitude)
            {
                float range = Stops[i + 1].Altitude - Stops[i].Altitude;
                float t = range > 0f ? (altitude - Stops[i].Altitude) / range : 0f;
                return Color.Lerp(Stops[i].Color, Stops[i + 1].Color, t);
            }
        }
        return Stops[0].Color;
    }

    /// <summary>Convenience for Cloud_Spawner: a white cloud tinted for the current sky.</summary>
    public static Color GetCloudColor(float altitude)
    {
        Color sky = Instance != null ? Instance.GetColorAt(altitude) : new Color(0.6f, 0.85f, 1f);

        // Clouds: stay lighter than the sky underneath them
        float lightness = Mathf.Max(sky.r, sky.g, sky.b);
        if (lightness < 0.35f)
        {
            // Dark sky → silvery clouds
            return new Color(0.75f, 0.78f, 0.85f, 0.7f);
        }
        // Bright sky → lift toward white
        return Color.Lerp(sky, Color.white, 0.7f);
    }
}
