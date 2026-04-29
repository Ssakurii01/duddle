
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Luxodd.Game.HelpersAndUtils.Utils;
using Luxodd.Game.Scripts.HelpersAndUtils;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using Luxodd.Game.Scripts.Network.CommandHandler;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace Luxodd.Game.Scripts.Network
{
    public class WebSocketService : MonoBehaviour
    {
        public bool IsConnected => _isConnected;
        public string SessionToken => GetSessionToken();
        public ISimpleEvent<bool> ConnectedToServerEvent =>  _isConnectedEvent;
        
        
        [SerializeField] private NetworkSettingsDescriptor _settingsDescriptor = null;
        [SerializeField] private WebSocketLibraryWrapper _socketLibraryWrapper = null;

        [SerializeField] private FetchUrlQueryString _fetchUrlQueryString = null;
        [SerializeField] private WebGlHostWrapper _webGlHostWrapper = null;

        [SerializeField] private float _secondsBeforeError = 4f;

        //for test purpose
        [Header("For Test")] [SerializeField] private float _timeBeforeReconnect = 3f;

        private ClientWebSocket _clientWebSocket;
        private bool _isConnected;
        private bool _wasConnected = false;
        private bool _isInReconnection = false;
        private bool _isInFlushing = false;

        private System.Action<string> _successCallback;
        private System.Action<string> _errorCallback;

        private Action _onConnectedCallback;
        private Action _onConnectionErrorCallback;

        private Action<SessionOptionAction> _onSessionOptionCallback;
        
        private readonly SimpleEvent<bool> _isConnectedEvent = new SimpleEvent<bool>();

        private Dictionary<CommandRequestType, Queue<CommandRequestHandler>> _commandRequestHandlers =
            new Dictionary<CommandRequestType, Queue<CommandRequestHandler>>();

        private Queue<SendCommandData> _sendCommandDataQueue = new Queue<SendCommandData>();
        private LuxoddSessionPayload _sessionPayload;

        public void ConnectToServer(Action onSuccessCallback = null, Action onErrorCallback = null)
        {
            _onConnectedCallback = onSuccessCallback;
            _onConnectionErrorCallback = onErrorCallback;
            _ = StartConnectionAsync();
        }

        internal void ConnectToServer(LuxoddSessionPayload sessionPayload, Action onSuccessCallback = null, Action onErrorCallback = null)
        {
            _sessionPayload = sessionPayload;
            _onConnectedCallback = onSuccessCallback;
            _onConnectionErrorCallback = onErrorCallback;
            _ = StartConnectionAsync();
        }

        public void CloseConnection()
        {
            _isConnected = false;
#if !UNITY_EDITOR
            _socketLibraryWrapper.CloseWebSocketConnection();
#else
            _clientWebSocket
                ?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close Application", CancellationToken.None)?.Wait();
#endif
            _isConnectedEvent.Notify(_isConnected);
        }

        public void BackToSystemWithError(string message, string error)
        {
            LoggerHelper.LogError(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(BackToSystemWithError)}] OK, message: {message}, error: {error}");
            _socketLibraryWrapper.NotifySessionEnd();
        }

        public void BackToSystem()
        {
            LoggerHelper.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(BackToSystem)}] OK");
            _socketLibraryWrapper.NotifySessionEnd();
        }

        public void SendSessionOptionRestart(Action<SessionOptionAction> callback)
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(SendSessionOptionRestart)}] OK");
            _onSessionOptionCallback = callback;
#if UNITY_WEBGL && !UNITY_EDITOR
            _socketLibraryWrapper.SendSessionOptionsRestart();
#endif
        }

        public void SendSessionOptionContinue(Action<SessionOptionAction> callback)
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(SendSessionOptionContinue)}] OK");
            _onSessionOptionCallback = callback;
#if UNITY_WEBGL && !UNITY_EDITOR
            _socketLibraryWrapper.SendSessionOptionsContinue();
#endif
        }

        public void SendSessionOptionEnd(Action<SessionOptionAction> callback)
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(SendSessionOptionEnd)}] OK");

            _onSessionOptionCallback = callback;
#if UNITY_WEBGL && !UNITY_EDITOR
            _socketLibraryWrapper.SendSessionOptionsEnd();
