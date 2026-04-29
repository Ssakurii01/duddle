using Luxodd.Game.Scripts.Runtime;
using Luxodd.Game.Scripts.Runtime.Viewport.Context;
using TMPro;
using UnityEngine;

namespace Luxodd.Game.Example.Scripts.MobileDetectionDemo
{
    public class MobileDetectionDemo : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text _statusText;

        [Header("Debug")]
        [Tooltip("If enabled, shows raw detection signals (for debugging).")]
        [SerializeField] private bool _showDebugSignals = false;

        [Tooltip("How often to refresh text/logs (seconds). 0 = only once on Start.")]
        [SerializeField] private float _refreshIntervalSeconds = 1.0f;

        [Tooltip("If enabled, writes the same text to the Console. Disable for production builds.")]
        [SerializeField] private bool _logToConsole = false;

        [Tooltip("If disabled, the demo text will be hidden.")]
        [SerializeField] private bool _isActive = true;

        private float _nextRefreshTime;

        public void Activate()
        {
            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
        }
        
        private void Start()
        {
            Print();
            _nextRefreshTime = Time.unscaledTime + _refreshIntervalSeconds;
        }
        private void Update()
        {
            if (_refreshIntervalSeconds <= 0f)
                return;

            if (Time.unscaledTime >= _nextRefreshTime)
            {
                _nextRefreshTime = Time.unscaledTime + _refreshIntervalSeconds;
                Print();
            }
        }

        private void Print()
        {
            if (_isActive == false)
            {
                _statusText.text = "";
                return;
            }
            
            bool isMobile = LuxoddRuntimeContext.IsMobileRuntime;

            string msg =
                $"<b>Luxodd Mobile Runtime Detection</b>\n" +
                $"IsMobileRuntime: <b>{isMobile}</b>\n" +
                $"Unity Platform: {Application.platform}\n" +
                $"Unity isMobilePlatform: {Application.isMobilePlatform}\n" +
                $"Screen: {Screen.width}x{Screen.height}\n";

            if (_showDebugSignals)
            {
                msg +=
                    $"\n<b>Debug signals</b>\n" +
                    $"MaxTouchPoints: {LuxoddRuntimeContext.MaxTouchPoints}\n" +
                    $"HasCoarsePointer: {LuxoddRuntimeContext.HasCoarsePointer}\n";
            }

            if (_logToConsole)
                Debug.Log("[Luxodd][MobileDetectionDemo]\n" + StripRichText(msg));
            if (_statusText != null)
                _statusText.text = msg;
        }

        private static string StripRichText(string s)
        {
            return s.Replace("<b>", "").Replace("</b>", "");
        }
    }
}
