using UnityEngine;

public class Propeller : MonoBehaviour
{
    private bool Attach = false;
    private bool Fall = false;
    private float Destroy_Distance;

    private GameObject Game_Controller;

    void Start()
    {
        Game_Controller = GameObject.Find("Game_Controller");
        if (Game_Controller == null) return;

        Game_Controller gc = Game_Controller.GetComponent<Game_Controller>();
        if (gc != null) Destroy_Distance = gc.Get_DestroyDistance();
    }

    void FixedUpdate()
    {
        if (!Fall) return;

        GetComponent<AudioSource>().Stop();
        transform.Rotate(new Vector3(0, 0, -3.5f));
        transform.position -= new Vector3(0, 0.3f, 0);

        if (transform.position.y - Camera.main.transform.position.y < Destroy_Distance)
            Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D Other)
    {
        if (Other.gameObject.tag != "Player" || Attach) return;
        if (Other.transform.childCount != 0) return;

        // Parent propeller to player
        transform.parent = Other.transform;
        transform.localPosition = new Vector3(0, -0.02f, 0);
        GetComponent<BoxCollider2D>().enabled = false;

        Rigidbody2D Rigid = Other.collider.GetComponent<Rigidbody2D>();
        if (Rigid != null)
        {
            Vector2 Force = Rigid.linearVelocity;
            Force.y = 80f;
            Rigid.linearVelocity = Force;

            GetComponent<AudioSource>().Play();

            Animator anim = GetComponent<Animator>();
            if (anim != null) anim.SetBool("Active", true);

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 12;

            AttachTrail(Other.gameObject);
        }

        Attach = true;
    }

    public void Set_Fall(GameObject Player)
    {
        Fall = true;

        BoxCollider2D box = Player.GetComponent<BoxCollider2D>();
        if (box != null) box.enabled = true;

        RemoveTrail(Player);
    }

    static void AttachTrail(GameObject player)
    {
        TrailRenderer trail = player.GetComponent<TrailRenderer>();
        if (trail == null) trail = player.AddComponent<TrailRenderer>();

        trail.enabled = true;
        trail.time = 0.45f;
        trail.startWidth = 0.55f;
        trail.endWidth = 0.02f;
        trail.minVertexDistance = 0.05f;
        trail.sortingOrder = 3;  // behind the player (player is 4+)
        trail.numCapVertices = 4;

        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 1f, 1f), 0f),
                new GradientColorKey(new Color(0.7f, 0.9f, 1f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.85f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = g;

        if (trail.material == null || trail.material.shader == null)
        {
            Shader sh = Shader.Find("Sprites/Default");
            if (sh == null) sh = Shader.Find("Unlit/Transparent");
            if (sh != null) trail.material = new Material(sh);
        }

        trail.Clear();
    }

    static void RemoveTrail(GameObject player)
    {
        TrailRenderer trail = player.GetComponent<TrailRenderer>();
        if (trail != null) trail.enabled = false;
    }
}
