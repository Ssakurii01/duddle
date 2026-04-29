using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_Controller : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public Vector3 SpawnPosition = new Vector3(0f, -3f, 0f);

    public Text Txt_Score;
    public Text Txt_GameOverScore;
    public Text Txt_GameOverHighsocre;

    public static bool Game_Started = false;
    public static int ComboCount = 0;
    public static float ScoreMultiplier = 1f;
    private static int CoinScore = 0;

    private GameObject Player;
    private int Score;
    private float displayScore = 0f;
    private float Max_Height = 0;
    private Vector3 Top_Left;
    private Vector3 Camera_Pos;
    private bool Game_Over = false;
    private bool Countdown_Started = false;

    private Text Txt_Combo;
    private float comboDisplayTimer = 0f;

    // Score text pulse (for milestones + high-combo reactions)
    private Vector3 scoreBaseScale = Vector3.one;
    private float scoreKick = 0f;

    // Milestones already celebrated (so each fires once per run)
    private readonly int[] milestones = { 1000, 2500, 5000, 10000, 20000, 50000 };
    private int milestoneIndex = 0;

    void Awake()
    {
        Game_Started = false;
        Countdown_Started = false;
        ComboCount = 0;
        ScoreMultiplier = 1f;
        CoinScore = 0;

        if (GetComponent<InGame_Music>() == null)
            gameObject.AddComponent<InGame_Music>();

        Player = Instantiate(PlayerPrefab, SpawnPosition, Quaternion.identity);
        Player.name = "Doodler";

        HideGameOverMenu();

        Camera_Pos = Camera.main.transform.position;
        Top_Left = Camera.main.ScreenToWorldPoint(Vector3.zero);

        ConfigureScoreCanvas();
        PositionScoreText();
        Create_ComboText();

        scoreBaseScale = Txt_Score.rectTransform.localScale;

        Time.timeScale = 0f;

        // Tell the Luxodd platform a new level run has begun (analytics).
        if (Luxodd_Bridge.Instance != null)
            Luxodd_Bridge.Instance.SendLevelBegin();
    }

    void Update()
    {
        bool confirmPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0);

        // Start: trigger countdown instead of starting immediately
        if (!Game_Started && !Countdown_Started && confirmPressed)
        {
            Countdown_Started = true;
            Start_Countdown.Play(this, () =>
            {
                Game_Started = true;
                Time.timeScale = 1f;
            });
        }

        if (Game_Over && confirmPressed)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("In_Game");
        }

        Score = (int)(Max_Height * 50 * ScoreMultiplier) + CoinScore;

        // Milestone flash — only when game is active
        if (Game_Started && !Game_Over) CheckMilestones();

        displayScore = Mathf.Lerp(displayScore, Score, Time.unscaledDeltaTime * 8f);
        if (Mathf.Abs(Score - displayScore) <= 1f) displayScore = Score;
        Txt_Score.text = "Score : " + Mathf.RoundToInt(displayScore);

        // Ease the score scale back to 1 after kicks
        scoreKick = Mathf.Lerp(scoreKick, 0f, Time.unscaledDeltaTime * 8f);
        Txt_Score.rectTransform.localScale = scoreBaseScale * (1f + scoreKick);

        UpdateComboText();
    }

    void FixedUpdate()
    {
        if (!Game_Started || Game_Over) return;

        // While respawning, skip the death check (player is frozen at safe spot).
        if (Lives_System.IsRespawning) return;

        if (Player.transform.position.y > Max_Height)
            Max_Height = Player.transform.position.y;

        if (Player.transform.position.y - Camera.main.transform.position.y < Get_DestroyDistance())
        {
            // If hearts remain, lose one and respawn instead of game over.
            if (Lives_System.Instance != null &&
                Lives_System.Instance.LoseLifeAndRespawn(Player))
            {
                GetComponent<AudioSource>().Play();
                return;
            }

            // No hearts left → real game over (leaderboard appears via the UI).
            GetComponent<AudioSource>().Play();
            Camera_Shake.Shake(0.6f, 0.5f);
            Set_GameOver();
            Game_Over = true;
        }
    }

    public bool Get_GameOver() => Game_Over;
    public float Get_DestroyDistance() => Camera_Pos.y + Top_Left.y;

    public static void AddCoinScore(int value) { CoinScore += value; }

    public static void AddCombo()
    {
        ComboCount++;
        ScoreMultiplier = ComboCount >= 3 ? 1f + (ComboCount - 2) * 0.5f : 1f;
    }

    // ----- Milestones -----

    void CheckMilestones()
    {
        while (milestoneIndex < milestones.Length && Score >= milestones[milestoneIndex])
        {
            TriggerMilestone(milestones[milestoneIndex]);
            milestoneIndex++;
        }
    }

    void TriggerMilestone(int value)
    {
        scoreKick = 0.6f;
        Camera_Shake.Shake(0.25f, 0.2f);

        // Gold banner at player position
        if (Player != null)
        {
            Score_Popup.Create(
                Player.transform.position + new Vector3(0, 1.5f, 0),
                value.ToString("N0") + "!",
                new Color(1f, 0.85f, 0.2f)
            );
            Jump_Particles.Burst(Player.transform.position, new Color(1f, 0.85f, 0.2f, 0.9f), 14);
        }

        // Gold flash on score text
        StartCoroutine(FlashScoreGold());
    }

    IEnumerator FlashScoreGold()
    {
        Color original = Txt_Score.color;
        Color gold = new Color(1f, 0.85f, 0.2f);

        float t = 0f;
        while (t < 0.45f)
        {
            t += Time.unscaledDeltaTime;
            Txt_Score.color = Color.Lerp(gold, original, t / 0.45f);
            yield return null;
        }
        Txt_Score.color = original;
    }

    // ----- UI setup -----

    void ConfigureScoreCanvas()
    {
        Canvas scoreCanvas = Txt_Score.GetComponentInParent<Canvas>();
        if (scoreCanvas == null) return;

        scoreCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        scoreCanvas.sortingOrder = 10;
        scoreCanvas.gameObject.SetActive(true);

        CanvasScaler scaler = scoreCanvas.GetComponent<CanvasScaler>();
        if (scaler == null) return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    void PositionScoreText()
    {
        RectTransform scoreRect = Txt_Score.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 1);
        scoreRect.anchorMax = new Vector2(0, 1);
        scoreRect.pivot = new Vector2(0, 1);
        scoreRect.sizeDelta = new Vector2(500, 100);
        scoreRect.anchoredPosition = new Vector2(20, -20);

        Txt_Score.alignment = TextAnchor.UpperLeft;
        Txt_Score.fontSize = 26;
        Txt_Score.color = Color.white;
        Txt_Score.horizontalOverflow = HorizontalWrapMode.Overflow;
        Txt_Score.verticalOverflow = VerticalWrapMode.Overflow;
        Txt_Score.raycastTarget = false;
        Txt_Score.gameObject.SetActive(true);
    }

    void Create_ComboText()
    {
        GameObject comboObj = new GameObject("Combo_Text");
        comboObj.transform.SetParent(Txt_Score.transform.parent, false);

        Txt_Combo = comboObj.AddComponent<Text>();
        Txt_Combo.font = Txt_Score.font;
        Txt_Combo.fontSize = 28;
        Txt_Combo.color = new Color(1f, 0.9f, 0.2f);
        Txt_Combo.alignment = TextAnchor.UpperLeft;
        Txt_Combo.horizontalOverflow = HorizontalWrapMode.Overflow;
        Txt_Combo.verticalOverflow = VerticalWrapMode.Overflow;
        Txt_Combo.raycastTarget = false;
        Txt_Combo.text = "";

        RectTransform comboRect = Txt_Combo.GetComponent<RectTransform>();
        comboRect.anchorMin = new Vector2(0, 1);
        comboRect.anchorMax = new Vector2(0, 1);
        comboRect.pivot = new Vector2(0, 1);
        comboRect.sizeDelta = new Vector2(600, 70);
        comboRect.anchoredPosition = new Vector2(20, -100);
    }

    void UpdateComboText()
    {
        if (Txt_Combo == null) return;

        if (ComboCount >= 3)
        {
            Txt_Combo.text = "COMBO x" + ComboCount + "! (x" + ScoreMultiplier.ToString("0.0") + ")";
            comboDisplayTimer = 1.5f;
            return;
        }

        if (comboDisplayTimer > 0)
        {
            comboDisplayTimer -= Time.deltaTime;
            if (comboDisplayTimer <= 0) Txt_Combo.text = "";
        }
    }

    void HideGameOverMenu()
    {
        GameObject bgCanvas = GameObject.Find("Background_Canvas");
        if (bgCanvas == null) return;

        for (int i = 1; i < bgCanvas.transform.childCount; i++)
            bgCanvas.transform.GetChild(i).gameObject.SetActive(false);
    }

    // ----- Game Over -----

    void Set_GameOver()
    {
        InGame_Music.StopMusic();

        bool newHighScore = Data_Manager.Get_HighScore() < Score;
        if (newHighScore) Data_Manager.Set_HighScore(Score);

        // Save the score into the local leaderboard (top 10).
        string playerName = (Luxodd_Bridge.Instance != null && !string.IsNullOrEmpty(Luxodd_Bridge.Instance.PlayerName))
            ? Luxodd_Bridge.Instance.PlayerName
            : "Player";
        Leaderboard_Manager.Add_Score(playerName, Score);
        Leaderboard_Manager.Save();

        // Luxodd: send level_end to the server and queue the Restart popup
        // ~3 seconds after the leaderboard appears (configurable on the bridge).
        if (Luxodd_Bridge.Instance != null)
            Luxodd_Bridge.Instance.TriggerRestartPopupAfterGameOver(Score);

        GameObject bgCanvas = GameObject.Find("Background_Canvas");
        if (bgCanvas == null) return;

        Button_OnClick.Set_GameOverMenu(true);
        RepositionGameOverButtons(bgCanvas);

        Animator anim = bgCanvas.GetComponent<Animator>();
        if (anim != null) anim.enabled = true;

        File_Manager.Save_Info();

        StartCoroutine(GameOverSequence(bgCanvas, newHighScore, Score));
    }

    IEnumerator GameOverSequence(GameObject bgCanvas, bool newHighScore, int finalScore)
    {
        // 1. Collect the active buttons and remember their target scales
        var buttons = new System.Collections.Generic.List<(RectTransform rt, Vector3 target)>();
        for (int i = 1; i < bgCanvas.transform.childCount; i++)
        {
            Transform child = bgCanvas.transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;
            if (child.GetComponent<Button>() == null) continue;

            RectTransform rt = child.GetComponent<RectTransform>();
            buttons.Add((rt, rt.localScale));
            rt.localScale = Vector3.zero;
        }

        // 2. Brief dramatic pause
        yield return new WaitForSecondsRealtime(0.35f);

        // 3. Optionally celebrate a new high score first
        if (newHighScore) yield return StartCoroutine(ShowHighScoreCelebration());

        // 4. Score count-up on Txt_GameOverScore (if wired)
        if (Txt_GameOverScore != null && Txt_GameOverScore.gameObject.activeInHierarchy)
        {
            float t = 0f;
            float duration = 0.9f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                int v = Mathf.RoundToInt(Mathf.Lerp(0, finalScore, t / duration));
                Txt_GameOverScore.text = v.ToString();
                yield return null;
            }
            Txt_GameOverScore.text = finalScore.ToString();
        }

        if (Txt_GameOverHighsocre != null)
            Txt_GameOverHighsocre.text = Data_Manager.Get_HighScore().ToString();

        // 5. Pop buttons in with a small bounce
        float b = 0f;
        float bDur = 0.45f;
        while (b < bDur)
        {
            b += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(b / bDur);
            // easeOutBack-ish
            float scale = EaseOutBack(p);
            foreach (var pair in buttons)
                pair.rt.localScale = Vector3.Lerp(Vector3.zero, pair.target, scale);
            yield return null;
        }
        foreach (var pair in buttons) pair.rt.localScale = pair.target;

        // 6. Show the jungle leaderboard panel after the game over animation
        yield return new WaitForSecondsRealtime(0.4f);
        Leaderboard_UI.ShowAutomatic();
    }

    IEnumerator ShowHighScoreCelebration()
    {
        // Banner text created on the Background_Canvas
        GameObject bannerObj = new GameObject("HS_Banner");
        Canvas bg = GameObject.Find("Background_Canvas")?.GetComponent<Canvas>();
        if (bg != null) bannerObj.transform.SetParent(bg.transform, false);

        Text banner = bannerObj.AddComponent<Text>();
        banner.font = Txt_Score.font;
        banner.text = "NEW HIGH SCORE!";
        banner.fontSize = 44;
        banner.color = new Color(1f, 0.85f, 0.1f);
        banner.alignment = TextAnchor.MiddleCenter;
        banner.raycastTarget = false;
        banner.horizontalOverflow = HorizontalWrapMode.Overflow;
        banner.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform brt = banner.rectTransform;
        brt.anchorMin = new Vector2(0.5f, 0.5f);
        brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.sizeDelta = new Vector2(900, 120);
        brt.anchoredPosition = new Vector2(0, 180);
        brt.localScale = Vector3.zero;

        // Confetti at the player's last known position — multiple color bursts
        Camera_Shake.Shake(0.3f, 0.25f);
        SpawnConfetti(Player != null ? Player.transform.position : Vector3.zero);

        float t = 0f;
        float pop = 0.5f;
        while (t < pop)
        {
            t += Time.unscaledDeltaTime;
            brt.localScale = Vector3.one * EaseOutBack(t / pop);
            yield return null;
        }
        brt.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(0.8f);
    }

    void SpawnConfetti(Vector3 center)
    {
        Color[] colors = new Color[]
        {
            new Color(1f, 0.3f, 0.3f, 0.95f),
            new Color(0.3f, 1f, 0.4f, 0.95f),
            new Color(0.3f, 0.5f, 1f, 0.95f),
            new Color(1f, 0.9f, 0.2f, 0.95f),
            new Color(1f, 0.4f, 0.9f, 0.95f),
        };
        for (int i = 0; i < colors.Length; i++)
            Jump_Particles.Burst(center, colors[i], 10);
    }

    static float EaseOutBack(float p)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float p1 = p - 1f;
        return 1f + c3 * p1 * p1 * p1 + c1 * p1 * p1;
    }

    void RepositionGameOverButtons(GameObject bgCanvas)
    {
        int btnIndex = 0;
        for (int i = 1; i < bgCanvas.transform.childCount; i++)
        {
            Transform child = bgCanvas.transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;
            if (child.GetComponent<Button>() == null) continue;

            RectTransform rect = child.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(350, 90);
            rect.anchoredPosition = new Vector2(0, 50 - (btnIndex * 110));
            btnIndex++;
        }
    }
}
