using UnityEngine;

namespace Luxodd.Game.Scripts.Runtime.Viewport
{
    /// <summary>
    /// Applies viewport changes to Camera and UI.
    /// - Camera: ResetAspect()
    /// - UI: Canvas.ForceUpdateCanvases()
    /// Extend here if you have SafeArea or custom scaling logic.
    /// </summary>
    public sealed class LuxoddViewportApplier : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LuxoddViewportWatcher _watcher;
        [SerializeField] private Camera _camera;

        [Header("Behavior")]
        [Tooltip("Force update canvases on every viewport change.")]
        [SerializeField] private bool _forceUpdateCanvases = true;

        private void Reset()
        {
            _camera = Camera.main;
            _watcher = FindFirstObjectByType<LuxoddViewportWatcher>();
        }

        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;
        }

        private void OnEnable()
        {
            if (_watcher == null)
                _watcher = FindFirstObjectByType<LuxoddViewportWatcher>();

            if (_watcher != null)
                _watcher.Changed += OnViewportChanged;
        }

        private void OnDisable()
        {
            if (_watcher != null)
                _watcher.Changed -= OnViewportChanged;
        }

        private void OnViewportChanged(int width, int height, ScreenOrientation orientation)
        {
            // Camera
            if (_camera != null)
                _camera.ResetAspect();

            // UI
            if (_forceUpdateCanvases)
                Canvas.ForceUpdateCanvases();
            
        }
    }
}
