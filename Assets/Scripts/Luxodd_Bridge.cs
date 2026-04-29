using System;
using System.Collections;
using UnityEngine;
using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.CommandHandler;
using Luxodd.Game.Scripts.Game.Leaderboard;

/// <summary>
/// Persistent bridge between Monkey Jump and the Luxodd platform.
///
/// • Lives for the whole app lifetime via DontDestroyOnLoad so it works across
///   Main_Menu, In_Game and Leaderboard scenes without being recreated.
/// • Connects to the Luxodd server on first start.
/// • Forwards level begin / level end and leaderboard requests.
/// • Drives the in-game transaction (Restart popup) after game over.
///
/// Setup in Unity (do once in the FIRST scene that loads, e.g. Main_Menu):
///   1. Drag Assets/Luxodd.Game/Prefabs/UnityPluginPrefab into the scene.
///   2. Create an empty GameObject called "Luxodd_Bridge" and add this script.
///   3. Drag the WebSocketService, WebSocketCommandHandler and
///      HealthStatusCheckService components from UnityPluginPrefab into the
///      matching slots on this script.
///
/// The bridge will mark the plugin prefab as DontDestroyOnLoad too so its
/// references stay valid after scene changes.
/// </summary>
public class Luxodd_Bridge : MonoBehaviour
{
    public static Luxodd_Bridge Instance;

    [Header("Plugin services (drag from UnityPluginPrefab)")]
    [SerializeField] private WebSocketService          _webSocketService;
    [SerializeField] private WebSocketCommandHandler   _webSocketCommandHandler;
    [SerializeField] private HealthStatusCheckService  _healthStatusCheckService;

    [Header("Behaviour")]
    [Tooltip("Auto-connect to the server on game start.")]
    public bool AutoConnect = true;

    [Tooltip("Auto-activate health check after a successful connection.")]
    public bool ActivateHealthCheck = true;

    [Tooltip("Seconds to wait after the leaderboard appears before showing the Restart popup.")]
    public float RestartPopupDelay = 3f;

    [Tooltip("Level number to send to the server. Monkey Jump is one continuous run, so we keep it at 1.")]
    public int LevelNumber = 1;

    // Public read-only state
    public bool IsConnected   { get; private set; }
    public string PlayerName  { get; private set; } = "Player";
    public int CurrentBalance { get; private set; }
    public LeaderboardDataResponse CachedLeaderboard { get; private set; }

    void Awake()
    {
        // Singleton — keep one instance for the whole app
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Make sure the plugin prefab also persists, otherwise its services
        // get destroyed on scene change and our references go null.
        TryPersistPluginRoot(_webSocketService);
        TryPersistPluginRoot(_webSocketCommandHandler);
        TryPersistPluginRoot(_healthStatusCheckService);
    }

    void Start()
    {
        if (AutoConnect) Connect();
    }

    static void TryPersistPluginRoot(Component c)
    {
        if (c == null) return;
        Transform root = c.transform.root;
        if (root != null) DontDestroyOnLoad(root.gameObject);
    }

    // ---------------- Connection ----------------

    public void Connect()
    {
        if (_webSocketService == null)
        {
            Debug.LogWarning("[Luxodd_Bridge] WebSocketService is not assigned in the Inspector.");
            return;
        }

        _webSocketService.ConnectToServer(OnConnectSuccess, OnConnectError);
    }

    void OnConnectSuccess()
    {
        IsConnected = true;
        Debug.Log("[Luxodd_Bridge] Connected to server.");

        if (ActivateHealthCheck && _healthStatusCheckService != null)
            _healthStatusCheckService.Activate();

        FetchProfile();
        FetchBalance();
    }

    void OnConnectError()
    {
        IsConnected = false;
        Debug.LogError("[Luxodd_Bridge] Failed to connect to Luxodd server.");
    }

    // ---------------- Profile / balance ----------------

    public void FetchProfile()
    {
        if (_webSocketCommandHandler == null) return;
        _webSocketCommandHandler.SendProfileRequestCommand(
            playerName =>
            {
                PlayerName = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
                Debug.Log("[Luxodd_Bridge] Player profile: " + PlayerName);
            },
            (code, msg) => Debug.LogWarning("[Luxodd_Bridge] Profile request failed " + code + ": " + msg));
    }

