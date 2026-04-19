using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Button_OnClick : MonoBehaviour
{
    public void Play_OnClick()
    {
        SceneManager.LoadScene("In_Game");
    }

    public void PlayAgain_OnClick()
    {
        SceneManager.LoadScene("In_Game");
    }

    public void Menu_OnClick()
    {
        SceneManager.LoadScene("Main_Menu");
    }

    public static void Set_GameOverMenu(bool State)
    {
        GameObject Background_Canvas = GameObject.Find("Background_Canvas");
        if (Background_Canvas == null) return;

        int count = Background_Canvas.transform.childCount;

        for (int i = 1; i < count; i++)
        {
            Transform child = Background_Canvas.transform.GetChild(i);
            Button btn = child.GetComponent<Button>();

            if (btn == null)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            string btnName = child.name.ToLower();
            bool isPlayAgain = btnName.Contains("play") || btnName.Contains("again");
            bool isMenu = btnName.Contains("menu");

            Text btnText = child.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                string txt = btnText.text.ToLower();
                isPlayAgain = isPlayAgain || txt.Contains("play again");
                isMenu = isMenu || txt.Contains("menu");
            }

            if (!isPlayAgain && !isMenu)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            child.gameObject.SetActive(State);
            if (!State) continue;

            RectTransform rt = child.GetComponent<RectTransform>();
            if (rt == null) continue;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = new Vector3(0.6f, 0.6f, 1f);
            rt.anchoredPosition = new Vector2(0f, isPlayAgain ? 45f : -45f);
        }
    }
}
