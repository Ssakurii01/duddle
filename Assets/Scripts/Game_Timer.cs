using UnityEngine;
using TMPro;

public class Game_Timer : MonoBehaviour
{
    [Tooltip("TextMeshPro text that shows the elapsed time.")]
    public TMP_Text TimerText;

    [Tooltip("Text color (leave default to force white).")]
    public Color TextColor = new Color(1f, 1f, 1f, 1f);

    [Header("Per-second pulse")]
    public float TickScaleKick = 0.2f;
    public float TickEaseSpeed = 10f;

    private float elapsed = 0f;
    private int lastShownSecond = -1;
    private float currentKick = 0f;
    private Vector3 baseScale = Vector3.one;

    private Game_Controller gameController;

    void Start()
    {
        GameObject gc = GameObject.Find("Game_Controller");
        if (gc != null) gameController = gc.GetComponent<Game_Controller>();

        if (TimerText != null)
        {
            baseScale = TimerText.rectTransform.localScale;

            // Force visible color — Unity's serialized default is invisible black.
            if (TextColor.a < 0.01f || (TextColor.r < 0.01f && TextColor.g < 0.01f && TextColor.b < 0.01f))
                TextColor = new Color(1f, 1f, 1f, 1f);

            TimerText.color = TextColor;
        }

        UpdateText();
    }

    void Update()
    {
        bool active = Game_Controller.Game_Started && (gameController == null || !gameController.Get_GameOver());

        if (active) elapsed += Time.deltaTime;

        int totalSeconds = Mathf.FloorToInt(elapsed);
        if (totalSeconds != lastShownSecond)
        {
            lastShownSecond = totalSeconds;
            UpdateText();
            currentKick = TickScaleKick;
        }

        EaseKick();
    }

    void UpdateText()
    {
        if (TimerText == null) return;

        int totalSeconds = Mathf.FloorToInt(elapsed);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        TimerText.text = minutes + ":" + seconds.ToString("00");
    }

    void EaseKick()
    {
        if (TimerText == null) return;
        currentKick = Mathf.Lerp(currentKick, 0f, Time.deltaTime * TickEaseSpeed);
        float s = 1f + currentKick;
        TimerText.rectTransform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, baseScale.z);
    }
}