    public void FetchBalance()
    {
        if (_webSocketCommandHandler == null) return;
        _webSocketCommandHandler.SendUserBalanceRequestCommand(
            credits =>
            {
                CurrentBalance = credits;
                Debug.Log("[Luxodd_Bridge] Player balance: " + credits);
            },
            (code, msg) => Debug.LogWarning("[Luxodd_Bridge] Balance request failed " + code + ": " + msg));
    }

    // ---------------- Level analytics ----------------

    /// <summary>Call when the gameplay scene starts.</summary>
    public void SendLevelBegin()
    {
        if (_webSocketCommandHandler == null) return;
        _webSocketCommandHandler.SendLevelBeginRequestCommand(LevelNumber,
            () => Debug.Log("[Luxodd_Bridge] level_begin sent (level " + LevelNumber + ")"),
            (code, msg) => Debug.LogWarning("[Luxodd_Bridge] level_begin failed " + code + ": " + msg));
    }

    /// <summary>Call when the player runs out of hearts. The callback fires
    /// once the server has accepted the score.</summary>
    public void SendLevelEnd(int score, Action onSent = null)
    {
        if (_webSocketCommandHandler == null)
        {
            onSent?.Invoke();
            return;
        }
        _webSocketCommandHandler.SendLevelEndRequestCommand(LevelNumber, score,
            () =>
            {
                Debug.Log("[Luxodd_Bridge] level_end sent (score " + score + ")");
                onSent?.Invoke();
            },
            (code, msg) =>
            {
                Debug.LogWarning("[Luxodd_Bridge] level_end failed " + code + ": " + msg);
                onSent?.Invoke();
            });
    }

    // ---------------- Leaderboard ----------------

    public void FetchLeaderboard(Action<LeaderboardDataResponse> onSuccess)
    {
        if (_webSocketCommandHandler == null)
        {
            onSuccess?.Invoke(null);
            return;
        }
        _webSocketCommandHandler.SendLeaderboardRequestCommand(
            response =>
            {
                CachedLeaderboard = response;
                onSuccess?.Invoke(response);
            },
            (code, msg) =>
            {
                Debug.LogWarning("[Luxodd_Bridge] Leaderboard request failed " + code + ": " + msg);
                onSuccess?.Invoke(null);
            });
    }

    // ---------------- In-game transaction ----------------

    /// <summary>
    /// Schedules the Restart popup. Call right after game over — this method
    /// first sends level_end (if not already sent) and then waits
    /// RestartPopupDelay seconds (so the leaderboard has time to be visible)
    /// before asking Luxodd to show the Restart/End choice.
    /// </summary>
    public void TriggerRestartPopupAfterGameOver(int finalScore)
    {
        StartCoroutine(RestartPopupSequence(finalScore));
    }

    IEnumerator RestartPopupSequence(int finalScore)
    {
        // 1. Send level_end first — Restart popup must follow finalized session.
        bool levelEndDone = false;
        SendLevelEnd(finalScore, () => levelEndDone = true);

        // Wait for level_end OR a short timeout, whichever comes first.
        float waitedForEnd = 0f;
        while (!levelEndDone && waitedForEnd < 2f)
        {
            waitedForEnd += Time.unscaledDeltaTime;
            yield return null;
        }

        // 2. Hold on the leaderboard for the configured delay.
        yield return new WaitForSecondsRealtime(RestartPopupDelay);

        // 3. Ask the platform to show the Restart popup.
        if (_webSocketService == null)
        {
            Debug.LogWarning("[Luxodd_Bridge] WebSocketService missing — cannot show Restart popup.");
            yield break;
        }

        _webSocketService.SendSessionOptionRestart(action =>
        {
            Debug.Log("[Luxodd_Bridge] Restart popup result: " + action);
            if (action == SessionOptionAction.End)
            {
                _webSocketService.BackToSystem();
            }
            // If action == Restart, the platform creates a new session and
            // reloads the game automatically — we don't have to do anything.
        });
    }

    /// <summary>
    /// Optional alternative: show a Continue popup instead (e.g. when the
    /// player loses their last heart and you want to offer a paid restore).
    /// </summary>
    public void ShowContinuePopup(Action onContinue, Action onEnd)
    {
        if (_webSocketService == null) { onEnd?.Invoke(); return; }
        _webSocketService.SendSessionOptionContinue(action =>
        {
            if (action == SessionOptionAction.Continue) onContinue?.Invoke();
            else onEnd?.Invoke();
        });
    }
}
