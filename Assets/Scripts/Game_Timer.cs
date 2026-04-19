using UnityEngine;
using TMPro;

public class Game_Timer : MonoBehaviour
{
    [Tooltip("TextMeshPro text that shows the elapsed time.")]
    public TMP_Text TimerText;

    private float elapsed = 0f;
    private Game_Controller gameController;

    void Start()
    {
        GameObject gc = GameObject.Find("Game_Controller");
        if (gc != null) gameController = gc.GetComponent<Game_Controller>();
        UpdateText();
    }

    void Update()
    {
        if (!Game_Controller.Game_Started) return;
        if (gameController != null && gameController.Get_GameOver()) return;

        elapsed += Time.deltaTime;
        UpdateText();
    }

    void UpdateText()
    {
        if (TimerText == null) return;

        int totalSeconds = Mathf.FloorToInt(elapsed);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        TimerText.text = minutes + ":" + seconds.ToString("00");
    }
}
