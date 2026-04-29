using Luxodd.Game.Scripts.Runtime.Viewport;
using Luxodd.Game.Scripts.Runtime.Viewport.Context;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Example.Scripts.MobileDetectionDemo
{
    public class LuxoddOrientationUiDemo : MonoBehaviour
    {
        [Header("UI")] [SerializeField] private TMP_Text _label;
        [SerializeField] private RectTransform _indicator;
        [SerializeField] private Image _background;

        [Header("Viewport")] [SerializeField] private LuxoddViewportWatcher _watcher;

        private void Awake()
        {
            if (_watcher == null)
                _watcher = FindFirstObjectByType<LuxoddViewportWatcher>();
        }

        private void OnEnable()
        {
            if (_watcher != null)
                _watcher.Changed += OnViewportChanged;

            Apply();
        }

        private void OnDisable()
        {
            if (_watcher != null)
                _watcher.Changed -= OnViewportChanged;
        }

        private void OnViewportChanged(int width, int height, ScreenOrientation orientation)
        {
            Apply();
        }

        private void Apply()
        {
            bool isPortrait = Screen.height >= Screen.width;

            if (_label != null)
            {
                var message = "\n" + LuxoddRuntimeContext.GetVisualViewportInsetsDebugString() + "\n";
                _label.text =
                    (isPortrait ? "PORTRAIT" : "LANDSCAPE") +
                    "\n" + Screen.width + " x " + Screen.height +
                    "\n" + Screen.orientation +
                    "\n" + message;
            }

            if (_indicator != null)
            {
                _indicator.anchorMin = new Vector2(0.5f, 0.5f);
                _indicator.anchorMax = new Vector2(0.5f, 0.5f);
                _indicator.pivot = new Vector2(0.5f, 0.5f);

                _indicator.sizeDelta = isPortrait ? new Vector2(300f, 80f) : new Vector2(80f, 300f);
                _indicator.localRotation = isPortrait ? Quaternion.identity : Quaternion.Euler(0f, 0f, 90f);
            }

            if (_background != null)
            {
                // Using two different shades makes orientation changes obvious at a glance.
                _background.color = isPortrait ? new Color(0.1f, 0.2f, 0.1f, 1f) : new Color(0.2f, 0.1f, 0.1f, 1f);
            }
        }

    }
}
