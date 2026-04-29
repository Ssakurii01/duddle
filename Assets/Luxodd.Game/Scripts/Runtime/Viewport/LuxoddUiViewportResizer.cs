using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Scripts.Runtime.Viewport
{
    public sealed class LuxoddUiViewportResizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LuxoddViewportWatcher _watcher;
        [SerializeField] private RectTransform _rootUi;
        [SerializeField] private CanvasScaler _canvasScaler;

        [Header("Optional safe padding")]
        [SerializeField] private bool _applyBottomPadding;
        [SerializeField] private float _bottomPaddingPixels = 48f;

        private void Awake()
        {
            if (_watcher == null)
                _watcher = FindFirstObjectByType<LuxoddViewportWatcher>();

            if (_canvasScaler == null)
                _canvasScaler = FindFirstObjectByType<CanvasScaler>();
        }

        private void OnEnable()
        {
            if (_watcher != null)
                _watcher.Changed += OnViewportChanged;

            ApplyNow();
        }

        private void OnDisable()
        {
            if (_watcher != null)
                _watcher.Changed -= OnViewportChanged;
        }

        private void OnViewportChanged(int width, int height, ScreenOrientation orientation)
        {
            ApplyNow();
        }

        private void ApplyNow()
        {
            // Ensure CanvasScaler is in the correct mode.
            if (_canvasScaler != null && _canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // Optional: apply a minimal bottom padding so critical buttons never hide behind browser UI.
            if (_applyBottomPadding && _rootUi != null)
            {
                _rootUi.offsetMin = new Vector2(_rootUi.offsetMin.x, _bottomPaddingPixels);
            }

            // Force Unity UI to recalculate layout immediately.
            Canvas.ForceUpdateCanvases();

            if (_rootUi != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rootUi);

            Canvas.ForceUpdateCanvases();
        }
    }
}