#endif
        }

        public void SendCommand(CommandRequestType commandRequestType, string commandRequestJson,
            Action<CommandRequestHandler> onSuccess)
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(SendCommand)}] OK, _isConnected: {_isConnected}");
            if (_isConnected)
            {
                ProcessCommandInner(commandRequestType, commandRequestJson, onSuccess);
            }
            else
            {
                AddCommandToDispatch(commandRequestType, commandRequestJson, onSuccess);

                if (_wasConnected && _isInReconnection == false)
                {
                    StartCoroutine(WaitForConnection());
                }
            }

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
            _socketLibraryWrapper.WebSocketOpenedEvent.AddListener(OnWebSocketConnectedHandler);
            _socketLibraryWrapper.WebSocketConnectionErrorEvent.AddListener(OnWebSocketConnectionErrorHandler);
            _socketLibraryWrapper.WebSocketClosedEvent.AddListener(OnWebSocketClosedHandler);
            _socketLibraryWrapper.MessageReceivedEvent.AddListener(OnMessageReceived);
            
            _socketLibraryWrapper.OnSessionOptionAction.AddListener(OnSessionOptionsCallback);
        }

        private void ProcessCommandInner(CommandRequestType commandRequestType, string commandRequestJson,
            Action<CommandRequestHandler> onSuccess)
        {
            AddCommandRequestHandler(commandRequestType, onSuccess);
            SendEventInner(commandRequestJson);
        }

        private void OnWebSocketConnectionErrorHandler(string error)
        {
            LoggerHelper.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(OnWebSocketConnectionErrorHandler)}] OK, error:{error}");
            _isConnected = false;
        }

        private void OnWebSocketClosedHandler(int code)
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnWebSocketConnectionErrorHandler)}] OK");
            _isConnected = false;
        }

        private void UnsubscribeFromEvents()
        {
            _socketLibraryWrapper.WebSocketOpenedEvent.RemoveListener(OnWebSocketConnectedHandler);
            _socketLibraryWrapper.WebSocketConnectionErrorEvent.RemoveListener(OnWebSocketConnectionErrorHandler);
            _socketLibraryWrapper.WebSocketClosedEvent.RemoveListener(OnWebSocketClosedHandler);
            _socketLibraryWrapper.MessageReceivedEvent.RemoveListener(OnMessageReceived);
        }

        private string GetSessionToken()
        {
            var isDebug = false;
#if UNITY_EDITOR
            isDebug = true;
#endif

            var sessionToken = isDebug == false ? _fetchUrlQueryString.Token : _settingsDescriptor.DeveloperDebugToken;
            return sessionToken;
        }

        private async Task StartConnectionAsync()
        {
            await Task.Yield();

            var isDebug = false;
            var serverUrlRaw = _settingsDescriptor.ServerAddress;
            if (string.IsNullOrEmpty(serverUrlRaw))
            {
                serverUrlRaw = _fetchUrlQueryString.WSUrl;
            }
#if UNITY_EDITOR
            isDebug = true;
#endif

            var host = _webGlHostWrapper.GetParentHostSafe();
            var protocol = _webGlHostWrapper.GetWebSocketProtocolSafe();
            var shouldUseHost = string.IsNullOrEmpty(host) == false && string.IsNullOrEmpty(protocol) == false && host.Contains("localhost") == false;
            var fullUrl = shouldUseHost ? $"{protocol}//{host}/ws" : string.Empty;
            var token = isDebug == false ? _fetchUrlQueryString.Token : _settingsDescriptor.DeveloperDebugToken;
            
            var serverUrl = $"{serverUrlRaw}?token={token}";

            if (shouldUseHost)
            {
                serverUrl = $"{fullUrl}?token={token}";
            }

            if (_sessionPayload != null)
            {
                serverUrl = $"{_sessionPayload.WsUrl}?token={_sessionPayload.Token}";
            }

            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] OK: {serverUrl}, fullUrl: {fullUrl}");

            var websocketUri =
                new Uri(serverUrl);



#if !UNITY_EDITOR
            try
            {
                await Task.Yield();
                LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] OK, connecting to: {websocketUri}...");
                _socketLibraryWrapper.StartWebSocket(websocketUri.AbsoluteUri);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] Error: {ex}");
                Console.WriteLine(ex);
                _isConnected = false;
                throw;
            }
