using System;
using UnityEngine;

namespace Luxodd.Game.Scripts.Runtime.Viewport
{
    /// <summary>
    /// Detects changes of Screen size/orientation and raises an event.
    /// Unity 2022 LTS+ friendly. Works on all platforms including WebGL.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class LuxoddViewportWatcher : MonoBehaviour
    {
        [Tooltip("If > 0, checks at this interval. If 0, checks every frame.")]
        [SerializeField] private float _checkIntervalSeconds = 0.1f;

        /// <summary>
        /// Fired when screen size or Screen.orientation changed.
        /// Args: width, height, orientation.
        /// </summary>
        public event Action<int, int, ScreenOrientation> Changed;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public ScreenOrientation Orientation { get; private set; }

        private float _nextCheckTime;

        private void Awake()
        {
            Snapshot();
        }

        private void OnEnable()
        {
            // Fire once so subscribers can initialize layout
            Changed?.Invoke(Width, Height, Orientation);
        }

        private void Update()
        {
            if (_checkIntervalSeconds > 0f && Time.unscaledTime < _nextCheckTime)
                return;

            if (_checkIntervalSeconds > 0f)
                _nextCheckTime = Time.unscaledTime + _checkIntervalSeconds;

            RaiseIfChanged();
        }

        private void RaiseIfChanged()
        {
            int w = Screen.width;
            int h = Screen.height;
            var o = Screen.orientation;

            if (w == Width && h == Height && o == Orientation)
                return;

            Width = w;
            Height = h;
            Orientation = o;

            Changed?.Invoke(Width, Height, Orientation);
        }

        private void Snapshot()
        {
            Width = Screen.width;
            Height = Screen.height;
            Orientation = Screen.orientation;
        }
    }
}