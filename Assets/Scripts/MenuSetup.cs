using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuSetup : MonoBehaviour
{
    [Header("Sprites (assign in Inspector)")]
    public Sprite BackgroundSprite;
    public Sprite LogoSprite;
    public Sprite ButtonSprite;

    [Header("Colors")]
    public Color ButtonColor = new Color(0.55f, 0.27f, 0.07f);
    public Color ButtonTextColor = Color.white;
    public Color TitleColor = Color.white;
    public Color MenuButtonColor = new Color(0.35f, 0.6f, 0.2f);

    [Header("Animation")]
    public float AnimDuration = 0.8f;
    public float StaggerDelay = 0.15f;

    private Canvas mainCanvas;
    private RectTransform logoRect;
    private RectTransform startBtnRect;
    private List<RectTransform> menuButtonRects = new List<RectTransform>();

    void Start()
    {
        HideOriginalUI();
        BuildMenu();
        StartCoroutine(PlayIntroAnimation());
    }

    void HideOriginalUI()
    {
        GameObject mainCanvasObj = GameObject.Find("Main_Canvas");
        if (mainCanvasObj != null)
            mainCanvasObj.SetActive(false);
    }

    void BuildMenu()
    {
        GameObject canvasObj = new GameObject("MenuCanvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        CreateBackground(canvasObj.transform);
        CreateOverlay(canvasObj.transform);
        CreateLogo(canvasObj.transform);
        CreateStartButton(canvasObj.transform);
        CreateMenuButtons(canvasObj.transform);
    }

    void CreateBackground(Transform parent)
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(parent, false);

        Image bgImage = bgObj.AddComponent<Image>();
        if (BackgroundSprite != null)
        {
            bgImage.sprite = BackgroundSprite;
            bgImage.type = Image.Type.Simple;
            bgImage.preserveAspect = false;
        }
        else
        {
            bgImage.color = new Color(0.2f, 0.5f, 0.3f);
        }

        RectTransform rect = bgObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void CreateOverlay(Transform parent)
    {
        GameObject overlayObj = new GameObject("DarkOverlay");
        overlayObj.transform.SetParent(parent, false);

        Image overlay = overlayObj.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.3f);
        overlay.raycastTarget = false;

        RectTransform rect = overlayObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void CreateLogo(Transform parent)
    {
        GameObject logoObj = new GameObject("Logo");
        logoObj.transform.SetParent(parent, false);

        if (LogoSprite != null)
        {
            Image logoImage = logoObj.AddComponent<Image>();
            logoImage.sprite = LogoSprite;
            logoImage.preserveAspect = true;
            logoImage.raycastTarget = false;
        }
        else
        {
            Text titleText = logoObj.AddComponent<Text>();
            titleText.text = "DOODLE\nJUMP";
            titleText.fontSize = 100;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = TitleColor;
            titleText.fontStyle = FontStyle.Bold;
            titleText.raycastTarget = false;

            Outline outline = logoObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            outline.effectDistance = new Vector2(4, -4);
        }

        logoRect = logoObj.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 1f);
        logoRect.anchorMax = new Vector2(0.5f, 1f);
        logoRect.pivot = new Vector2(0.5f, 1f);
        logoRect.sizeDelta = new Vector2(700, 350);
        logoRect.anchoredPosition = new Vector2(0, 500);
    }

    void CreateStartButton(Transform parent)
    {
        GameObject btnObj = new GameObject("StartButton");
        btnObj.transform.SetParent(parent, false);

        Image btnImage = btnObj.AddComponent<Image>();
        if (ButtonSprite != null)
        {
            btnImage.sprite = ButtonSprite;
            btnImage.type = Image.Type.Sliced;
        }
        else
        {
            btnImage.color = ButtonColor;
        }

        startBtnRect = btnObj.GetComponent<RectTransform>();
        startBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
        startBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
        startBtnRect.pivot = new Vector2(0.5f, 0.5f);
        startBtnRect.anchoredPosition = new Vector2(0, 50);
        startBtnRect.sizeDelta = new Vector2(450, 130);
        startBtnRect.localScale = Vector3.zero;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(OnStartClicked);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        Text btnText = textObj.AddComponent<Text>();
        btnText.text = "START";
        btnText.fontSize = 60;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = ButtonTextColor;
        btnText.fontStyle = FontStyle.Bold;
        btnText.raycastTarget = false;

        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.6f);
        shadow.effectDistance = new Vector2(3, -3);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    void CreateMenuButtons(Transform parent)
    {
        string[] labels = { "LEADERBOARD", "HIGH SCORE", "ABOUT" };
        float startY = -100f;
        float spacing = 100f;

        for (int i = 0; i < labels.Length; i++)
        {
            GameObject btnObj = new GameObject("MenuBtn_" + labels[i]);
            btnObj.transform.SetParent(parent, false);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = MenuButtonColor;

            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 80);
            rect.anchoredPosition = new Vector2(0, 1200);

            menuButtonRects.Add(rect);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            btn.colors = colors;

            string label = labels[i];
            btn.onClick.AddListener(() => OnMenuButtonClicked(label));

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            Text btnText = textObj.AddComponent<Text>();
            btnText.text = labels[i];
            btnText.fontSize = 36;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
            btnText.fontStyle = FontStyle.Bold;
            btnText.raycastTarget = false;

            Shadow shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }

    IEnumerator PlayIntroAnimation()
    {
        yield return StartCoroutine(SwipeToPosition(logoRect, new Vector2(0, -120), AnimDuration));
        yield return StartCoroutine(ScaleBounce(startBtnRect, AnimDuration * 0.6f));

        float startY = -100f;
        float spacing = 100f;
        for (int i = 0; i < menuButtonRects.Count; i++)
        {
            float targetY = startY - (spacing * i);
            StartCoroutine(SwipeToPosition(menuButtonRects[i], new Vector2(0, targetY), AnimDuration));
            yield return new WaitForSeconds(StaggerDelay);
        }

        StartCoroutine(PulseButton(startBtnRect));
    }

    IEnumerator SwipeToPosition(RectTransform rect, Vector2 targetPos, float duration)
    {
        Vector2 startPos = rect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            float overshoot = 1f + 0.1f * Mathf.Sin(t * Mathf.PI);
            rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, ease * overshoot);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
    }

    IEnumerator ScaleBounce(RectTransform rect, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = 1f - Mathf.Pow(1f - t, 3f) * Mathf.Cos(t * Mathf.PI * 2f) * (1f - t);
            rect.localScale = Vector3.one * Mathf.Max(0, scale);
            yield return null;
        }

        rect.localScale = Vector3.one;
    }

    IEnumerator PulseButton(RectTransform btnRect)
    {
        float speed = 1.5f;

        while (true)
        {
            float scale = 1f + Mathf.Sin(Time.time * speed) * 0.03f;
            btnRect.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    void OnStartClicked()
    {
        StartCoroutine(PlayOutroAndLoadGame());
    }

    IEnumerator PlayOutroAndLoadGame()
    {
        StartCoroutine(SwipeToPosition(logoRect, new Vector2(0, 500), AnimDuration * 0.5f));

        for (int i = menuButtonRects.Count - 1; i >= 0; i--)
        {
            StartCoroutine(SwipeToPosition(menuButtonRects[i], new Vector2(0, -1200), AnimDuration * 0.5f));
            yield return new WaitForSeconds(StaggerDelay * 0.5f);
        }

        float elapsed = 0f;
        float dur = 0.3f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dur;
            startBtnRect.localScale = Vector3.one * (1f - t);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("In_Game");
    }

    void OnMenuButtonClicked(string label)
    {
        switch (label)
        {
            case "LEADERBOARD":
                Debug.Log("Leaderboard clicked");
                break;
            case "HIGH SCORE":
                Debug.Log("High Score clicked");
                break;
            case "ABOUT":
                Debug.Log("About clicked");
                break;
        }
    }
}
