using System;
using System.Collections;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network
{
    public enum SessionFlowResult
    {
        EventPath,
        LegacyPath,
        Error
    }
    
    public class SessionFlowController : MonoBehaviour
    {
        public bool SessionPayloadIsPresent => _sessionPayload != null;
        
        [SerializeField] private float _waitTimeoutSeconds = 5f;
        [SerializeField] private LuxoddSessionBridge _sessionBridge;
        [SerializeField] private WebSocketService _socketService;
        
        private LuxoddSessionPayload _sessionPayload;
        
        private System.Action _onConnectedToServerCallback;
        private System.Action _onConnectToServerErrorCallback;
        
        private Coroutine _coroutine;

        private void Awake()
        {
            _sessionBridge.OnSessionReceived.AddListener(OnSessionReceivedHandler);
        }
        
        public void ActivateProcess(System.Action onConnectedToServerCallback, System.Action onConnectToServerErrorCallback)
        {
            _onConnectedToServerCallback = onConnectedToServerCallback;
            _onConnectToServerErrorCallback = onConnectToServerErrorCallback;

            _coroutine = StartCoroutine(StartListenEventProcess());
        }

        private void OnSessionReceivedHandler(LuxoddSessionPayload payload)
        {
            _sessionPayload = payload;
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }
            
            CheckPayloadAndConnectToServer();
        }

        private void GoLegacyPath()
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(GoLegacyPath)}] OK");
            _socketService.ConnectToServer(_onConnectedToServerCallback, _onConnectToServerErrorCallback);
        }

        private void GoEventPath()
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(GoEventPath)}] OK");
            _socketService.ConnectToServer(_sessionPayload, _onConnectedToServerCallback, _onConnectToServerErrorCallback);
        }
        
        private IEnumerator StartListenEventProcess()
        {
            var timeCounter = 0f;

            while (timeCounter < _waitTimeoutSeconds && _sessionBridge.HasSession == false)
            {
                timeCounter += Time.deltaTime;
                yield return null;
            }
            
            CheckPayloadAndConnectToServer();
        }

        private void CheckPayloadAndConnectToServer()
        {
            if (_sessionPayload == null)
            {
                GoLegacyPath();
            }
            else
            {
                GoEventPath();
            }
        }
    }
}
