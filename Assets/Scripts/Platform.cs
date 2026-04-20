using UnityEngine;

public class Platform : MonoBehaviour
{
    public float Jump_Force = 10f;

    private float Destroy_Distance;
    private AudioSource audioSource;
    private EdgeCollider2D edgeCollider;
    private PlatformEffector2D platformEffector;
    private SpriteRenderer spriteRenderer;
    private bool isSuperJump = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        edgeCollider = GetComponent<EdgeCollider2D>();
        platformEffector = GetComponent<PlatformEffector2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject gc = GameObject.Find("Game_Controller");
        if (gc != null)
        {
            Game_Controller ctrl = gc.GetComponent<Game_Controller>();
            if (ctrl != null) Destroy_Distance = ctrl.Get_DestroyDistance();
        }

        // Only plain green platforms can roll into a golden Super Jump
        if (GetComponent<Platform_White>() != null) return;
        if (GetComponent<Platform_Brown>() != null) return;
        if (GetComponent<Platform_Blue>() != null) return;

        if (Random.Range(0, 7) == 0)
        {
            isSuperJump = true;
            if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.75f, 0f);
        }
    }

    void FixedUpdate()
    {
        if (Camera.main == null) return;
        if (transform.position.y - Camera.main.transform.position.y >= Destroy_Distance) return;

        if (edgeCollider != null) edgeCollider.enabled = false;
        if (platformEffector != null) platformEffector.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        if (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            if (child.GetComponent<Platform>() != null)
            {
                EdgeCollider2D childEdge = child.GetComponent<EdgeCollider2D>();
                PlatformEffector2D childEffector = child.GetComponent<PlatformEffector2D>();
                SpriteRenderer childSprite = child.GetComponent<SpriteRenderer>();
                if (childEdge != null) childEdge.enabled = false;
                if (childEffector != null) childEffector.enabled = false;
                if (childSprite != null) childSprite.enabled = false;
            }

            AudioSource childAudio = child.GetComponent<AudioSource>();
            bool myDone = audioSource == null || !audioSource.isPlaying;
            bool childDone = childAudio == null || !childAudio.isPlaying;
            if (myDone && childDone) Destroy(gameObject);
            return;
        }

        if (audioSource == null || !audioSource.isPlaying)
            Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D Other)
    {
        Rigidbody2D Rigid = Other.collider.GetComponent<Rigidbody2D>();
        if (Rigid == null || Other.transform.position.y <= transform.position.y) return;

        Vector2 Force = Rigid.linearVelocity;
        Force.y = isSuperJump ? Jump_Force * 3f : Jump_Force;
        Rigid.linearVelocity = Force;

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.spatialBlend = 0f;
            audioSource.Stop();
            audioSource.Play();
        }

        // Juice: dust burst at landing point
        Vector3 burstPos = new Vector3(Other.transform.position.x, transform.position.y + 0.15f, 0);
        Jump_Particles.Burst(burstPos, isSuperJump ? 12 : 6);

        // Juice: landing squash on player
        Other.gameObject.SendMessage("OnPlatformLand", SendMessageOptions.DontRequireReceiver);

        // Juice: screen shake — tiny on normal jump, meaty on super
        Camera_Shake.Shake(isSuperJump ? 0.3f : 0.06f, isSuperJump ? 0.35f : 0.05f);

        if (isSuperJump)
            Score_Popup.Create(transform.position, "SUPER!", new Color(1f, 0.8f, 0f));

        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("Active", true);

        Game_Controller.AddCombo();

        Platform_White white = GetComponent<Platform_White>();
        if (white != null) { white.Deactive(); return; }

        Platform_Brown brown = GetComponent<Platform_Brown>();
        if (brown != null) brown.Deactive();
    }
}
