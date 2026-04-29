using Luxodd.Game.HelpersAndUtils.Utils;
using UnityEngine;

namespace Luxodd.Game.Scripts.HelpersAndUtils.Logger
{
    public class LoggerHelper : MonoBehaviour
    {
        private static LoggerHelper _instance;

        public static LoggerHelper Instance => _instance;

        [SerializeField] private bool _isLogsEnabled = false;
        [SerializeField] private bool _isWarningsEnabled = true;

        private void Awake()
        {
            _instance = this;
            
            EventAggregator.Subscribe<DebugLoggerEnableEvent>(OnDebugLoggerEnableEvent);
            
            DontDestroyOnLoad(gameObject);
            
            NotifyLogEnabledEvent();
        }
        
        private void OnDestroy()
        {
            EventAggregator.Unsubscribe<DebugLoggerEnableEvent>(OnDebugLoggerEnableEvent);
        }

        private void LogInner(string message)
        {
            if (_isLogsEnabled == false) return;
            
            Debug.Log(message);
        }

        private void LogErrorInner(string message)
        {
            Debug.LogError(message);
        }

        private void LogWarningInner(string message)
        {
            if (_isWarningsEnabled == false) return;
            
            Debug.LogWarning(message);
        }
        
        private void OnDebugLoggerEnableEvent(object sender, DebugLoggerEnableEvent eventData)
        {
            _isLogsEnabled = eventData.IsLogEnabled;
        }

        private void NotifyLogEnabledEvent()
        {
            EventAggregator.Post(this,
                new DebugLoggerSetupEvent()
                    { IsLogEnabled = _isLogsEnabled, IsWarningLogEnabled = _isWarningsEnabled });
        }

        public static void Log(string message)
        {
            Instance.LogInner(message);
        }

        public static void LogError(string message)
        {
            Instance.LogErrorInner(message);
        }

        public static void LogWarning(string message)
        {
            Instance.LogWarningInner(message);
        }
    }
}
