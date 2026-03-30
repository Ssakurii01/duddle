using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour {

    Rigidbody2D Rigid;
    public float Movement_Speed = 10f;
    private float Movement = 0;
    private int Direction = 1;
    private Vector3 Player_LocalScale;
    private SpriteRenderer spriteRenderer;

    public Sprite[] Spr_Player = new Sprite[2];

    void Awake()
    {
        Rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

	// Use this for initialization
	void Start ()
    {
        Player_LocalScale = transform.localScale;
        GetComponent<BoxCollider2D>().enabled = true;
        if (Rigid != null)
            Rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!Game_Controller.Game_Started) return;

        // Set Movement value
        Movement = Input.GetAxisRaw("Horizontal") * Movement_Speed;

        
        // Player look right or left
        if (Movement > 0)
            transform.localScale = new Vector3(Player_LocalScale.x, Player_LocalScale.y, Player_LocalScale.z);
        else if (Movement < 0)
            transform.localScale = new Vector3(-Player_LocalScale.x, Player_LocalScale.y, Player_LocalScale.z);
	}

    void FixedUpdate()
    {
        if (Rigid == null || !Game_Controller.Game_Started) return;

        // Calculate player velocity
        Vector2 Velocity = Rigid.linearVelocity;
        Velocity.x = Movement;
        Rigid.linearVelocity = Velocity;

        // Player change sprite
        if (Velocity.y < 0)
        {
            spriteRenderer.sprite = Spr_Player[0];

            // Reset combo when falling
            Game_Controller.ComboCount = 0;
            Game_Controller.ScoreMultiplier = 1f;

            // Fall propeller after fly up
            Propeller_Fall();
        }
        else
        {
            spriteRenderer.sprite = Spr_Player[1];
        }

        // Player wrap
        if (Camera.main != null)
        {
            Vector3 Top_Left = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
            float Offset = 0.5f;

            if (transform.position.x > -Top_Left.x + Offset)
                transform.position = new Vector3(Top_Left.x - Offset, transform.position.y, transform.position.z);
            else if (transform.position.x < Top_Left.x - Offset)
                transform.position = new Vector3(-Top_Left.x + Offset, transform.position.y, transform.position.z);
        }
    }



    void Propeller_Fall()
    {
        if (transform.childCount > 0)
        {
            transform.GetChild(0).GetComponent<Animator>().SetBool("Active", false);
            transform.GetChild(0).GetComponent<Propeller>().Set_Fall(gameObject);
            transform.GetChild(0).parent = null;
        }
    }
}
