using System;
using System.Collections;
using Luxodd.Game.HelpersAndUtils.Utils;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using Luxodd.Game.Scripts.Network.CommandHandler;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network
{
    public class HealthStatusCheckService : MonoBehaviour
    {
        [SerializeField] private float _howOften = 2f;
        
        [SerializeField] private WebSocketCommandHandler _commandHandler;
        [SerializeField] private ErrorHandlerService _errorHandlerService;
        
        private bool _isActive;
        
        private WaitForSeconds _waitForSeconds;
        private Coroutine _coroutine;

        public void Activate()
        {
            if (_isActive) return;
            
            _isActive = true;
            _coroutine = StartCoroutine(SendHealthStatusRequest());
        }

        public void Deactivate()
        {
            if (_isActive == false) return;
            _isActive = false;
            StopCoroutine(_coroutine);
        }

        private void Awake()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            EventAggregator.Subscribe<DebugHealthCheckStatusEvent>(OnDebugHealthCheckEventHandler);
        }

        private void UnsubscribeFromEvents()
        {
            EventAggregator.Unsubscribe<DebugHealthCheckStatusEvent>(OnDebugHealthCheckEventHandler);
        }

        private IEnumerator SendHealthStatusRequest()
        {
            _waitForSeconds = new WaitForSeconds(_howOften);
            
            while (_isActive)
            {
                yield return _waitForSeconds;
                _commandHandler.SendHealthCheckStatusCommand(OnHealthStatusCheckSuccessHandler, OnHealthStatusCheckFailureHandler);
            }
        }

        private void OnHealthStatusCheckSuccessHandler()
        {
            LoggerHelper.Log($"[{GetType().Name}][{nameof(OnHealthStatusCheckSuccessHandler)}] OK");
        }

        private void OnHealthStatusCheckFailureHandler(int statusCode, string reason)
        {
            LoggerHelper.Log($"[{GetType().Name}][{nameof(OnHealthStatusCheckFailureHandler)}] OK, reason: {reason}");
            Deactivate();
            _errorHandlerService.HandleConnectionError($"health status check failure, code: {statusCode}, reason: {reason}");
        }

        private void OnDebugHealthCheckEventHandler(object sender, DebugHealthCheckStatusEvent eventArgs)
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnDebugHealthCheckEventHandler)}] OK");
            if (eventArgs.IsOn)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }
    }
}
