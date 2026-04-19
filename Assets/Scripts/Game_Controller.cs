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
    private float Max_Height = 0;
    private Vector3 Top_Left;
    private Vector3 Camera_Pos;
    private bool Game_Over = false;

    private Text Txt_Combo;
    private float comboDisplayTimer = 0f;

    void Awake()
    {
        Game_Started = false;
        ComboCount = 0;
        ScoreMultiplier = 1f;
        CoinScore = 0;

        Player = Instantiate(PlayerPrefab, SpawnPosition, Quaternion.identity);
        Player.name = "Doodler";

        HideGameOverMenu();

        Camera_Pos = Camera.main.transform.position;
        Top_Left = Camera.main.ScreenToWorldPoint(Vector3.zero);

        ConfigureScoreCanvas();
        PositionScoreText();
        Create_ComboText();

        Time.timeScale = 0f;
    }

    void Update()
    {
        bool confirmPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0);

        if (!Game_Started && confirmPressed)
        {
            Game_Started = true;
            Time.timeScale = 1f;
        }

        if (Game_Over && confirmPressed)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("In_Game");
        }

        Score = (int)(Max_Height * 50 * ScoreMultiplier) + CoinScore;
        Txt_Score.text = "Score : " + Score;

        UpdateComboText();
    }

    void FixedUpdate()
    {
        if (!Game_Started || Game_Over) return;

        if (Player.transform.position.y > Max_Height)
            Max_Height = Player.transform.position.y;

        if (Player.transform.position.y - Camera.main.transform.position.y < Get_DestroyDistance())
        {
            GetComponent<AudioSource>().Play();
            Set_GameOver();
            Game_Over = true;
        }
    }

    public bool Get_GameOver()
    {
        return Game_Over;
    }

    public float Get_DestroyDistance()
    {
        return Camera_Pos.y + Top_Left.y;
    }

    public static void AddCoinScore(int value)
    {
        CoinScore += value;
    }

    public static void AddCombo()
    {
        ComboCount++;
        ScoreMultiplier = ComboCount >= 3 ? 1f + (ComboCount - 2) * 0.5f : 1f;
    }

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

    void Set_GameOver()
    {
        if (Data_Manager.Get_HighScore() < Score)
            Data_Manager.Set_HighScore(Score);

        GameObject Background_Canvas = GameObject.Find("Background_Canvas");
        if (Background_Canvas == null) return;

        Button_OnClick.Set_GameOverMenu(true);
        RepositionGameOverButtons(Background_Canvas);

        Animator anim = Background_Canvas.GetComponent<Animator>();
        if (anim != null) anim.enabled = true;

        File_Manager.Save_Info();
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
