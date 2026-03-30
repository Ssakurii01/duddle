using UnityEngine;

public class Score_Popup : MonoBehaviour {

    private float lifetime = 1f;
    private float timer = 0f;
    private TextMesh textMesh;
    private Color startColor;
    private bool initialized = false;

    public static void Create(Vector3 position, string text, Color color)
    {
        GameObject obj = new GameObject("Score_Popup");
        obj.transform.position = position + new Vector3(0, 0.3f, 0);

        Score_Popup popup = obj.AddComponent<Score_Popup>();
        popup.textMesh = obj.AddComponent<TextMesh>();
        popup.textMesh.text = text;
        popup.textMesh.fontSize = 36;
        popup.textMesh.characterSize = 0.04f;
        popup.textMesh.color = color;
        popup.textMesh.alignment = TextAlignment.Center;
        popup.textMesh.anchor = TextAnchor.MiddleCenter;
        popup.textMesh.fontStyle = FontStyle.Bold;
        popup.startColor = color;
        popup.initialized = true;

        // Render in front of everything
        obj.GetComponent<MeshRenderer>().sortingOrder = 50;
    }

    void Update()
    {
        if (!initialized) return;
        timer += Time.deltaTime;

        // Float upward
        transform.position += new Vector3(0, 1.5f * Time.deltaTime, 0);

        // Scale up then shrink
        float t = timer / lifetime;
        float scale = t < 0.2f ? Mathf.Lerp(0.5f, 1.2f, t / 0.2f) :
                      t < 0.4f ? Mathf.Lerp(1.2f, 1f, (t - 0.2f) / 0.2f) : 1f;
        transform.localScale = Vector3.one * scale;

        // Fade out
        float alpha = 1f - Mathf.Clamp01((t - 0.5f) / 0.5f);
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
