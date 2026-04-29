using System;
using Luxodd.Game.HelpersAndUtils.Utils;
using UnityEngine;

namespace Luxodd.Game.Scripts.HelpersAndUtils.Logger
{
    public class LogSuppressor : MonoBehaviour
    {
        [SerializeField] private bool _isLogsEnabled = true;

        public void EnableLogs()
        {
            _isLogsEnabled = true;
        }
        
        public void DisableLogs()
        {
            _isLogsEnabled = false;
        }

        private void Awake()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            NotifyLogEnabledEvent();
            
            EventAggregator.Subscribe<DebugLoggerEnableEvent>(OnDebugLoggerEnableEvent);
        }
        
        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            
            EventAggregator.Unsubscribe<DebugLoggerEnableEvent>(OnDebugLoggerEnableEvent);
        }

        //Log message handler, receives all logs from Unity
        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (type is LogType.Error or LogType.Exception)
            {
                Debug.unityLogger.Log(type, logString, stackTrace);
                LoggerHelper.LogError(
                    $"[{DateTime.Now}][{GetType().Name}][{nameof(OnLogMessageReceived)}] {logString}, stackTrace: {stackTrace}, type: {type}"
                );
            }
            else if (_isLogsEnabled)
            {
                Debug.unityLogger.Log(type, logString, stackTrace);
            }
        }

        private void OnDebugLoggerEnableEvent(object sender, DebugLoggerEnableEvent eventData)
        {
            _isLogsEnabled = eventData.IsLogEnabled;
        }

        private void NotifyLogEnabledEvent()
        {
            EventAggregator.Post(this, new DebugLoggerSetupEvent(){IsLogEnabled = _isLogsEnabled});
        }
    }
}