#else
            LoggerHelper.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] OK, connecting to: {websocketUri.AbsoluteUri}");
            try
            {
                _clientWebSocket = new ClientWebSocket();
                await _clientWebSocket.ConnectAsync(websocketUri, CancellationToken.None);
                _isConnected = true;
                _wasConnected = true;
                
                LoggerHelper.Log(
                    $"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] OK, connected to: {websocketUri.AbsoluteUri}");
                OnWebSocketConnectedHandler();
                _ = ReceiveMessage();
            }
            catch (Exception e)
            {
                LoggerHelper.LogError($"[{DateTime.Now}][{GetType().Name}][{nameof(StartConnectionAsync)}] Error: {e}");
                Console.WriteLine(e);
                _isConnected = false;
                throw;
            }

#endif
            _isConnectedEvent.Notify(_isConnected);
        }

        private void OnMessageReceived(string message)
        {
            //LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnMessageReceived)}] " + message);
            UnityMainThread.Worker.AddJob(() => HandleMessageReceived(message));
        }

        private async void OnApplicationQuit()
        {
            if (_clientWebSocket != null && _clientWebSocket.State == WebSocketState.Open)
            {
                await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Application closing",
                    CancellationToken.None);
            }
        }

        private async Task ReceiveMessage()
        {
            var buffer = new byte[1024 * 4];
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result =
                        await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                        {
                            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            LoggerHelper.Log(
                                $"[{DateTime.Now}][{GetType().Name}][{nameof(ReceiveMessage)}] OK, received message: {receivedMessage}");
                            OnMessageReceived(receivedMessage);
                            //UnityMainThread.Worker.AddJob(() => _successCallback?.Invoke(receivedMessage));

                            break;
                        }
                        case WebSocketMessageType.Close:
                            LoggerHelper.Log(
                                $"[{DateTime.Now}][{GetType().Name}][{nameof(ReceiveMessage)}] OK, closed");
                            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing",
                                CancellationToken.None);
                            break;
                        case WebSocketMessageType.Binary:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError($"[{GetType().Name}][{nameof(ReceiveMessage)}] OK, error: {ex.Message}");
                    UnityMainThread.Worker.AddJob(() => _errorCallback?.Invoke(ex.Message));
                }
            }
        }

        private async Task SendMessageWebsocket(string message)
        {
            if (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open)
            {
                LoggerHelper.LogError($"[{GetType().Name}][{nameof(SendMessageWebsocket)}] OK, not connected");
                _errorCallback?.Invoke($"[{GetType().Name}][{nameof(SendMessageWebsocket)}] Not connected");
                return;
            }

            var messageBytes = Encoding.UTF8.GetBytes(message);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true,
                CancellationToken.None);
            LoggerHelper.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(SendMessageWebsocket)}] OK, message sent: {message}");
        }

        private async void SendEventInner(string commandRequestRaw)
        {
#if !UNITY_EDITOR
            try
            {
                if (_isConnected == false)
                {
                    LoggerHelper.LogError($"[{GetType().Name}][{nameof(SendEventInner)}] Not connected");
                    return;
                }

                await Task.Yield();

                _socketLibraryWrapper.SendMessageToWebSocket(commandRequestRaw);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"[{GetType().Name}][{nameof(SendEventInner)}] OK, error: {ex.Message}");
            }
#else
            await SendMessageWebsocket(commandRequestRaw);
