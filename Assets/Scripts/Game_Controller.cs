using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Game_Controller : MonoBehaviour {

    public GameObject PlayerPrefab;
    public Vector3 SpawnPosition = new Vector3(0f, -3f, 0f);

    private GameObject Player;

    private float Max_Height = 0;
    public Text Txt_Score;

    private int Score;

    private Vector3 Top_Left;
    private Vector3 Camera_Pos;

    private bool Game_Over = false;
    public static bool Game_Started = false;

    private GameObject PressToStartObj;

    public Text Txt_GameOverScore;
    public Text Txt_GameOverHighsocre;

    // High score display during gameplay
    private Text Txt_HighScore;

    // Coin score bonus
    private static int CoinScore = 0;
    private Text Txt_Coins;

    // Combo multiplier system
    public static int ComboCount = 0;
    public static float ScoreMultiplier = 1f;
    private Text Txt_Combo;
    private float comboDisplayTimer = 0f;

	void Awake ()
    {
        Game_Started = false;

        // Spawn the player from prefab
        Player = Instantiate(PlayerPrefab, SpawnPosition, Quaternion.identity);
        Player.name = "Doodler";

        // Hide submit score and game over menus on start (prevent text overlap)
        HideSubmitScoreMenu();

        // Initialize boundary
        Camera_Pos = Camera.main.transform.position;
        Top_Left = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));

        // Make sure the score Canvas is visible and in Overlay mode
        Canvas scoreCanvas = Txt_Score.GetComponentInParent<Canvas>();
        if (scoreCanvas != null)
        {
            scoreCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            scoreCanvas.sortingOrder = 10;
            scoreCanvas.gameObject.SetActive(true);

            CanvasScaler scaler = scoreCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        // Position score text on the top-left of the screen
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

        // Create high score text on the top-right
        Create_HighScoreText();

        // Create coin counter under high score
        Create_CoinText();

        // Create combo text under the score
        Create_ComboText();

        // Reset combo and coins
        ComboCount = 0;
        ScoreMultiplier = 1f;
        CoinScore = 0;

        // Create "Press Space to Start" text above the doodler
        Create_PressToStartText();

        // Freeze the game until Space is pressed
        Time.timeScale = 0f;
	}

    void HideSubmitScoreMenu()
    {
        GameObject bgCanvas = GameObject.Find("Background_Canvas");
        if (bgCanvas == null) return;

        // Hide game over menu elements (children 1-11)
        for (int i = 1; i < 12 && i < bgCanvas.transform.childCount; i++)
            bgCanvas.transform.GetChild(i).gameObject.SetActive(false);

        // Hide submit score menu elements (children 12-16)
        for (int i = 12; i < 17 && i < bgCanvas.transform.childCount; i++)
            bgCanvas.transform.GetChild(i).gameObject.SetActive(false);

        // Hide error message (child 17)
        if (bgCanvas.transform.childCount > 17)
            bgCanvas.transform.GetChild(17).gameObject.SetActive(false);
    }

    void Create_HighScoreText()
    {
        GameObject highScoreObj = new GameObject("HighScore_Text");
        highScoreObj.transform.SetParent(Txt_Score.transform.parent, false);
        Txt_HighScore = highScoreObj.AddComponent<Text>();
        Txt_HighScore.font = Txt_Score.font;
        Txt_HighScore.fontSize = 14;
        Txt_HighScore.color = new Color(1f, 0.85f, 0.4f); // Gold color
        Txt_HighScore.alignment = TextAnchor.UpperRight;
        Txt_HighScore.horizontalOverflow = HorizontalWrapMode.Overflow;
        Txt_HighScore.verticalOverflow = VerticalWrapMode.Overflow;
        Txt_HighScore.raycastTarget = false;
        Txt_HighScore.text = "Best: " + Data_Manager.Get_HighScore();

        RectTransform highScoreRect = Txt_HighScore.GetComponent<RectTransform>();
        highScoreRect.anchorMin = new Vector2(1, 1);
        highScoreRect.anchorMax = new Vector2(1, 1);
        highScoreRect.pivot = new Vector2(1, 1);
        highScoreRect.sizeDelta = new Vector2(500, 80);
        highScoreRect.anchoredPosition = new Vector2(-20, -20);
    }

    void Create_CoinText()
    {
        GameObject coinObj = new GameObject("Coin_Text");
        coinObj.transform.SetParent(Txt_Score.transform.parent, false);
        Txt_Coins = coinObj.AddComponent<Text>();
        Txt_Coins.font = Txt_Score.font;
        Txt_Coins.fontSize = 14;
        Txt_Coins.color = new Color(1f, 0.85f, 0f); // Gold
        Txt_Coins.alignment = TextAnchor.UpperRight;
        Txt_Coins.horizontalOverflow = HorizontalWrapMode.Overflow;
        Txt_Coins.verticalOverflow = VerticalWrapMode.Overflow;
        Txt_Coins.raycastTarget = false;
        Txt_Coins.text = "Coins: 0";

        RectTransform coinRect = Txt_Coins.GetComponent<RectTransform>();
        coinRect.anchorMin = new Vector2(1, 1);
        coinRect.anchorMax = new Vector2(1, 1);
        coinRect.pivot = new Vector2(1, 1);
        coinRect.sizeDelta = new Vector2(500, 60);
        coinRect.anchoredPosition = new Vector2(-20, -36);
    }

    public static void AddCoinScore(int value)
    {
        CoinScore += value;
    }

    void Create_ComboText()
    {
        // Create a UI Text for combo display under the score
        GameObject comboObj = new GameObject("Combo_Text");
        comboObj.transform.SetParent(Txt_Score.transform.parent, false);
        Txt_Combo = comboObj.AddComponent<Text>();
        Txt_Combo.font = Txt_Score.font;
        Txt_Combo.fontSize = 28;
        Txt_Combo.color = new Color(1f, 0.9f, 0.2f); // Yellow-gold
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

    public static void AddCombo()
    {
        ComboCount++;
        if (ComboCount >= 3)
            ScoreMultiplier = 1f + (ComboCount - 2) * 0.5f; // x1.5, x2.0, x2.5...
        else
            ScoreMultiplier = 1f;
    }

    void Create_PressToStartText()
    {
        // Create a world-space text right above the doodler's starting platform
        PressToStartObj = new GameObject("PressToStart_Text");
        TextMesh textMesh = PressToStartObj.AddComponent<TextMesh>();
        textMesh.text = "Press to Start";
        textMesh.fontSize = 30;
        textMesh.characterSize = 0.05f;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;

        // Position below the doodler, on the green platform
        Vector3 playerPos = Player.transform.position;
        PressToStartObj.transform.position = new Vector3(playerPos.x, playerPos.y - 1.3f, 0);

        // Make sure it renders in front
        PressToStartObj.GetComponent<MeshRenderer>().sortingOrder = 20;
    }

    void Update()
    {
        // Wait for Space or arcade Black button (JoystickButton0) to start
        bool confirmPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0);

        if (!Game_Started && confirmPressed)
        {
            Game_Started = true;
            Time.timeScale = 1f;

            // Remove the "Press Space to Start" text
            if (PressToStartObj != null)
                Destroy(PressToStartObj);
        }

        // Restart game with Space or Black button when game over
        if (Game_Over && confirmPressed)
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("In_Game");
        }
    }

	void FixedUpdate ()
    {
        if(!Game_Started || Game_Over) return;

        if(!Game_Over)
        {
            // Calculate max height
            if (Player.transform.position.y > Max_Height)
            {
                Max_Height = Player.transform.position.y;
            }

            // Check player fall
            if (Player.transform.position.y - Camera.main.transform.position.y < Get_DestroyDistance())
            {
                // Play game over sound
                GetComponent<AudioSource>().Play();

                // Set game over
                Set_GameOver();
                Game_Over = true;
            }
        }
	}

    void OnGUI()
    {
        // Set score with combo multiplier + coin bonus
        Score = (int)(Max_Height * 50 * ScoreMultiplier) + CoinScore;
        Txt_Score.text = Score.ToString();

        // Update coin counter
        if (Txt_Coins != null)
            Txt_Coins.text = "Coins: " + CoinScore;

        // Update high score display (show current best in real time)
        if (Txt_HighScore != null)
        {
            int displayHighScore = Mathf.Max(Data_Manager.Get_HighScore(), Score);
            Txt_HighScore.text = "Best: " + displayHighScore;
        }

        // Show combo text when combo >= 3
        if (Txt_Combo != null)
        {
            if (ComboCount >= 3)
            {
                Txt_Combo.text = "COMBO x" + ComboCount + "! (x" + ScoreMultiplier.ToString("0.0") + ")";
                comboDisplayTimer = 1.5f;
            }
            else if (comboDisplayTimer > 0)
            {
                comboDisplayTimer -= Time.deltaTime;
                if (comboDisplayTimer <= 0)
                    Txt_Combo.text = "";
            }
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

    void Set_GameOver()
    {
        if (Data_Manager.Get_HighScore() < Score)
            Data_Manager.Set_HighScore(Score);

        GameObject Background_Canvas = GameObject.Find("Background_Canvas");

        // Hide ALL submit score / text elements (children 12+)
        for (int i = 12; i < Background_Canvas.transform.childCount; i++)
            Background_Canvas.transform.GetChild(i).gameObject.SetActive(false);

        // Show only play again and menu buttons
        Button_OnClick.Set_GameOverMenu(true);

        // Move Play Again and Menu buttons to center of screen
        RepositionGameOverButtons(Background_Canvas);

        // Enable animation
        Background_Canvas.GetComponent<Animator>().enabled = true;

        // Save file
        File_Manager.Save_Info();
    }

    void RepositionGameOverButtons(GameObject bgCanvas)
    {
        int btnIndex = 0;
        for (int i = 1; i < 12 && i < bgCanvas.transform.childCount; i++)
        {
            Transform child = bgCanvas.transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            Button btn = child.GetComponent<Button>();
            if (btn == null) continue;

            RectTransform rect = child.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(350, 90);
            // Stack buttons vertically in center: first at +50, second at -60
            rect.anchoredPosition = new Vector2(0, 50 - (btnIndex * 110));
            btnIndex++;
        }
    }
}
