using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu_AutoStart : MonoBehaviour
{
    [Tooltip("TextMeshPro text that shows the countdown.")]
    public TMP_Text TimerText;

    [Tooltip("How many seconds before the game auto-starts.")]
    public float AutoStartSeconds = 60f;

    [Tooltip("Scene to load when the timer runs out.")]
    public string SceneToLoad = "In_Game";

    [Header("Urgency colors")]
    public Color NormalColor = new Color(1f, 1f, 1f, 1f);
    public Color WarnColor = new Color(1f, 0.75f, 0.2f, 1f);   // <= 10s
    public Color DangerColor = new Color(1f, 0.25f, 0.25f, 1f); // <= 5s

    [Header("Tick pulse")]
    public float TickScaleKick = 0.35f;   // how much bigger it pops each tick
    public float TickEaseSpeed = 8f;

    private float timer;
    private bool loading = false;
    private int lastShownSecond = -1;
    private float currentKick = 0f;
    private Vector3 baseScale = Vector3.one;

    void Start()
    {
        if (GetComponent<MainMenu_Music>() == null)
            gameObject.AddComponent<MainMenu_Music>();

        timer = AutoStartSeconds;

        Button playButton = FindPlayButton();

        if (TimerText != null)
        {
            RectTransform rt = TimerText.rectTransform;

            // Anchor timer to screen center and place it just below the Play button
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(700f, 80f);

            float belowY = -120f;
            if (playButton != null)
            {
                RectTransform prt = playButton.GetComponent<RectTransform>();
                if (prt != null)
                {
                    float buttonHeight = prt.rect.height * Mathf.Abs(prt.localScale.y);
                    belowY = prt.anchoredPosition.y - (buttonHeight * 0.5f) - 50f;
                }
            }
            rt.anchoredPosition = new Vector2(-120f, belowY);

            // Smaller font + center alignment
            TimerText.fontSize = 28f;
            TimerText.alignment = TextAlignmentOptions.Center;

            baseScale = rt.localScale;

            // Force visible defaults — ignore whatever may be serialized as black on existing components.
            if (IsUnusable(NormalColor)) NormalColor = new Color(1f, 1f, 1f, 1f);          // white
            if (IsUnusable(WarnColor))   WarnColor   = new Color(1f, 0.75f, 0.2f, 1f);     // amber
            if (IsUnusable(DangerColor)) DangerColor = new Color(1f, 0.25f, 0.25f, 1f);    // red

            TimerText.color = NormalColor;
        }

        UpdateText((int)Mathf.Ceil(timer));
    }

    Button FindPlayButton()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button b in buttons)
        {
            string n = b.gameObject.name.ToLower();
            if (n.Contains("play") && !n.Contains("again")) return b;
        }
        return null;
    }

    static bool IsUnusable(Color c)
    {
        // Treat "black with no alpha" (Unity's default-serialized Color) as unusable.
        return c.a < 0.01f || (c.r < 0.01f && c.g < 0.01f && c.b < 0.01f);
    }

    void Update()
    {
        if (loading) return;

        timer -= Time.unscaledDeltaTime;
        int secondsLeft = Mathf.Max(0, Mathf.CeilToInt(timer));

        if (secondsLeft != lastShownSecond)
        {
            lastShownSecond = secondsLeft;
            OnTick(secondsLeft);
        }

        EaseKick();

        if (timer <= 0f)
        {
            loading = true;
            MainMenu_Music.StopMusic();
            SceneManager.LoadScene(SceneToLoad);
        }
    }

    void OnTick(int secondsLeft)
    {
        UpdateText(secondsLeft);

        if (TimerText == null) return;

        // Urgency color
        if (secondsLeft <= 5) TimerText.color = DangerColor;
        else if (secondsLeft <= 10) TimerText.color = WarnColor;
        else TimerText.color = NormalColor;

        // Scale kick — bigger kick when time is low
        float urgency = secondsLeft <= 5 ? 1.6f : secondsLeft <= 10 ? 1.2f : 1f;
        currentKick = TickScaleKick * urgency;
    }

    void EaseKick()
    {
        if (TimerText == null) return;
        currentKick = Mathf.Lerp(currentKick, 0f, Time.unscaledDeltaTime * TickEaseSpeed);
        float s = 1f + currentKick;
        TimerText.rectTransform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, baseScale.z);
    }

    void UpdateText(int seconds)
    {
        if (TimerText == null) return;
        TimerText.text = "Auto starts in " + seconds + " seconds";
    }
}
