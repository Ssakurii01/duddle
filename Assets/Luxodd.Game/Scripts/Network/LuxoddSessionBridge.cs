using System;
using Luxodd.Game.HelpersAndUtils.Utils;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network
{
    [Serializable]
    internal class LuxoddSessionPayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("token")] public string Token { get; set; }
        [JsonProperty("wsUrl")] public string WsUrl { get; set; }
#else
        public string Token { get; set; }
        public string WsUrl { get; set; }
#endif
        
    }
    
    public class LuxoddSessionBridge : MonoBehaviour
    {
        
        public static LuxoddSessionBridge Instance { get; private set; }

        /// <summary>Token from events</summary>
        public string Token { get; private set; }

        /// <summary>Full WebSocket URL, sent from host.</summary>
        public string WebSocketUrl { get; private set; }
        
        internal LuxoddSessionPayload SessionPayload { get; private set; }

        /// <summary>Flag that event received and data valid</summary>
        public bool HasSession => !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(WebSocketUrl);

        /// <summary>Events that raised from payload luxodd:session.</summary>
        internal ISimpleEvent<LuxoddSessionPayload> OnSessionReceived => _onSessionReceived;
        
        private readonly SimpleEvent<LuxoddSessionPayload> _onSessionReceived = new  SimpleEvent<LuxoddSessionPayload>(); 

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void InitLuxoddSessionListener(string goName);

    [DllImport("__Internal")]
    private static extern string GetLuxoddSessionToken();

    [DllImport("__Internal")]
    private static extern string GetLuxoddSessionWsUrl();

    [DllImport("__Internal")]
    private static extern void ConnectWebSocket(string url);
#endif

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_WEBGL && !UNITY_EDITOR

        InitLuxoddSessionListener(gameObject.name);
        //Debug.Log("[LuxoddSessionBridge] InitLuxoddSessionListener called");
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnLuxoddSession)}] OK, InitLuxoddSessionListener called");
#endif
        }

        
        public void OnLuxoddSession(string json)
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnLuxoddSession)}] OK, session json = {json}");

            if (string.IsNullOrEmpty(json))
                return;

            try
            {
#if NEWTONSOFT_JSON
                var payload = JsonConvert.DeserializeObject<LuxoddSessionPayload>(json);
                if (payload != null)
                {
                    Token = payload.Token;
                    WebSocketUrl = payload.WsUrl;
                    SessionPayload = payload;
                }
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"[{DateTime.Now}][{GetType().Name}][{nameof(OnLuxoddSession)}] Error,  Failed to parse payload:  {e.Message}");
            }

            _onSessionReceived.Notify(SessionPayload);
        }
        
        public void RefreshFromJsState()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            Token = GetLuxoddSessionToken();
            WebSocketUrl = GetLuxoddSessionWsUrl();
            Debug.Log("[LuxoddSessionBridge] Refreshed from JS state. Token len = " +
                      (Token?.Length ?? 0) + ", wsUrl = " + WebSocketUrl);
        }
        catch (Exception e)
        {
            Debug.LogError("[LuxoddSessionBridge] RefreshFromJsState error: " + e);
        }
#endif
        }


        public void ConnectUsingCurrentSession()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (string.IsNullOrEmpty(WebSocketUrl))
        {
            Debug.LogWarning("[LuxoddSessionBridge] Cannot connect: WebSocketUrl is empty.");
            return;
        }

        Debug.Log("[LuxoddSessionBridge] Connecting to WebSocket: " + WebSocketUrl);
        ConnectWebSocket(WebSocketUrl);
#else
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(ConnectUsingCurrentSession)}] ConnectUsingCurrentSession: not in WebGL build.");
#endif
        }
    }
}