#endif
        }

        private void OnWebSocketConnectedHandler()
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnWebSocketConnectedHandler)}] OK");
            _isConnected = true;
            _wasConnected = true;
            _isConnectedEvent.Notify(_isConnected);
            _onConnectedCallback?.Invoke();
        }

        private void AddCommandRequestHandler(CommandRequestType commandRequestType,
            Action<CommandRequestHandler> onSuccess)
        {
            var commandRequestHandler = new CommandRequestHandler()
            {
                CommandRequestType = commandRequestType,
                Id = 0,
                OnCommandResponseSuccessHandler = onSuccess,
            };

            if (_commandRequestHandlers.TryGetValue(commandRequestType, out var queueOfCommandRequestHandlers))
            {
                commandRequestHandler.Id = queueOfCommandRequestHandlers.Count;
                queueOfCommandRequestHandlers.Enqueue(commandRequestHandler);
            }
            else
            {
                var queue = new Queue<CommandRequestHandler>();
                queue.Enqueue(commandRequestHandler);
                _commandRequestHandlers.Add(commandRequestType, queue);
            }
        }

        private void HandleMessageReceived(string message)
        {
#if NEWTONSOFT_JSON
            //LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(HandleMessageReceived)}] OK, message: {message}");
            var responseJson = JsonConvert.DeserializeObject<CommandResponse>(message);
            if (responseJson == null) return;

            if (responseJson.Type.Contains("_"))
            {
                responseJson.Type = responseJson.Type.ToPascalCaseStyle();
            }

            if (Enum.TryParse<CommandResponseType>(responseJson.Type, out var commandResponseType) == false) return;

            var commandRequestType = commandResponseType.ToCommandRequestType();

            if (_commandRequestHandlers.TryGetValue(commandRequestType, out var queueOfCommandRequestHandlers) ==
                false) return;

            if (queueOfCommandRequestHandlers.Count > 0)
            {
                var commandRequestHandler = queueOfCommandRequestHandlers.Dequeue();
                commandRequestHandler.CommandResponse = responseJson;
                commandRequestHandler.RawResponse = message;
                commandRequestHandler.OnCommandResponseSuccessHandler?.Invoke(commandRequestHandler);
            }
            else
            {
                LoggerHelper.LogError(
                    $"[{DateTime.Now}][{GetType().Name}][{nameof(HandleMessageReceived)}] Error: Message: {message}, did not contain any command");
            }
#endif
        }

        //if connected - OK
        //if was connected and now disconnected - add to query
        //if was disconnected and now connected

        private IEnumerator WaitForConnection()
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(WaitForConnection)}] OK");
            _isInReconnection = true;
            var deadline = Time.time + _secondsBeforeError;

            while (_isConnected == false && Time.time < deadline)
            {
                yield return new WaitForSeconds(0.3f);
            }

            _isInReconnection = false;

            Debug.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(WaitForConnection)}] OK, _isConnected={_isConnected}");

            if (_isConnected && _isInFlushing == false)
            {
                // flush dispatched commands
                StartCoroutine(FlushCommands());
            }
            else if (Time.time >= deadline)
            {
                // connection error - go back to the system 
                BackToSystemWithError("Can't connect to server", "Connection error");
            }
        }

        private IEnumerator FlushCommands()
        {
            Debug.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(FlushCommands)}] OK, total commands: {_sendCommandDataQueue.Count}");
            _isInFlushing = true;
            while (_sendCommandDataQueue.Count > 0)
            {
                var command = _sendCommandDataQueue.Dequeue();
                ProcessCommandInner(command.CommandRequestType, command.CommandRequestJson,
                    command.OnCommandResponseSuccessHandler);
                yield return new WaitForSeconds(0.3f);
            }

            _isInFlushing = false;
        }

        private void AddCommandToDispatch(CommandRequestType commandRequestType, string commandRequestJson,
            Action<CommandRequestHandler> onSuccess)
        {
            Debug.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(AddCommandToDispatch)}] OK, commandRequestType: {commandRequestType}, commandRequestJson: {commandRequestJson}");

            var command = new SendCommandData()
            {
                CommandRequestType = commandRequestType,
                CommandRequestJson = commandRequestJson,
                OnCommandResponseSuccessHandler = onSuccess
            };
            _sendCommandDataQueue.Enqueue(command);
        }

        [ContextMenu("Test Close and Reconnect")]
        private void TestCloseConnectionAndReconnect()
        {
            CloseConnection();

            CoroutineManager.DelayedAction(_timeBeforeReconnect, () => ConnectToServer());
        }

        private void OnSessionOptionsCallback(SessionOptionAction action)
        {
            _onSessionOptionCallback?.Invoke(action);
        }

        public class SendCommandData
        {
            public CommandRequestType CommandRequestType { get; set; }
            public string CommandRequestJson { get; set; }
            public Action<CommandRequestHandler> OnCommandResponseSuccessHandler { get; set; }
        }
    }
}
