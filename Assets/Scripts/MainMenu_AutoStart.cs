using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu_AutoStart : MonoBehaviour
{
    [Tooltip("TextMeshPro text that shows the countdown.")]
    public TMP_Text TimerText;

    [Tooltip("How many seconds before the game auto-starts.")]
    public float AutoStartSeconds = 60f;

    [Tooltip("Scene to load when the timer runs out.")]
    public string SceneToLoad = "In_Game";

    private float timer;
    private bool loading = false;

    void Start()
    {
        timer = AutoStartSeconds;
        UpdateText(timer);
    }

    void Update()
    {
        if (loading) return;

        timer -= Time.unscaledDeltaTime;
        UpdateText(timer);

        if (timer <= 0f)
        {
            loading = true;
            SceneManager.LoadScene(SceneToLoad);
        }
    }

    void UpdateText(float seconds)
    {
        if (TimerText == null) return;
        int s = Mathf.Max(0, Mathf.CeilToInt(seconds));
        TimerText.text = "Auto starts in " + s + " seconds";
    }
}
