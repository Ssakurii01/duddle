using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {

    public float Jump_Force = 10f;
    private float Destroy_Distance;
    private bool Create_NewPlatform = false;

    private GameObject Game_Controller_Obj;
    private AudioSource audioSource;
    private EdgeCollider2D edgeCollider;
    private PlatformEffector2D platformEffector;
    private SpriteRenderer spriteRenderer;

    // Super jump - random golden platform
    private bool isSuperJump = false;

    // Use this for initialization
    void Start()
    {
        Game_Controller_Obj = GameObject.Find("Game_Controller");
        audioSource = GetComponent<AudioSource>();
        edgeCollider = GetComponent<EdgeCollider2D>();
        platformEffector = GetComponent<PlatformEffector2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set distance to destroy the platforms out of screen
        Destroy_Distance = Game_Controller_Obj.GetComponent<Game_Controller>().Get_DestroyDistance();

        // 1 in 7 chance to become a super jump golden platform (only green platforms)
        if (!GetComponent<Platform_White>() && !GetComponent<Platform_Brown>() && !GetComponent<Platform_Blue>())
        {
            if (Random.Range(0, 7) == 0)
            {
                isSuperJump = true;
                spriteRenderer.color = new Color(1f, 0.75f, 0f); // Bright golden color
            }
        }
    }

    void FixedUpdate()
    {
        // Platform out of screen
        if (transform.position.y - Camera.main.transform.position.y < Destroy_Distance)
        {
            // Create new platform
            if (name != "Platform_Brown(Clone)" && name != "Spring(Clone)" && name != "Trampoline(Clone)" && !Create_NewPlatform)
            {
                Game_Controller_Obj.GetComponent<Platform_Generator>().Generate_Platform(1);
                Create_NewPlatform = true;
            }
            
            // Deactive Collider and effector
            edgeCollider.enabled = false;
            platformEffector.enabled = false;
            spriteRenderer.enabled = false;

            // Deactive collider and effector if gameobject has child
            if (transform.childCount > 0)
            {
                if(transform.GetChild(0).GetComponent<Platform>()) // if child is platform
                {
                    transform.GetChild(0).GetComponent<EdgeCollider2D>().enabled = false;
                    transform.GetChild(0).GetComponent<PlatformEffector2D>().enabled = false;
                    transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                }

                // Destroy this platform if sound has finished
                if ((audioSource == null || !audioSource.isPlaying) && !transform.GetChild(0).GetComponent<AudioSource>().isPlaying)
                    Destroy(gameObject);
            }
            else
            {
                // Destroy this platform if sound has finished
                if (audioSource == null || !audioSource.isPlaying)
                    Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D Other)
    {
        Rigidbody2D Rigid = Other.collider.GetComponent<Rigidbody2D>();

        // Player must be landing on top for it to count
        if (Rigid != null && Other.transform.position.y > transform.position.y)
        {
            Vector2 Force = Rigid.linearVelocity;
            float jumpForce = isSuperJump ? Jump_Force * 3f : Jump_Force;
            Force.y = jumpForce;
            Rigid.linearVelocity = Force;

            // Force 2D sound and play jump sound
            if (audioSource != null && audioSource.clip != null) {
                audioSource.spatialBlend = 0f;
                audioSource.Stop();
                audioSource.Play();
            }

            // Flash golden platform white on super jump
            if (isSuperJump)
            {
                StartCoroutine(SuperJumpFlash());
                Score_Popup.Create(transform.position, "SUPER!", new Color(1f, 0.8f, 0f));
            }
            else
            {
                // Show floating score for normal jumps
                int points = (int)(50 * Game_Controller.ScoreMultiplier);
                Score_Popup.Create(transform.position, "+" + points, Color.white);
            }

            // if gameobject has animation; Like spring, trampoline and etc...
            if (GetComponent<Animator>())
                GetComponent<Animator>().SetBool("Active", true);

            // Add combo hit
            Game_Controller.AddCombo();

            // Check platform type
            Platform_Type();
        }
    }

    void Platform_Type()
    {
        if (GetComponent<Platform_White>())
            GetComponent<Platform_White>().Deactive();
        else if (GetComponent<Platform_Brown>())
            GetComponent<Platform_Brown>().Deactive();
    }

    IEnumerator SuperJumpFlash()
    {
        Color original = new Color(1f, 0.75f, 0f);
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }
}
