using UnityEngine;

public class Coin : MonoBehaviour {

    private SpriteRenderer spriteRenderer;
    private bool collected = false;
    private int coinValue = 100;
    private float bobTimer = 0f;
    private float startY;
    private static AudioClip coinSound;

    void Start()
    {
        // Load coin sprite from assets
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        Sprite coinSprite = Resources.Load<Sprite>("CoinIcon");
        // Try loading as sub-sprite if single mode fails
        if (coinSprite == null)
        {
            Sprite[] all = Resources.LoadAll<Sprite>("CoinIcon");
            if (all != null && all.Length > 0)
                coinSprite = all[0];
        }
        spriteRenderer.sprite = coinSprite != null ? coinSprite : CreateCoinSprite();
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 4;

        if (coinSound == null)
            coinSound = Resources.Load<AudioClip>("CoinSound");

        transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        // Add rigidbody (kinematic for trigger detection)
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Add trigger collider
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        startY = transform.position.y;

        // Tag as Object for cleanup on game over
        gameObject.tag = "Object";
    }

    void Update()
    {
        if (collected) return;

        // Gentle bobbing animation
        bobTimer += Time.deltaTime * 3f;
        transform.position = new Vector3(
            transform.position.x,
            startY + Mathf.Sin(bobTimer) * 0.08f,
            transform.position.z
        );
    }

    void FixedUpdate()
    {
        // Destroy when off screen below camera
        if (Camera.main != null)
        {
            float destroyY = Camera.main.transform.position.y - 6f;
            if (transform.position.y < destroyY)
                Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (other.gameObject.name != "Doodler") return;

        collected = true;

        if (coinSound != null)
            AudioSource.PlayClipAtPoint(coinSound, transform.position);

        // Add bonus score
        Game_Controller.AddCoinScore(coinValue);

        // Floating score popup
        Score_Popup.Create(transform.position, "+" + coinValue, new Color(1f, 0.85f, 0f));

        // Collect animation: scale up and fade out
        StartCoroutine(CollectAnimation());
    }

    System.Collections.IEnumerator CollectAnimation()
    {
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        Color startColor = spriteRenderer.color;

        while (timer < 0.3f)
        {
            timer += Time.deltaTime;
            float t = timer / 0.3f;

            // Scale up
            transform.localScale = startScale * (1f + t * 0.8f);

            // Fade out
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            // Float up
            transform.position += new Vector3(0, 2f * Time.deltaTime, 0);

            yield return null;
        }

        Destroy(gameObject);
    }

    Sprite CreateCoinSprite()
    {
        Texture2D tex = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dx = x - 16f;
                float dy = y - 16f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= 14f && dist >= 10f)
                {
                    // Ring of the coin
                    pixels[y * 32 + x] = Color.white;
                }
                else if (dist < 10f)
                {
                    // Inner coin face with a star/dollar pattern
                    bool isStar = (Mathf.Abs(dx) < 2f) || (Mathf.Abs(dy) < 2f);
                    pixels[y * 32 + x] = isStar ? new Color(1f, 1f, 0.7f) : Color.white;
                }
                else
                {
                    pixels[y * 32 + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
    }
}
