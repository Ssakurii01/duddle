using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to any UI Button for arcade-style feedback:
///  - scale up when hovered or selected (keyboard/gamepad nav)
///  - bounce down when pressed
///  - plays optional hover / click sounds
///
/// Works with both mouse pointers and the EventSystem selection (arcade joystick).
/// </summary>
public class UI_ButtonFeedback : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Scale")]
    public float HoverScale = 1.1f;
    public float PressScale = 0.9f;
    public float EaseSpeed = 15f;

    [Header("Sound")]
    public AudioClip HoverSound;
    public AudioClip ClickSound;
    [Range(0f, 1f)] public float Volume = 0.7f;

    private Vector3 baseScale;
    private Vector3 targetScale;
    private bool hovered = false;
    private bool pressed = false;

    private static AudioSource sharedAudio;

    void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    void OnEnable()
    {
        hovered = false;
        pressed = false;
        targetScale = baseScale;
        transform.localScale = baseScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * EaseSpeed);
    }

    public void OnPointerEnter(PointerEventData e) { Hover(true); PlaySound(HoverSound); }
    public void OnPointerExit(PointerEventData e) { Hover(false); }
    public void OnPointerDown(PointerEventData e) { Press(true); PlaySound(ClickSound); }
    public void OnPointerUp(PointerEventData e) { Press(false); }
    public void OnSelect(BaseEventData e) { Hover(true); PlaySound(HoverSound); }
    public void OnDeselect(BaseEventData e) { Hover(false); }

    void Hover(bool state) { hovered = state; UpdateTarget(); }
    void Press(bool state) { pressed = state; UpdateTarget(); }

    void UpdateTarget()
    {
        if (pressed) targetScale = baseScale * PressScale;
        else if (hovered) targetScale = baseScale * HoverScale;
        else targetScale = baseScale;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (sharedAudio == null)
        {
            GameObject obj = new GameObject("UI_AudioSource");
            Object.DontDestroyOnLoad(obj);
            sharedAudio = obj.AddComponent<AudioSource>();
            sharedAudio.playOnAwake = false;
            sharedAudio.spatialBlend = 0f;
        }
        sharedAudio.PlayOneShot(clip, Volume);
    }
}
