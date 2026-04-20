using UnityEngine;

[DefaultExecutionOrder(100)]
public class Camera_Shake : MonoBehaviour
{
    public static Camera_Shake Instance;

    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;
    private Vector3 lastOffset = Vector3.zero;

    void Awake()
    {
        Instance = this;
    }

    public static void Shake(float duration, float magnitude)
    {
        if (Instance == null)
        {
            if (Camera.main == null) return;
            Instance = Camera.main.GetComponent<Camera_Shake>();
            if (Instance == null) Instance = Camera.main.gameObject.AddComponent<Camera_Shake>();
        }

        if (duration > Instance.shakeTimer) Instance.shakeTimer = duration;
        if (magnitude > Instance.shakeMagnitude) Instance.shakeMagnitude = magnitude;
    }

    void LateUpdate()
    {
        // Undo previous frame's offset so the base position stays clean
        transform.position -= lastOffset;
        lastOffset = Vector3.zero;

        if (shakeTimer <= 0f)
        {
            shakeMagnitude = 0f;
            return;
        }

        shakeTimer -= Time.unscaledDeltaTime;

        if (shakeTimer <= 0f)
        {
            shakeMagnitude = 0f;
            return;
        }

        // Decay magnitude as shake runs out for a smoother fade
        float decay = Mathf.Clamp01(shakeTimer / 0.5f);
        float m = shakeMagnitude * decay;

        lastOffset = new Vector3(
            Random.Range(-1f, 1f) * m,
            Random.Range(-1f, 1f) * m,
            0f
        );
        transform.position += lastOffset;
    }
}
