using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    public float Movement_Speed = 10f;
    public Sprite[] Spr_Player = new Sprite[2];

    private Rigidbody2D Rigid;
    private SpriteRenderer spriteRenderer;
    private Vector3 Player_LocalScale;
    private float Movement = 0;

    void Awake()
    {
        Rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Player_LocalScale = transform.localScale;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null) box.enabled = true;

        if (Rigid != null)
            Rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        if (!Game_Controller.Game_Started) return;

        Movement = Input.GetAxisRaw("Horizontal") * Movement_Speed;

        if (Movement != 0)
        {
            float sign = Mathf.Sign(Movement);
            transform.localScale = new Vector3(sign * Player_LocalScale.x, Player_LocalScale.y, Player_LocalScale.z);
        }
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
