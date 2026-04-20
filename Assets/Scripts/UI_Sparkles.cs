using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to any UI element (RectTransform) to emit animated sparkle particles around it.
/// Sparkles are Image objects spawned as children, drift outward, scale down, fade out.
/// Works on Screen Space - Overlay canvases. Uses unscaled time.
/// </summary>
public class UI_Sparkles : MonoBehaviour
{
    [Header("Spawn")]
    public float SpawnInterval = 0.15f;
    public Vector2 SpawnArea = new Vector2(400f, 120f);

    [Header("Visual")]
    public Color SparkleColor = new Color(1f, 0.95f, 0.5f, 1f);
    public float SparkleSize = 18f;
    public float Lifetime = 0.9f;
    public float DriftSpeed = 40f;

    private static Sprite sharedSprite;
    private float timer;

    void Start()
    {
        if (sharedSprite == null) sharedSprite = CreateStarSprite();
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer < SpawnInterval) return;
        timer = 0f;
        Spawn();
    }

    void Spawn()
    {
        GameObject obj = new GameObject("Sparkle");
        obj.transform.SetParent(transform, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(SparkleSize, SparkleSize);
        rt.anchoredPosition = new Vector2(
            Random.Range(-SpawnArea.x * 0.5f, SpawnArea.x * 0.5f),
            Random.Range(-SpawnArea.y * 0.5f, SpawnArea.y * 0.5f)
        );
        rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        Image img = obj.AddComponent<Image>();
        img.sprite = sharedSprite;
        img.color = SparkleColor;
        img.raycastTarget = false;

        SparkleInstance inst = obj.AddComponent<SparkleInstance>();
        inst.Init(Lifetime, DriftSpeed);
    }

    static Sprite CreateStarSprite()
    {
        int size = 11;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        float c = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - c);
                float dy = Mathf.Abs(y - c);
                // Four-point star shape
                bool on = (dx + dy) <= 3.5f || (dx <= 0.8f && dy <= c) || (dy <= 0.8f && dx <= c);
                pixels[y * size + x] = on ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

public class SparkleInstance : MonoBehaviour
{
    private float lifetime;
    private float timer;
    private Vector2 drift;
    private RectTransform rt;
    private Image img;
    private Color baseColor;
    private float baseScale;
    private float spin;

    public void Init(float life, float speed)
    {
        lifetime = life;
        float angle = Random.Range(0f, 360f);
        drift = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * speed;
        spin = Random.Range(-180f, 180f);
    }

    void Start()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        if (img != null) baseColor = img.color;
        baseScale = Random.Range(0.8f, 1.3f);
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        float t = timer / lifetime;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        if (rt != null)
        {
            rt.anchoredPosition += drift * Time.unscaledDeltaTime;
            float scale = baseScale * (1f - t);
            rt.localScale = new Vector3(scale, scale, 1f);
            rt.localRotation *= Quaternion.Euler(0f, 0f, spin * Time.unscaledDeltaTime);
        }

        if (img != null)
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t));
    }
}
