using UnityEngine;

public class Jump_Particles : MonoBehaviour
{
    private static Sprite sharedSprite;

    private float lifetime = 0.5f;
    private float timer = 0f;
    private Vector2 velocity;
    private SpriteRenderer sr;
    private Color baseColor;

    public static void Burst(Vector3 position, int count = 8)
    {
        Burst(position, new Color(1f, 1f, 1f, 0.9f), count);
    }

    public static void Burst(Vector3 position, Color color, int count = 8)
    {
        if (sharedSprite == null) sharedSprite = CreateDustSprite();

        for (int i = 0; i < count; i++)
        {
            GameObject p = new GameObject("JumpParticle");
            p.transform.position = position;
            p.tag = "Object";

            float angle = (180f / count) * i + Random.Range(-10f, 10f);
            float speed = Random.Range(1.5f, 3.5f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad)));

            Jump_Particles jp = p.AddComponent<Jump_Particles>();
            jp.velocity = dir * speed;
            jp.baseColor = color;
        }
    }

    void Start()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sharedSprite;
        sr.color = baseColor;
        sr.sortingOrder = 6;
        transform.localScale = Vector3.one * Random.Range(0.18f, 0.32f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        float t = timer / lifetime;

        transform.position += (Vector3)velocity * Time.deltaTime;
        velocity *= 0.94f;                          // Drag
        velocity += Vector2.down * 6f * Time.deltaTime; // Gravity

        if (sr != null)
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t));
    }

    static Sprite CreateDustSprite()
    {
        int size = 8;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        float r = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                pixels[y * size + x] = (dx * dx + dy * dy) < r * r ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
