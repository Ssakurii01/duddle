using System.Globalization;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Luxodd.Game.Scripts.Runtime.Viewport.Context
{
     public static class LuxoddRuntimeContext
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int Luxodd_IsMobileRuntime();
        [DllImport("__Internal")] private static extern int Luxodd_GetMaxTouchPoints();
        [DllImport("__Internal")] private static extern int Luxodd_HasCoarsePointer();

        [DllImport("__Internal")]
        private static extern void Luxodd_GetVisualViewportInsets(
            out float left,
            out float top,
            out float right,
            out float bottom,
            out float viewWidth,
            out float viewHeight
        );
#endif

        public static bool IsMobileRuntime
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return Luxodd_IsMobileRuntime() != 0;
#else
                return Application.isMobilePlatform;
#endif
            }
        }

        // -------- Debug helpers (optional) --------

        public static int MaxTouchPoints
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return Luxodd_GetMaxTouchPoints();
#else
                return UnityEngine.Input.touchSupported ? UnityEngine.Input.touchCount : 0;
#endif
            }
        }

        public static bool HasCoarsePointer
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return Luxodd_HasCoarsePointer() != 0;
#else
                return UnityEngine.Input.touchSupported;
#endif
            }
        }

        // -------- VisualViewport Insets (WebGL mobile safe area) --------

        public struct VisualViewportInsets
        {
            public float Left;
            public float Top;
            public float Right;
            public float Bottom;

            public float ViewWidth;
            public float ViewHeight;

            public bool IsZero => Left == 0f && Top == 0f && Right == 0f && Bottom == 0f;
        }

        public static VisualViewportInsets GetVisualViewportInsets()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Luxodd_GetVisualViewportInsets(out float left, out float top, out float right, out float bottom, out float viewW, out float viewH);

            return new VisualViewportInsets
            {
                Left = left,
                Top = top,
                Right = right,
                Bottom = bottom,
                ViewWidth = viewW,
                ViewHeight = viewH
            };
#else
            return default;
#endif
        }

        public static string GetVisualViewportInsetsDebugString()
        {
            VisualViewportInsets i = GetVisualViewportInsets();

            // In editor/non-WebGL this will be zeros, which is expected.
            return string.Format(CultureInfo.InvariantCulture,
                "VisualViewport:\n" +
                "Insets L:{0:0} T:{1:0} R:{2:0} B:{3:0}\n" +
                "View {4:0}x{5:0}",
                i.Left, i.Top, i.Right, i.Bottom, i.ViewWidth, i.ViewHeight);
        }
    }
}