using UnityEngine;
using Luxodd.Game.Scripts.Runtime.Viewport.Context;

namespace Luxodd.Game.Scripts.Runtime.UI
{
    public sealed class LuxoddWebGLSafeAreaApplier : MonoBehaviour
    {
        [Header("Target UI root (must be stretch-stretch)")]
        [SerializeField] private RectTransform _root;

        [Header("Optional extra padding")]
        [SerializeField] private float _extraBottomPixels = 0f;

        [Header("Refresh")]
        [SerializeField] private float _pollIntervalSeconds = 0.2f;

        [Header("Debug")]
        [Tooltip("If enabled, logs current insets. Disable for production builds.")]
        [SerializeField] private bool _logInsets = false;

        [Tooltip("If enabled, logs only when insets have changed.")]
        [SerializeField] private bool _logOnlyOnChange = true;

        private LuxoddRuntimeContext.VisualViewportInsets _last;
        private float _nextPollTime;

        private void Reset()
        {
            _root = transform as RectTransform;
        }

        private void Awake()
        {
            if (_root == null)
                _root = transform as RectTransform;
        }

        private void OnEnable()
        {
            Apply(force: true);
        }

        private void Update()
        {
            if (_pollIntervalSeconds <= 0f)
            {
                Apply(force: false);
                return;
            }

            if (Time.unscaledTime < _nextPollTime)
                return;

            _nextPollTime = Time.unscaledTime + _pollIntervalSeconds;
            Apply(force: false);
        }

        private void Apply(bool force)
        {
            if (_root == null)
                return;

            var insets = LuxoddRuntimeContext.GetVisualViewportInsets();

            if (_logInsets && (!_logOnlyOnChange || !ApproximatelySame(insets, _last)))
                Debug.Log($"[Luxodd] SafeArea insets L:{insets.Left:0} T:{insets.Top:0} R:{insets.Right:0} B:{insets.Bottom:0} View:{insets.ViewWidth:0}x{insets.ViewHeight:0}");

            insets.Bottom += _extraBottomPixels;

            if (!force && ApproximatelySame(insets, _last))
                return;

            _last = insets;

            _root.offsetMin = new Vector2(insets.Left, insets.Bottom);
            _root.offsetMax = new Vector2(-insets.Right, -insets.Top);
        }

        private static bool ApproximatelySame(LuxoddRuntimeContext.VisualViewportInsets a, LuxoddRuntimeContext.VisualViewportInsets b)
        {
            return Mathf.Approximately(a.Left, b.Left)
                && Mathf.Approximately(a.Top, b.Top)
                && Mathf.Approximately(a.Right, b.Right)
                && Mathf.Approximately(a.Bottom, b.Bottom)
                && Mathf.Approximately(a.ViewWidth, b.ViewWidth)
                && Mathf.Approximately(a.ViewHeight, b.ViewHeight);
        }
    }
}
