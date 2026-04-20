using UnityEngine;

/// <summary>
/// Attach to any sprite that should parallax with the main camera.
///
/// ParallaxFactor semantics:
///   1.0 = moves exactly with camera  → looks glued to the view (sky background)
///   0.8 = moves almost as fast as camera → appears distant (far mountains)
///   0.5 = mid distance                → trees / midground
///   0.2 = barely moves                → close foreground (grass)
///   0.0 = static world position       → foreground foliage that passes quickly
///
/// Turn on Repeat_Vertical and the sprite will wrap itself so you never see the edge.
/// </summary>
public class Parallax_Layer : MonoBehaviour
{
    [Range(0f, 1f)] public float ParallaxFactor = 0.5f;

    [Tooltip("Wrap the sprite vertically so it never leaves view (requires a SpriteRenderer).")]
    public bool Repeat_Vertical = false;

    private float startY;
    private float startCameraY;
    private float spriteHeight;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        startY = transform.position.y;
        if (cam != null) startCameraY = cam.transform.position.y;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            spriteHeight = sr.bounds.size.y;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
            startCameraY = cam.transform.position.y;
        }

        float dy = cam.transform.position.y - startCameraY;
        float newY = startY + dy * ParallaxFactor;

        if (Repeat_Vertical && spriteHeight > 0f)
        {
            float camY = cam.transform.position.y;
            while (newY < camY - spriteHeight * 0.5f) newY += spriteHeight;
            while (newY > camY + spriteHeight * 1.5f) newY -= spriteHeight;
        }

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
