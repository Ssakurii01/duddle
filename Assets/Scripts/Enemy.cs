using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour {

    private float moveSpeed = 1.5f;
    private int direction = 1;
    private float leftBound;
    private float rightBound;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private int bonusScore = 200;

    void Start()
    {
        // Create a simple colored square sprite for the enemy
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateSquareSprite();
        spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f); // Red enemy
        spriteRenderer.sortingOrder = 5;

        transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        // Add rigidbody (kinematic, so trigger detection works)
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Add collider
        BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        // Set movement bounds based on screen
        Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        leftBound = topLeft.x + 0.5f;
        rightBound = -topLeft.x - 0.5f;

        // Random start direction
        direction = Random.Range(0, 2) == 0 ? -1 : 1;

        // Increase speed with height
        float heightFactor = Mathf.Clamp(transform.position.y / 150f, 0f, 1f);
        moveSpeed = Mathf.Lerp(1.5f, 3.5f, heightFactor);

        // Tag as Object so it gets cleaned up on game over
        gameObject.tag = "Object";
    }

    void Update()
    {
        if (isDead) return;

        // Move horizontally
        transform.position += new Vector3(moveSpeed * direction * Time.deltaTime, 0, 0);

        // Bounce off screen edges
        if (transform.position.x > rightBound)
            direction = -1;
        else if (transform.position.x < leftBound)
            direction = 1;

        // Flip sprite based on direction
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * direction,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    void FixedUpdate()
    {
        // Destroy when off screen (below camera)
        if (Camera.main != null)
        {
            float destroyY = Camera.main.transform.position.y - 6f;
            if (transform.position.y < destroyY)
                Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (other.gameObject.name != "Doodler") return;

        Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        bool stomp = other.transform.position.y > transform.position.y + 0.15f && playerRb.linearVelocity.y < 0;

        Vector2 vel = playerRb.linearVelocity;
        vel.y = stomp ? 12f : -5f;
        playerRb.linearVelocity = vel;

        if (!stomp) return;

        isDead = true;
        Score_Popup.Create(transform.position, "+" + bonusScore, new Color(1f, 0.3f, 0.3f));
        Game_Controller.AddCombo();
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        // Squash effect
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        while (timer < 0.3f)
        {
            timer += Time.deltaTime;
            float t = timer / 0.3f;
            transform.localScale = new Vector3(
                startScale.x * (1f + t * 0.5f),
                startScale.y * (1f - t),
                startScale.z
            );
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f - t);
            yield return null;
        }
        Destroy(gameObject);
    }

    // Create a simple square sprite at runtime (no asset needed)
    Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                // Simple monster face: body with eyes
                bool isBody = true;
                bool isEye = (x >= 8 && x <= 12 && y >= 18 && y <= 24) ||
                             (x >= 19 && x <= 23 && y >= 18 && y <= 24);
                bool isMouth = (y >= 8 && y <= 10 && x >= 10 && x <= 21);

                // Rounded corners
                float dx = x - 16f;
                float dy = y - 16f;
                if (dx * dx + dy * dy > 15f * 15f)
                    isBody = false;

                if (isEye)
                    pixels[y * 32 + x] = Color.white;
                else if (isMouth)
                    pixels[y * 32 + x] = new Color(0.3f, 0f, 0f);
                else if (isBody)
                    pixels[y * 32 + x] = Color.white;
                else
                    pixels[y * 32 + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
    }
}
