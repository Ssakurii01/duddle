using Luxodd.Game.Scripts.Runtime.Viewport.Context;
using UnityEngine;

namespace Luxodd.Game.Example.Scripts.MobileDetectionDemo
{
    public sealed class LuxoddSafeAreaVisualizer : MonoBehaviour
    {
        [Header("Overlay Rects")]
        [SerializeField] private RectTransform _top;
        [SerializeField] private RectTransform _bottom;
        [SerializeField] private RectTransform _left;
        [SerializeField] private RectTransform _right;

        [Header("Options")]
        [SerializeField] private bool _updateEveryFrame = false;

        [Tooltip("How often to poll (seconds). Ignored if Update Every Frame is enabled.")]
        [SerializeField] private float _pollIntervalSeconds = 0.2f;

        private float _nextPollTime;

        private LuxoddRuntimeContext.VisualViewportInsets _last;

        private void OnEnable()
        {
            Apply(force: true);
        }

        private void Update()
        {
            if (_updateEveryFrame)
            {
                Apply(force: false);
                return;
            }

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
            var insets = LuxoddRuntimeContext.GetVisualViewportInsets();

            if (!force && ApproximatelySame(insets, _last))
                return;

            _last = insets;

            ApplyRect(_top, vertical: true, size: insets.Top);
            ApplyRect(_bottom, vertical: true, size: insets.Bottom);
            ApplyRect(_left, vertical: false, size: insets.Left);
            ApplyRect(_right, vertical: false, size: insets.Right);
        }

        private static void ApplyRect(RectTransform rect, bool vertical, float size)
        {
            if (rect == null)
                return;

            Vector2 sd = rect.sizeDelta;
            rect.sizeDelta = vertical
                ? new Vector2(sd.x, Mathf.Max(0f, size))
                : new Vector2(Mathf.Max(0f, size), sd.y);
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
