using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Auto-attaches to the In_Game and Main_Menu scenes and spawns procedural clouds
/// that drift across the screen. Clouds tint themselves based on the altitude they
/// spawn at, so they feel at home in day / sunset / night skies driven by Sky_Gradient.
/// </summary>
public class Cloud_Spawner : MonoBehaviour
{
    private const int MaxClouds = 10;

    private static Sprite cloudSprite;

    private float timer = 0f;
    private Camera cam;
    private int active = 0;

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
        if (GameObject.Find("Cloud_Spawner") != null) return;

        GameObject host = new GameObject("Cloud_Spawner");
        host.AddComponent<Cloud_Spawner>();
    }

    void Start()
    {
        cam = Camera.main;
        if (cloudSprite == null) cloudSprite = CreateCloudSprite();

        // Pre-seed a few clouds so the sky isn't empty on first frame
        for (int i = 0; i < 4; i++) SpawnCloud(randomX: true);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < 2.0f) return;
        timer = 0f;

        if (active < MaxClouds) SpawnCloud(randomX: false);
    }

    void SpawnCloud(bool randomX)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        GameObject obj = new GameObject("Cloud");
        obj.tag = "Object";

        float halfW = cam.orthographicSize * cam.aspect;
        float halfH = cam.orthographicSize;

        float xStart;
        float speedDir = Random.Range(0, 2) == 0 ? 1f : -1f;

        if (randomX)
            xStart = cam.transform.position.x + Random.Range(-halfW, halfW);
        else
            xStart = cam.transform.position.x + (speedDir > 0 ? -halfW - 2f : halfW + 2f);

        float yOffset = Random.Range(-halfH * 0.6f, halfH * 0.9f);
        obj.transform.position = new Vector3(xStart, cam.transform.position.y + yOffset, 1f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = cloudSprite;
        sr.sortingOrder = 1;

        Color tint = Sky_Gradient.GetCloudColor(obj.transform.position.y);
        tint.a = Random.Range(0.55f, 0.9f);
        sr.color = tint;

        float scale = Random.Range(1.2f, 3.4f);
        obj.transform.localScale = new Vector3(scale, scale * Random.Range(0.55f, 0.8f), 1f);

        Cloud c = obj.AddComponent<Cloud>();
        c.Init(this, Random.Range(0.3f, 1.1f) * speedDir);
        active++;
    }

    public void OnCloudDestroyed() { active--; }

    static Sprite CreateCloudSprite()
    {
        int w = 40, h = 20;
        Texture2D tex = new Texture2D(w, h);
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        // Stack of overlapping circular blobs
        float[] cx = { 10f, 18f, 26f, 32f };
        float[] cy = { 10f, 12f, 10f, 9f };
        float[] r  = { 7f,  9f,  8f,  6f };

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float alpha = 0f;
                for (int i = 0; i < cx.Length; i++)
                {
                    float dx = x - cx[i];
                    float dy = y - cy[i];
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d < r[i])
                    {
                        float a = 1f - (d / r[i]);
                        if (a > alpha) alpha = a;
                    }
                }
                if (alpha > 0f)
                    pixels[y * w + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha * 1.2f));
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), w);
    }
}

public class Cloud : MonoBehaviour
{
    private float speed;
    private Cloud_Spawner spawner;
    private Camera cam;

    public void Init(Cloud_Spawner s, float horizontalSpeed)
    {
        spawner = s;
        speed = horizontalSpeed;
    }

    void Start() { cam = Camera.main; }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        transform.position += new Vector3(speed * Time.deltaTime, 0f, 0f);

        if (cam == null) return;
        float halfW = cam.orthographicSize * cam.aspect + 3f;
        float dx = transform.position.x - cam.transform.position.x;
        float dy = transform.position.y - cam.transform.position.y;

        if (Mathf.Abs(dx) > halfW || dy < -cam.orthographicSize - 3f)
        {
            if (spawner != null) spawner.OnCloudDestroyed();
            Destroy(gameObject);
        }
    }
}
