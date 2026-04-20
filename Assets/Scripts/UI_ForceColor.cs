using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Forces a visible color on any UI element (TMP_Text, Image, any Graphic) at Start.
/// Use this when a text is rendering black / invisible and you can't change it via the
/// Inspector. Ignores a serialized "black with no alpha" value and falls back to white.
/// </summary>
public class UI_ForceColor : MonoBehaviour
{
    [Tooltip("Color to force on this element. If left as default (black/transparent), white is used.")]
    public Color Color = new Color(1f, 0.85f, 0.2f, 1f); // default: bright gold

    void Start()
    {
        Color final = Color;
        if (final.a < 0.01f || (final.r < 0.01f && final.g < 0.01f && final.b < 0.01f))
            final = UnityEngine.Color.white;

        TMP_Text tmp = GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.color = final;
            return;
        }

        Graphic g = GetComponent<Graphic>();
        if (g != null) g.color = final;
    }
}
