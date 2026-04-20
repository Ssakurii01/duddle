using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Plays a "3 … 2 … 1 … GO!" countdown animation and then hands control back via callback.
/// Runs on unscaled time so it works while the game is paused (Time.timeScale = 0).
/// Game_Controller calls StartCountdown.Play(this, OnFinished) to trigger it.
/// </summary>
public class Start_Countdown : MonoBehaviour
{
    public float SecondDuration = 0.65f;
    public float GoDuration = 0.55f;

    private TextMeshProUGUI text;

    public static Start_Countdown Play(MonoBehaviour host, System.Action onFinished)
    {
        GameObject obj = new GameObject("Start_Countdown");
        Start_Countdown sc = obj.AddComponent<Start_Countdown>();
        host.StartCoroutine(sc.Run(onFinished));
        return sc;
    }

    IEnumerator Run(System.Action onFinished)
    {
        CreateText();

        yield return ShowNumber("3", new Color(1f, 1f, 1f));
        yield return ShowNumber("2", new Color(1f, 0.85f, 0.3f));
        yield return ShowNumber("1", new Color(1f, 0.5f, 0.25f));
        yield return ShowGo();

        if (text != null) Destroy(text.gameObject);
        Destroy(gameObject);

        if (onFinished != null) onFinished();
    }

    void CreateText()
    {
        Canvas canvas = FindOverlayCanvas();
        if (canvas == null) return;

        GameObject obj = new GameObject("Countdown_Text");
        obj.transform.SetParent(canvas.transform, false);

        text = obj.AddComponent<TextMeshProUGUI>();
        text.text = "3";
        text.fontSize = 220;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        text.outlineColor = new Color32(0, 0, 0, 220);
        text.outlineWidth = 0.2f;

        RectTransform rt = text.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(600, 400);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.zero;
    }

    Canvas FindOverlayCanvas()
    {
        // Prefer a Screen Space - Overlay canvas that isn't the submenu leaderboard
        Canvas best = null;
        Canvas[] all = Object.FindObjectsOfType<Canvas>();
        foreach (Canvas c in all)
        {
            if (c.renderMode != RenderMode.ScreenSpaceOverlay) continue;
            if (best == null || c.sortingOrder > best.sortingOrder) best = c;
        }
        if (best != null) return best;
        return all.Length > 0 ? all[0] : null;
    }

    IEnumerator ShowNumber(string label, Color color)
    {
        if (text == null) yield break;

        text.text = label;
        Camera_Shake.Shake(0.08f, 0.06f);

        float t = 0f;
        while (t < SecondDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / SecondDuration;

            // Scale: pop in, hold, grow out
            float scale = p < 0.25f ? Mathf.Lerp(0.2f, 1.3f, p / 0.25f)
                       : p < 0.5f  ? Mathf.Lerp(1.3f, 1.0f, (p - 0.25f) / 0.25f)
                       :             Mathf.Lerp(1.0f, 1.4f, (p - 0.5f) / 0.5f);
            text.rectTransform.localScale = Vector3.one * scale;

            // Alpha: fade in fast, hold, fade out
            float alpha = p < 0.12f ? p / 0.12f
                        : p < 0.75f ? 1f
                        :             1f - (p - 0.75f) / 0.25f;
            Color c = color; c.a = alpha; text.color = c;

            yield return null;
        }
    }

    IEnumerator ShowGo()
    {
        if (text == null) yield break;

        text.text = "GO!";
        text.fontSize = 260;
        Color go = new Color(0.3f, 1f, 0.35f);

        Camera_Shake.Shake(0.25f, 0.22f);

        // Particle burst at screen center (world space)
        if (Camera.main != null)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 10f));
            world.z = 0f;
            Jump_Particles.Burst(world, new Color(0.3f, 1f, 0.35f, 0.9f), 18);
            Jump_Particles.Burst(world, new Color(1f, 1f, 0.3f, 0.9f), 12);
        }

        float t = 0f;
        while (t < GoDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / GoDuration;

            float scale = p < 0.2f ? Mathf.Lerp(0.3f, 1.6f, p / 0.2f)
                       :             Mathf.Lerp(1.6f, 2.4f, (p - 0.2f) / 0.8f);
            text.rectTransform.localScale = Vector3.one * scale;

            float alpha = p < 0.15f ? p / 0.15f : 1f - (p - 0.15f) / 0.85f;
            Color c = go; c.a = alpha; text.color = c;

            yield return null;
        }
    }
}
