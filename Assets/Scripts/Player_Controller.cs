using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    public float Movement_Speed = 10f;
    public Sprite[] Spr_Player = new Sprite[2];

    private Rigidbody2D Rigid;
    private SpriteRenderer spriteRenderer;
    private Vector3 Player_LocalScale;
    private float Movement = 0;

    // Squash & stretch
    private float squashImpulse = 0f;   // set >0 by external triggers for a landing squash
    private float facingSign = 1f;

    void Awake()
    {
        Rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Player_LocalScale = transform.localScale;
        facingSign = Mathf.Sign(Player_LocalScale.x);
        if (facingSign == 0f) facingSign = 1f;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null) box.enabled = true;

        if (Rigid != null)
            Rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        if (!Game_Controller.Game_Started)
        {
            ApplyScale(1f, 1f);
            return;
        }

        Movement = Input.GetAxisRaw("Horizontal") * Movement_Speed;
        if (Movement > 0.01f) facingSign = 1f;
        else if (Movement < -0.01f) facingSign = -1f;

        ApplyScaleFromVelocity();
    }

    void FixedUpdate()
    {
        if (Rigid == null || !Game_Controller.Game_Started) return;

        Vector2 Velocity = Rigid.linearVelocity;
        Velocity.x = Movement;
        Rigid.linearVelocity = Velocity;

        if (Velocity.y < 0)
        {
            if (spriteRenderer != null && Spr_Player.Length > 0) spriteRenderer.sprite = Spr_Player[0];
            Game_Controller.ComboCount = 0;
            Game_Controller.ScoreMultiplier = 1f;
            Propeller_Fall();
        }
        else if (spriteRenderer != null && Spr_Player.Length > 1)
        {
            spriteRenderer.sprite = Spr_Player[1];
        }

        WrapAroundScreen();
    }

    void ApplyScaleFromVelocity()
    {
        float vy = Rigid != null ? Rigid.linearVelocity.y : 0f;
        // Velocity stretch: up fast = tall, down fast = slightly squat
        float stretch = Mathf.Clamp(vy / 18f, -0.25f, 0.35f);

        float scaleY = 1f + stretch;
        float scaleX = 1f - stretch * 0.4f;

        // Landing impulse pop: strong squat that decays
        if (squashImpulse > 0f)
        {
            scaleY *= 1f - squashImpulse * 0.35f;
            scaleX *= 1f + squashImpulse * 0.45f;
            squashImpulse = Mathf.Max(0f, squashImpulse - Time.deltaTime * 6f);
        }

        ApplyScale(scaleX, scaleY);
    }

    void ApplyScale(float sx, float sy)
    {
        transform.localScale = new Vector3(
            facingSign * Mathf.Abs(Player_LocalScale.x) * sx,
            Player_LocalScale.y * sy,
            Player_LocalScale.z
        );
    }

    // Called by Platform when the player lands (via SendMessage).
    void OnPlatformLand()
    {
        squashImpulse = 1f;
    }

    void WrapAroundScreen()
    {
        if (Camera.main == null) return;

        Vector3 Top_Left = Camera.main.ScreenToWorldPoint(Vector3.zero);
        float Offset = 0.5f;

        if (transform.position.x > -Top_Left.x + Offset)
            transform.position = new Vector3(Top_Left.x - Offset, transform.position.y, transform.position.z);
        else if (transform.position.x < Top_Left.x - Offset)
            transform.position = new Vector3(-Top_Left.x + Offset, transform.position.y, transform.position.z);
    }

    void Propeller_Fall()
    {
        if (transform.childCount == 0) return;

        Transform child = transform.GetChild(0);
        Propeller prop = child.GetComponent<Propeller>();
        if (prop == null) return;

        Animator anim = child.GetComponent<Animator>();
        if (anim != null) anim.SetBool("Active", false);

        prop.Set_Fall(gameObject);
        child.parent = null;
    }
}
