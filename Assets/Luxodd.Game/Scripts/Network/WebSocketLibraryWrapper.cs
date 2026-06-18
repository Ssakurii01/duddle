using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Luxodd.Game.HelpersAndUtils.Utils;
using Luxodd.Game.Scripts.HelpersAndUtils;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network
{
    public enum SessionOptionAction
    {
        Restart,
        Continue,
        End,
        Cancel
    }

    public class WebSocketLibraryWrapper : MonoBehaviour
    {
        [DllImport("__Internal")]
        public static extern void ConnectWebSocket(string url);

        [DllImport("__Internal")]
        public static extern void SendWebSocketMessage(string message);

        [DllImport("__Internal")]
        public static extern void CloseWebSocket();

        [DllImport("__Internal")]
        public static extern void NavigateToHome();

        [DllImport("__Internal")]
        public static extern void SendSessionEndMessage();

        [DllImport("__Internal")]
        public static extern void SendSessionOptionsMessageWithAction(string action);
        
        [DllImport("__Internal")]
        private static extern string GetParentHost();

        // GetWebSocketProtocol is correctly declared (and used) in
        // WebGlHostWrapper.cs as `string GetWebSocketProtocol()`. The duplicate
        // declaration that lived here returned `void`, which conflicted with
        // the JS-side signature and caused the WebGL build to fail with
        // wasm-ld: error: function signature mismatch: GetWebSocketProtocol.
        // Removed — nothing in this file referenced it anyway.

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] 
        private static extern void RegisterHostMessageListener(string goName);
#else
        private static void RegisterHostMessageListener(string goName) =>
            Debug.Log(
                $"[{DateTime.Now}][{nameof(RegisterHostMessageListener)}][{nameof(CloseWebSocketConnection)}] OK, goName={goName}");
#endif

        [DllImport("__Internal")]
        private static extern void SetExpectedHostOrigin(string origin);

        public ISimpleEvent<SessionOptionAction> OnSessionOptionAction => _onSessionOptionAction;

        public ISimpleEvent<string> MessageReceivedEvent => _webSocketMessageEvent;
        public ISimpleEvent<string> WebSocketConnectionErrorEvent => _webSockedConnectionErrorEvent;
        public ISimpleEvent WebSocketOpenedEvent => _webSocketOpenedEvent;
        public ISimpleEvent<int> WebSocketClosedEvent => _webSocketClosedEvent;

        private readonly SimpleEvent<string> _webSocketMessageEvent = new SimpleEvent<string>();
        private readonly SimpleEvent<string> _webSockedConnectionErrorEvent = new SimpleEvent<string>();
        private readonly SimpleEvent _webSocketOpenedEvent = new SimpleEvent();
        private readonly SimpleEvent<int> _webSocketClosedEvent = new SimpleEvent<int>();

        private readonly SimpleEvent<SessionOptionAction> _onSessionOptionAction =
            new SimpleEvent<SessionOptionAction>();

        public void StartWebSocket(string url)
        {
            LoggerHelper.Log($"[{GetType().Name}][{nameof(StartWebSocket)}] OK, url: {url}");
            _ = ConnectAsync(url);
        }

        public void CloseWebSocketConnection()
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(CloseWebSocketConnection)}] OK");
            CloseWebSocket();
        }

        public Task ConnectAsync(string url)
        {
            LoggerHelper.Log($"[{GetType().Name}][{nameof(ConnectAsync)}] OK, url: {url}");
            try
            {
                ConnectWebSocket(url);
                LoggerHelper.Log("Connecting to WebSocket...");
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError("WebSocket connection failed: " + ex.Message);
            }

            return Task.CompletedTask;
        }

        public void SendMessageToWebSocket(string message)
        {
            LoggerHelper.Log($"[{GetType().Name}][{nameof(SendMessageToWebSocket)}] " + message);
            try
            {
                SendWebSocketMessage(message);
                LoggerHelper.Log("Message sent: " + message);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError("Error sending WebSocket message: " + ex.Message);
            }
        }

        public void GoToHome()
        {
            NavigateToHome();
        }

        public void NotifySessionEnd()
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(NotifySessionEnd)}] OK");
#if UNITY_WEBGL && !UNITY_EDITOR
            SendSessionEndMessage();
#endif
        }

        public void SendSessionOptionsRestart()
        {
            PostSessionOptionCommand(SessionOptionAction.Restart);
        }

        public void SendSessionOptionsContinue()
        {
            PostSessionOptionCommand(SessionOptionAction.Continue);
        }

        public void SendSessionOptionsEnd()
        {
            PostSessionOptionCommand(SessionOptionAction.End);
        }

        public void OnHostSessionAction(string action)
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][Host→Game] Action allowed: {action}");

            var actionPascalCase = action.ToPascalCase();
            var sessionOptionAction = (SessionOptionAction)Enum.Parse(typeof(SessionOptionAction), actionPascalCase);

            _onSessionOptionAction.Notify(sessionOptionAction);

        }

        public void OnWebSocketOpen()
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnWebSocketOpen)}] OK");
            _webSocketOpenedEvent.Notify();
        }

        public void OnWebSocketMessage(string message)
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnWebSocketMessage)}] " + message);
            _webSocketMessageEvent.Notify(message);
        }

        public void OnWebSocketClose(int code)
        {
            LoggerHelper.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(OnWebSocketClose)}] OK, code: {code}");
            _webSocketClosedEvent.Notify(code);
        }

        public void OnWebSocketError(string error)
        {
            LoggerHelper.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(OnWebSocketError)}] OK, message: {error}");
            _webSockedConnectionErrorEvent.Notify(error);
        }

        public void ReceiveMessageFromWebSocket(string message)
        {
            LoggerHelper.Log($"[{DateTime.Now}]Message from WebSocket: " + message);
            _webSocketMessageEvent.Notify(message);
        }

        private void PostSessionOptionCommand(SessionOptionAction sessionOptionAction)
        {
            var actionOption = sessionOptionAction.ToString().ToLower();
            SendSessionOptionsMessageWithAction(actionOption);
        }

        private void Awake()
        {
            RegisterHostMessageListener(gameObject.name);
        }
    }
}