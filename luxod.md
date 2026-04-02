# Luxodd Arcade Platform — Complete Developer Guide
> From Unity project to live arcade cabinet — everything you need, in order.

**Docs source:** https://docs.luxodd.com | **GitHub:** https://github.com/luxodd/arcade-documentation

---

## Table of Contents
1. [Platform Overview](#1-platform-overview)
2. [System Architecture](#2-system-architecture)
3. [Hardware Specifications](#3-hardware-specifications)
4. [Controls & Input Mapping](#4-controls--input-mapping)
5. [Game Technical Requirements](#5-game-technical-requirements)
6. [Unity Plugin — Installation](#6-unity-plugin--installation)
7. [Unity Plugin — Configuration](#7-unity-plugin--configuration)
8. [Unity Plugin — Testing in Editor](#8-unity-plugin--testing-in-editor)
9. [Unity Plugin — Integration Guide](#9-unity-plugin--integration-guide)
10. [In-Game Transactions (Continue / Restart)](#10-in-game-transactions-continue--restart)
11. [WebSocket API Reference](#11-websocket-api-reference)
12. [Building for WebGL (Browser)](#12-building-for-webgl-browser)
13. [Onboarding Process (6 Steps)](#13-onboarding-process-6-steps)
14. [Game Submission Checklist](#14-game-submission-checklist)
15. [Revenue & Payouts](#15-revenue--payouts)
16. [Going Live](#16-going-live)
17. [Performance & Export Tips](#17-performance--export-tips)
18. [FAQ & Support](#18-faq--support)

---

## 1. Platform Overview

Luxodd is a **skill-based arcade platform** where players wager on their own performance — not chance ("Strategic Betting™"). Games run as **Unity WebGL builds** served from Luxodd's cloud, loaded inside an iframe on physical arcade cabinets in bars, arcades, and entertainment venues.

**Key facts for developers:**
- Games run as **Unity WebGL** in Chrome on Windows 11
- Screen is **portrait 9:16** (1080×1920 target)
- Input is **joystick + 6 colored buttons** — no touch, no mouse
- Backend communication is via **WebSocket** (`wss://app.luxodd.com/ws`)
- Players pay per session via built-in POS terminal
- You earn **10% of every session** played on your game
- Plugin version shown in example scene: **1.0.8**

---

## 2. System Architecture

### Actors

| Actor | Role |
|-------|------|
| **Game Developer** | Manages games via Admin Portal, tests API via Postman, uploads WebGL builds |
| **Local Player** | Physically at the arcade, pays via credit card or registered account, plays games |
| **Online User** | Manages account (funds, profile, PIN) via browser — no online play yet |
| **Merchant** | Owns/hosts an arcade, selects up to 5 games per cabinet, sets prices |

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    ADMIN PORTAL (Admin Functions)               │
│         Upload games · Issue Dev API Keys · Merchant Mgmt       │
└───────────────────────────┬─────────────────────────────────────┘
                            │ Uploads Game Assets (WebGL .zip)
                            ▼
                   Google Cloud Storage
                   (WebGL static files)
                            │
┌───────────────────────────▼─────────────────────────────────────┐
│                  GAME SERVER (Game Server Functions)             │
│                                                                  │
│  ┌─────────────────────┐   ┌──────────────────────────────────┐ │
│  │   Server Front-End  │   │      Socket Gateway API          │ │
│  │   (IFRAME host)     │   │   wss://app.luxodd.com/ws        │ │
│  └─────────────────────┘   └──────────────────────────────────┘ │
│                                                                  │
│  • IFRAME between WebGL game and arcade system                   │
│  • SSE + Pub/Sub for hardware control (monitor, printer, POS)    │
│  • Session key validation & scoping                              │
└───────────────────────────┬─────────────────────────────────────┘
                            │
              ┌─────────────┴────────────┐
              ▼                          ▼
   ┌──────────────────────┐   ┌─────────────────────┐
   │  Game Server API     │   │   Arcade Cabinet     │
   │  · User Accounts     │   │  (Rook or Pawn)      │
   │  · Transactions      │   │  Windows 11 + Chrome │
   │  · Leaderboard       │   │  9:16 portrait       │
   │  · User Data/State   │   │  Joystick + 6 btns   │
   └──────────┬───────────┘   └─────────────────────┘
              ▼
         Data Store
```

### Game Launch Sequence

```
1. Player selects game on arcade → Client App requests game access
2. Server validates user auth → requests game assets from GCS
3. Server generates session key → stores in DB
4. Server returns game URL + session key to Client App
5. Client App loads your WebGL game in iframe (URL has ?token=KEY)
6. Your game reads token from URL → establishes WebSocket connection
7. Server validates session key → confirms connection
8. ── GAME SESSION ── Real-time WebSocket communication until end
9. Game calls BackToSystem() → session ends, player returns to menu
```

### Session Token Security Model
- Scoped to: **one player + one game + one session**
- Only grants access to that player's data for the duration of play
- Never exposes other users' data or global system info
- Your **Dev API token** = a never-expiring version — use it for testing

---

## 3. Hardware Specifications

Both cabinet types share identical hardware and software. One WebGL build serves both.

### Rook (Large — Public Venues & Tournaments)

| Spec | Value |
|------|-------|
| CPU | AMD Ryzen 7 5800H |
| Clock Speed | 4.4 GHz |
| RAM | 16 GB |
| Storage | 500 GB PCIe 3.0 SSD |
| Network | WiFi 6 |
| Screen | 55" TV |
| Resolution | 4K / 2K / Full HD |
| **Aspect Ratio** | **9:16 (portrait)** |
| OS | Windows 11 |
| Browser | Chrome |
| Controls | Joystick + 6 colored buttons + numeric keypad |
| Extras | POS Terminal + Printer, dual-screen |

### Pawn (Small — Home / Compact Venues)

| Spec | Value |
|------|-------|
| CPU | AMD Ryzen 7 5800H |
| Clock Speed | 4.4 GHz |
| RAM | 16 GB |
| Storage | 500 GB PCIe 3.0 SSD |
| Network | WiFi 6 |
| Screen | 24" Monitor |
| Resolution | 4K / 2K / Full HD |
| **Aspect Ratio** | **9:16 (portrait)** |
| OS | Windows 11 |
| Browser | Chrome |
| Controls | Joystick + 6 colored buttons + numeric keypad |
| Extras | POS Terminal + Printer |

### Key Design Targets
- Canvas: **1080 × 1920** (portrait)
- Frame rate: **60 FPS**
- Memory: stay well under **2048 MB**
- Load time: under **1 minute** (target < 10 seconds)
- Build size: **≤ 100 MB**

---

## 4. Controls & Input Mapping

### Physical Control Panel

![Luxodd Arcade Control Panel](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/static/img/arcade-controls.png)

The cabinet has:
- **Joystick** — 8-way directional, reads as `Vector2` in Unity
- **8 colored buttons** — some reserved by the system
- **Numeric keypad** — for PIN entry only, NOT for gameplay

### Button Color → Unity Input Mapping

| Physical Button | Color | Unity Input | Notes |
|----------------|-------|-------------|-------|
| Black | ⚫ | `JoystickButton0` | Default confirm |
| Red | 🔴 | `JoystickButton1` | Gameplay action |
| Green | 🟢 | `JoystickButton2` | Gameplay action |
| Yellow | 🟡 | `JoystickButton3` | Gameplay action |
| Blue | 🔵 | `JoystickButton4` | Gameplay action |
| Purple | 🟣 | `JoystickButton5` | Gameplay action |
| **Orange** | 🟠 | `JoystickButton8` | **RESERVED — system overlay/help** |
| **White** | ⚪ | `JoystickButton9` | **RESERVED — back/cancel** |

> **Do NOT use Orange or White buttons for gameplay.** They are reserved by the system.

> **Numeric keypad is for PIN input only** — do not bind gameplay actions to it.

### ArcadeControls API (Official SDK)

```csharp
// Read joystick direction
StickData stick = ArcadeControls.GetStick();
Vector2 direction = stick.Vector;  // -1 to 1 on both axes

// Button pressed this frame (one-shot — good for jump, confirm, use item)
bool ArcadeControls.GetButtonDown(ArcadeButtonColor button);

// Button released this frame
bool ArcadeControls.GetButtonUp(ArcadeButtonColor button);

// Button color enum
public enum ArcadeButtonColor
{
    Black,    // JoystickButton0
    Red,      // JoystickButton1
    Green,    // JoystickButton2
    Yellow,   // JoystickButton3
    Blue,     // JoystickButton4
    Purple,   // JoystickButton5
    Orange,   // JoystickButton8 — RESERVED
    White     // JoystickButton9 — RESERVED
}
```

### Recommended Architecture: Adapter Pattern

Do **not** call `ArcadeControls` directly in gameplay code. Use an adapter interface:

```csharp
// Define game actions, not hardware buttons
public interface IPlayerControlAdapter
{
    Vector2 MovementVector { get; }
    event Action JumpButtonPressed;
    event Action UseItemButtonPressed;
    bool IsFireButtonPressed { get; }
}

// Arcade implementation of the adapter
public class ArcadePlayerControlAdapter : MonoBehaviour, IPlayerControlAdapter
{
    public Vector2 MovementVector { get; private set; }
    public event Action JumpButtonPressed;
    public event Action UseItemButtonPressed;
    public bool IsFireButtonPressed { get; private set; }

    void Update()
    {
        // Read joystick
        var stickData = ArcadeControls.GetStick();
        MovementVector = stickData.Vector;

        // Fire button — hold state
        if (ArcadeControls.GetButtonDown(ArcadeButtonColor.Red))
            IsFireButtonPressed = true;
        else if (ArcadeControls.GetButtonUp(ArcadeButtonColor.Red))
            IsFireButtonPressed = false;

        // Jump — one-shot event
        if (ArcadeControls.GetButtonDown(ArcadeButtonColor.Black))
            JumpButtonPressed?.Invoke();

        // Use item — one-shot event
        if (ArcadeControls.GetButtonDown(ArcadeButtonColor.Green))
            UseItemButtonPressed?.Invoke();
    }
}
```

### Rules for Input Code

| Rule | Reason |
|------|--------|
| Read input in `Update()` | Not `FixedUpdate()` — you'll miss frames |
| Apply physics in `FixedUpdate()` | Consistent physics steps |
| Use `GetButtonDown` for one-shot actions | `GetButton` would fire every frame |
| Fire rate must be **time-based** | Not frame-based — different machines have different FPS |
| Gameplay code must NOT import `ArcadeControls` | Use adapter interface only |

### Arcade Bindings Editor Tool

After installing the plugin, open:
**Luxodd Unity Plugin → Control → Binding Editor**

This shows a visual map of the control panel, lets you assign Action Labels (e.g. "Jump", "Shoot") for overlays, and previews joystick direction in Play Mode. It is a **reference/debug tool only** — it does not create input bindings automatically.

---

## 5. Game Technical Requirements

### Unity Build Settings

| Setting | Value |
|---------|-------|
| Platform | **WebGL** |
| WebGL Template | **LuxoddTemplate** (included in plugin) |
| Canvas Width | **1080** |
| Canvas Height | **1920** |
| Run In Background | **✓ Enabled** |
| Compression Format | **Gzip** |
| Name Files as Hashes | ✓ |
| Data Caching | ✓ |
| `Application.targetFrameRate` | **-1** (let browser control — best WebGL performance) |

### Build Output Structure (required)

Your zip **must have `index.html` in the root:**

```
YourGame.zip
├── index.html          ← REQUIRED in root
├── Build/
│   ├── YourGame.data
│   ├── YourGame.framework.js
│   ├── YourGame.loader.js
│   └── YourGame.wasm
├── TemplateData/
└── StreamingAssets/    (if used)
```

---

## 6. Unity Plugin — Installation

**Plugin download:** https://github.com/luxodd/unity-plugin/releases

### Step 1 — Download

Download the latest `.unitypackage` from the releases page above.

### Step 2 — Import into Unity

1. Open your Unity project
2. **Assets → Import Package → Custom Package...**

![Import Package Menu](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image2.png)

3. Select the downloaded `.unitypackage` → click **Open**
4. Review contents → click **Import**

![Import Unity Package Window](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image3.png)

The package includes:
```
Luxodd.Game/
├── Editor/          (NewtonsoftInstaller.cs)
├── Example/
│   ├── Fonts/       (Eurostile font family)
│   ├── Scenes/      (ExampleScene.unity)
│   └── Scripts/     (ExampleStartBehaviour, ViewHandlers...)
├── Plugins/
├── Prefabs/
├── Resources/       (NetworkSettingsDescriptor)
├── Scripts/
├── Sprites/
└── TextMesh Pro/
```

### Step 3 — Install Dependencies

If prompted, install **Newtonsoft.Json**:

![Newtonsoft.Json Prompt](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image9.png)

If not prompted, it's already installed:

![Newtonsoft.Json Already Installed](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image7.png)

---

## 7. Unity Plugin — Configuration

### Set Your Developer Token

**Tools → Luxodd Plugin → Set Developer Token**

![Developer Token Menu](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image4.png)

Paste the token from your Admin Portal. This enables Debug Mode so editor tests connect to the real backend.

### Server Environments

Edit: `Assets/Luxodd.Game/Resources/NetworkSettingsDescriptor`

![Network Settings Descriptor](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image1.png)

| Field | Value |
|-------|-------|
| Server Address | `wss://app.luxodd.com/ws` |
| Developer Debug Token | Your dev token |

Two environments available:
- **Staging** (default) — for development and testing
- **Production** — for live arcade deployment

---

## 8. Unity Plugin — Testing in Editor

### Open the Example Scene

`Assets → Luxodd.Game → Example → Scenes → ExampleScene`

![Example Scene](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image22.png)

### Main Command Panel

![Connect to Server](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image12.png)
![Get User Profile](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image6.png)
![Toggle Health Check](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image8.png)
![Add Credits](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image5.png)
![Charge Credits](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image11.png)

| Button | What it does |
|--------|-------------|
| **Connect to Server** | Initiates WebSocket connection |
| **Get User Profile** | Fetches player name and balance |
| **Toggle Health Check** | Sends `health_status_check` every 2 seconds |
| **Add Credits Request** | Adds 5 credits (requires PIN) |
| **Charge Credits Request** | Deducts 3 credits (requires PIN) |
| **Storage Commands** | Opens User State Test Panel |
| **Controlling Test** | Opens local joystick/button input test scene |

### User State Test Panel

![User State Panel](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image17.png)

| Button | What it does |
|--------|-------------|
| **Get User State** | Fetches saved state from server |
| **Set User State** | Saves current state to server |
| **Clear User State** | Sends `null` — clears all saved state |

### Controlling Test Scene

Tests joystick direction and button highlighting — fully local, no server connection.

- Move a 2D square with the joystick
- On-screen buttons highlight when physical buttons are pressed
- Use this to verify your input mappings before writing gameplay code

---

## 9. Unity Plugin — Integration Guide

### Step 1 — Add the Network Prefab

Drag to your scene: `Assets/Luxodd.Game/Prefabs/UnityPluginPrefab`

![Prefab in Scene](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image21.png)

This prefab contains all services: `WebSocketService`, `WebSocketCommandHandler`, `HealthStatusCheckService`, `ReconnectService`, etc.

Add this using statement to all your integration scripts:
```csharp
using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.CommandHandler;
```

### Step 2 — Connect to Server

```csharp
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private WebSocketService _webSocketService;

    void Start()
    {
        _webSocketService.ConnectToServer(
            onSuccessCallback: () => Debug.Log("Connected!"),
            onErrorCallback:   () => Debug.LogError("Connection failed!")
        );
    }
}
```

### Step 3 — Enable Health Check (Required)

The server expects a health check every **5 seconds**. If 3 checks are missed, the session is automatically ended and the player is redirected to the game list.

```csharp
[SerializeField] private HealthStatusCheckService _healthStatusCheckService;

// Call after successful connection:
_healthStatusCheckService.Activate();

// To stop (for testing server timeout behavior only):
_healthStatusCheckService.Deactivate();
```

> You can change the interval in the prefab Inspector (default: 2 seconds).

![Health Check Interval Setting](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image20.png)

### Step 4 — Get Player Profile

```csharp
[SerializeField] private WebSocketCommandHandler _webSocketCommandHandler;

_webSocketCommandHandler.SendProfileRequestCommand(
    onSuccessCallback: (playerName) => {
        Debug.Log($"Player: {playerName}");
        // Show welcome message in your game UI
    },
    onFailureCallback: (code, message) => Debug.LogError($"Profile failed: {code} {message}")
);
```

### Step 5 — Get Player Balance

```csharp
_webSocketCommandHandler.SendUserBalanceRequestCommand(
    onSuccessCallback: (credits) => {
        Debug.Log($"Credits: {credits}");
        // Display balance in HUD
    },
    onFailureCallback: (code, message) => Debug.LogError($"Balance failed: {code} {message}")
);
```

### Step 6 — Track Level Events (Required for Leaderboard)

**At the start of each level/run:**
```csharp
_webSocketCommandHandler.SendLevelBeginRequestCommand(
    level: currentLevel,
    onSuccessCallback: () => Debug.Log("Level begin tracked"),
    onFailureCallback: (code, msg) => Debug.LogError($"Level begin failed: {code} {msg}")
);
```

**At the end of each level/run (or game over):**
```csharp
_webSocketCommandHandler.SendLevelEndRequestCommand(
    level: currentLevel,
    score: finalScore,
    onSuccessCallback: () => Debug.Log("Level end tracked"),
    onFailureCallback: (code, msg) => Debug.LogError($"Level end failed: {code} {msg}")
);
```

> Sending `SendLevelEndRequestCommand` is what populates the leaderboard. Do not skip it.

### Step 7 — Credit Operations (Optional)

**Add credits (top-up):**
```csharp
// Show a PIN input UI first, then:
_webSocketCommandHandler.SendAddBalanceRequestCommand(
    amount: 5,
    pinCode: playerEnteredPin,
    onSuccess: () => Debug.Log("Credits added"),
    onFailureCallback: (code, msg) => {
        // code 412 = wrong PIN
        // code 402 = insufficient balance
        // code 500 = other error
        Debug.LogError($"Add credits failed: {code} {msg}");
    }
);
```

**Charge credits (deduct for continue/extra lives):**
```csharp
_webSocketCommandHandler.SendChargeUserBalanceRequestCommand(
    amount: 3,
    pinCode: playerEnteredPin,
    onSuccess: () => Debug.Log("Credits deducted"),
    onFailureCallback: (code, msg) => Debug.LogError($"Charge failed: {code} {msg}")
);
```

> PIN is automatically hashed by the plugin. Always mask the PIN input field. Only use the off-screen arcade keypad for PIN entry.

### Step 8 — Leaderboard

Requires `SendLevelEndRequestCommand` to have been called with a score first.

```csharp
using Luxodd.Game.Scripts.Game.Leaderboard;

_webSocketCommandHandler.SendLeaderboardRequestCommand(
    onSuccessCallback: (response) => {
        Debug.Log($"Your rank: {response.CurrentUserData.Rank}");
        Debug.Log($"Your score: {response.CurrentUserData.TotalScore}");
        foreach (var entry in response.Leaderboard)
            Debug.Log($"#{entry.Rank} {entry.PlayerName}: {entry.TotalScore}");
    },
    onFailureCallback: (code, msg) => Debug.LogError($"Leaderboard failed: {code} {msg}")
);
```

`LeaderboardDataResponse` fields:
- `CurrentUserData.Rank` — player's current position
- `CurrentUserData.TotalScore` — player's highest score
- `CurrentUserData.PlayerName` — player's game handle
- `Leaderboard` — `List<LeaderboardData>` of other players

> **Requirement:** Your game **must** have a leaderboard and use the Leaderboard Plugin API.

### Step 9 — User State / Save Data (Optional)

Store per-player persistent data (level progress, skin selection, settings, etc.):

```csharp
// Get saved state
_webSocketCommandHandler.SendGetUserDataRequestCommand(
    onSuccessCallback: (data) => {
        if (data == null) { /* first run, no saved state */ return; }
        // Deserialize: var state = JsonConvert.DeserializeObject<PlayerState>(data.ToString());
    },
    onFailureCallback: (code, msg) => Debug.LogError($"Get state failed: {code} {msg}")
);

// Save state
public class PlayerState
{
    public int CurrentLevel { get; set; }
    public int SkinId { get; set; }
}

var state = new PlayerState { CurrentLevel = 3, SkinId = 2 };
string json = JsonConvert.SerializeObject(state);

_webSocketCommandHandler.SendSetUserDataRequestCommand(
    userData: json,
    onSuccessCallback: () => Debug.Log("State saved"),
    onFailureCallback: (code, msg) => Debug.LogError($"Save state failed: {code} {msg}")
);
```

### Step 10 — End Session and Return to System

**Always call this when the game ends** — never let the game just sit idle.

```csharp
// Normal game end
_webSocketService.BackToSystem();

// Error/crash recovery
_webSocketService.BackToSystemWithError("Game crashed", errorMessage);
```

### Joystick Navigation for UI Menus

All menus must be navigable with joystick only (no mouse). The plugin includes a `VirtualKeyboardNavigator`:

```csharp
// 1. Add KeyButtonItem to each button in your menu
// 2. Add VirtualKeyboardNavigator to the menu root, list items top-to-bottom
// 3. On menu open:
navigator.Activate();
// 4. On menu close:
navigator.Deactivate();
// 5. If a child window opens, freeze the parent:
parentNavigator.SetFocus(false);
```

---

## 10. In-Game Transactions (Continue / Restart)

### Overview

Two separate transaction flows exist for when a player runs out of lives/time:

| Flow | Session state | Who handles it |
|------|--------------|----------------|
| **Continue** | Session stays active | Game |
| **Restart** | Session must be finalized first, then system creates a new one | System |

**Never unify them** — they have different lifecycles. Doing so causes session data loss.

### Continue Flow

Use when: player can resume the same run (restore lives, add time, respawn at checkpoint).

![Continue Popup](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image28.png)

```csharp
[SerializeField] private WebSocketService _webSocketService;
[SerializeField] private WebSocketCommandHandler _webSocketCommandHandler;

// When game over and continue is allowed:
void ShowContinuePopup()
{
    _webSocketService.SendSessionOptionContinue(OnContinueResult);
}

void OnContinueResult(SessionOptionAction action)
{
    switch (action)
    {
        case SessionOptionAction.Continue:
            // Player paid → restore lives/HP/time and resume
            ResumeGameplay();
            break;
        case SessionOptionAction.End:
            // Player declined → finalize and exit
            EndSession();
            break;
    }
}
```

### Restart Flow

Use when: player wants to start a new run from scratch.

![Restart Popup](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image27.png)

```csharp
// IMPORTANT: send session results BEFORE showing restart popup
void ShowRestartPopup()
{
    _webSocketCommandHandler.SendLevelEndRequestCommand(level, score, () =>
    {
        // Only show restart popup after results are safely sent
        _webSocketService.SendSessionOptionRestart(OnRestartResult);
    }, OnError);
}

void OnRestartResult(SessionOptionAction action)
{
    // NOTE: if player picks Restart, the system creates the new session
    // automatically — you will NOT receive a Restart callback.
    // You only receive End:
    if (action == SessionOptionAction.End)
        _webSocketService.BackToSystem();
}
```

### Responsibility Matrix

| Action | Game | System |
|--------|------|--------|
| Pause gameplay | ✅ | |
| Resume gameplay (Continue) | ✅ | |
| Restore lives/time/HP | ✅ | |
| Show Continue popup | ✅ | |
| Show Restart popup | ✅ | |
| Send session results | ✅ | |
| Create new session (Restart) | | ✅ |
| Balance & credit checks | | ✅ |
| Top-up flow | | ✅ |

### Admin Panel Configuration

![Admin Panel Config](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image25.png)

- **Continue Price > 0** — enables Continue popup
- **Restart Price > 0** — enables Restart popup
- **Price = 0** — disables that popup (only End button shown)

![Zero Price Result](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image26.png)

---

## 11. WebSocket API Reference

> **Postman Collection:** https://www.postman.com/luxodd-team/luxodd-public-game-dev/collection/681886238c4b51f645ce9787/game-dev-collection

Server URL: `wss://app.luxodd.com/ws`
Token passed via URL: `/index.html?token=YOUR_DEV_TOKEN`

### Get Player Profile

**Request:**
```json
{ "type": "GetProfileRequest" }
```
**Response:**
```json
{
  "msgver": "1", "type": "GetProfileResponse", "status": 200,
  "payload": {
    "game_handle": "user_nickname",
    "name": "User Name",
    "email": "user@gmail.com",
    "profile_picture": "https://link_to_photo"
  }
}
```
> In-game only `game_handle` is typically used.

---

### Get User Balance

**Request:**
```json
{ "type": "GetUserBalanceRequest" }
```
**Response:**
```json
{ "msgver": "1", "type": "GetUserBalanceResponse", "status": 200,
  "payload": { "balance": 336 } }
```

---

### Add Balance

**Request:**
```json
{ "type": "AddBalanceRequest", "payload": { "amount": 5, "pin": "WUFLXVU=" } }
```
**Response:**
```json
{ "msgver": "1", "type": "AddBalanceResponse", "status": 200, "payload": null }
```

---

### Charge Balance

**Request:**
```json
{ "type": "ChargeUserBalanceRequest", "payload": { "amount": 7, "pin": "WUFLXVU=" } }
```
**Response:**
```json
{ "msgver": "1", "type": "ChargeUserBalanceResponse", "status": 200, "payload": null }
```

**Error codes:**
- `412` — incorrect PIN
- `402` — insufficient balance
- `500` — other error

---

### Leaderboard

**Request:**
```json
{ "type": "leaderboard_request", "version": "1.0", "payload": {} }
```
**Response:**
```json
{
  "type": "leaderboard_response", "status": 200,
  "payload": {
    "current_user": { "game_handle": "you", "rank": 1, "score_total": 9901 },
    "leaderboard": [
      { "rank": 2, "game_handle": "max008", "score_total": 9731 },
      { "rank": 3, "game_handle": "test-1", "score_total": 7494 }
    ]
  }
}
```

---

### Level Begin

**Request:**
```json
{ "type": "level_begin", "version": "1.0", "payload": { "level": 1 } }
```
**Response:**
```json
{ "type": "level_begin_response", "status": 200, "payload": null }
```

---

### Level End

**Request:**
```json
{
  "type": "level_end", "version": "1.0",
  "payload": {
    "score": 2376, "level": 1, "status": "completed",
    "time_taken": 218, "coins_collected": 50,
    "powerups_used": 2, "deaths": 3, "completion_percentage": 100
  }
}
```
**Response:**
```json
{ "type": "level_end_response", "status": 200, "payload": null }
```

---

### Health Status Check

**Request:**
```json
{ "type": "health_status_check", "version": "1.0", "payload": {} }
```
**Response:**
```json
{
  "type": "health_status_check_response", "status": 200,
  "payload": { "status": "ok", "timestamp": "2025-05-01T16:52:43Z" }
}
```
> Must be sent every 5 seconds. 3 missed = session auto-terminated.

---

### Get / Set User Data

**Get:**
```json
{ "type": "GetUserDataRequest", "version": "1.0", "payload": {} }
```
**Set:**
```json
{
  "type": "SetUserDataRequest",
  "payload": { "user_data": { "current_level": 1, "level_states": [2] } }
}
```

---

## 12. Building for WebGL (Browser)

### Setup Steps

1. **File → Build Profiles** (Unity 6) or **File → Build Settings**
2. Confirm **Web / WebGL** is the active platform

![Build Profiles Window](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image16.png)

3. **Player Settings → Settings for Web → Resolution and Presentation:**
   - Default Canvas Width: `1080`
   - Default Canvas Height: `1920`
   - Run in Background: `✓`
   - WebGL Template: **LuxoddTemplate**

![Player Settings Resolution](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image13.png)

4. **Player Settings → Publishing Settings:**
   - Compression Format: **Gzip**
   - Name Files as Hashes: `✓`
   - Data Caching: `✓`
   - Debug Symbols: **Off**

![Publishing Settings](https://raw.githubusercontent.com/luxodd/arcade-documentation/main/docs/arcade-launch/unity-plugin/assets/image15.png)

5. Click **Build and Run** — the game opens in your browser.

### Test Plugin in Browser

After the game loads, append your dev token to the URL:

```
http://localhost:8002/index.html?token=YOUR_DEV_TOKEN
```

This enables full server communication, including WebSocket, session data, credits, and leaderboard.

### What the LuxoddTemplate Does

The `LuxoddTemplate` is included with the plugin. It:
- Sets correct viewport for 9:16 portrait
- Handles the `?token=` URL query string for session key injection
- Provides the iframe communication layer
- Configures `NotifySessionEnd()` JS call for `BackToSystem()`

---

## 13. Onboarding Process (6 Steps)

### Step 1 — Create a Luxodd Player Account

https://luxodd.com — register as a player first (required).

### Step 2 — Apply as a Developer

Fill out the form at https://luxodd.com/developer

- Join the **Discord server** — being known to the team speeds up approval
- Approval is quick if you have an existing game or prior contact
- Once approved: your player account gains access to the **Admin Portal**

### Step 3 — Register Your Game (Draft State)

1. Log into https://app.luxodd.com
2. **Games → New**
3. Fill in required fields:
   - **Game Title**
   - **Game Description** — shown on the game card in the Luxodd Launcher
   - **Game Icon** — 3:4 portrait ratio, minimum 600×800 px
4. Click **Save** → game is in **Draft** state

### Step 4 — Get Dev Token & Integrate Plugin

1. Admin Portal → your game → **Generate Developer Token**
2. Download plugin from https://github.com/luxodd/unity-plugin/releases
3. Follow [Installation](#6-unity-plugin--installation) and [Integration](#9-unity-plugin--integration-guide) sections
4. Test with the Postman Collection: https://www.postman.com/luxodd-team/luxodd-public-game-dev

### Step 5 — Upload & Complete Details (Draft → Review)

1. **Games → List → Edit** your game
2. **Game File:** Upload WebGL zip (`index.html` in root, ≤ 100 MB)
3. **Image URL:** Game selection image (minimum 800×600)
4. **Video ID:** YouTube gameplay video ID (optional)
5. **Price:** Session price in USD
6. Test at https://app.luxodd.com/selectGame (all transactions are mocked in draft state)
7. Complete all checklist items → **Request Review**

### Step 6 — Review → Approved → Live

- Review team: **1–3 business days**
- **Approved** → email notification → game enters Merchant Catalog
- **Rejected** → email with feedback → fix and resubmit via **Update Game**
- **First merchant selects your game** = officially **Live** state
- Monthly revenue reports sent to your email

---

## 14. Game Submission Checklist

### 1. General Requirements

| # | Requirement | Detail |
|---|-------------|--------|
| 1 | **Build Format** | Unity WebGL only |
| 2 | **Resolution** | 1080×1920 portrait (9:16) |
| 3 | **File Size** | ≤ 100 MB total |
| 4 | **Version Tag** | Version number in build metadata |
| 5 | **Platform Cleanup** | Remove ads, vibration, mobile-specific scoring, non-Luxodd currencies |

### 2. Session & Security

| # | Requirement | Detail |
|---|-------------|--------|
| 1 | **Session Token** | Read from `?token=` URL param, pass to WebSocket |
| 2 | **Health Check** | Every 5 seconds — 3 missed = auto session end |
| 3 | **Data Egress** | All external URLs must be declared and approved by Luxodd (email `admin@luxodd.com`) |
| 4 | **PIN Handling** | Auto-hashed by plugin, input must be masked, only off-screen keypad |
| 5 | **Leaderboard** | Required — must use Leaderboard Plugin API |
| 6 | **Reconnection** | Must auto-reconnect on timeout/disconnect |

> WebSocket connection must stay alive for minimum **5 minutes** per session.

### 3. Game Flow & UX

| # | Requirement | Detail |
|---|-------------|--------|
| 1 | **Game End Callback** | Must call `WebSocketService.BackToSystem()` |
| 2 | **State Restoration** | Must load and resume from saved user state if used |
| 3 | **Joystick Navigation** | ALL menus AND gameplay navigable via joystick only |
| 4 | **Menu Timeout** | All menus auto-return to arcade menu after max **30 seconds** of inactivity |
| 5 | **Start Timeout** | Max **1 minute** wait for input before force-starting |
| 6 | **Functional UI** | Every button must have a clear, working purpose |
| 7 | **Non-Cumulative Scoring** | Score is per-session, not cumulative across sessions |
| 8 | **Finite Play** | Session must end in ≤ **10 minutes** (lives, time limit, or level completion) |
| 9 | **No Pausing** | No pause mechanic allowed |
| 10 | **Score Visibility** | Player must see their score change during gameplay |

### 4. Quality Assurance

| # | Requirement | Detail |
|---|-------------|--------|
| 1 | **Load Time** | Must load within 1 minute on target hardware |
| 2 | **Crash Handling** | Must fail gracefully and return control to arcade launcher |
| 3 | **Asset Optimization** | Compressed textures, optimized audio, minimal shaders |
| 4 | **Debug Logs** | Disabled in production build — console clean unless error |
| 5 | **No Critical Bugs** | No crashes or game-breaking bugs |
| 6 | **QA Process** | Thorough testing for stability and absence of obvious glitches |
| 7 | **Sound** | Arcade-quality sound effects; compressed music for load speed |

---

## 15. Revenue & Payouts

- **Your cut: 10%** of every session played on your game
- Luxodd handles: payment processing, POS, merchant relationships, refunds
- **Payment setup:** After approval, Luxodd emails you a form to fill in your payment method
- **Reports:** Monthly, sent to your registered email
- **Payouts:** Monthly, via your registered payment method
- **Pricing:** You set the session price in USD (merchants may adjust within limits)

---

## 16. Going Live

1. Game is approved → enters **Merchant Catalog**
2. Merchants browse catalog → select games for their specific arcades
3. First deployment by any merchant → **Live** status
4. Real players, real money, real metrics in Admin Portal

### Update Your Game (Post-Launch)

Self-service update is not yet available in the Admin Portal.

**Current process:**
1. Build updated WebGL zip
2. Test on sandbox
3. Email `admin@luxodd.com` OR contact via Discord
4. Luxodd team deploys the update

---

## 17. Performance & Export Tips

### Memory

- Keep memory usage well under **2048 MB** — browser may fail to resize the heap
- Use [Memory Profiler package](https://docs.unity3d.com/Packages/com.unity.memoryprofiler@1.0/manual/index.html) to find unused textures, meshes, audio clips
- Background audio: set Load Type to **Compressed In Memory**
- `Application.targetFrameRate` must be **-1** on WebGL (let browser control)

### Garbage Collection

WebGL runs GC at end of every frame — minimize allocations:

```csharp
// BAD — allocates every frame
for (int i = 0; i < getArray().Length; i++) { }

// GOOD — cache it
var arr = getArray();
for (int i = 0; i < arr.Length; i++) { }

// BAD — string concatenation in loops
result += someString;

// GOOD — use StringBuilder if > 10 concatenations

// GOOD — no allocation
gameObject.CompareTag("Ball");  // not gameObject.tag == "Ball"

// GOOD — cache coroutine yields
var delay = new WaitForSeconds(0.5f);  // create once, reuse
```

### WebGL-Specific Rules

- **No `Thread.Sleep`** — use coroutines
- **No native plugins** unless they support WebGL
- **No file I/O** — use `PlayerPrefs` or Luxodd User State API
- **Test in Chrome** — production cabinet uses Chrome
- Joystick axes map via **Input Manager** — verify they match ArcadeControls

### Build Size

- Strip Engine Code: `✓`
- Use texture compression (DXT/ETC)
- Avoid uncompressed audio in streaming assets
- Target ≤ 100 MB compressed

---

## 18. FAQ & Support

**Q: How do I update my game after going live?**
Email `admin@luxodd.com` or Discord. Self-service update coming soon.

**Q: Can I test without spending real money?**
Yes — sandbox at `app.luxodd.com/selectGame` mocks all transactions while in Draft/Review state.

**Q: Do I need to support both Rook and Pawn?**
One build covers both — same hardware, same 9:16 aspect ratio.

**Q: Can my game use external services (analytics, CDN, etc.)?**
Yes, but all external URLs must be declared and approved by Luxodd before submission.

**Q: What happens if the player loses connection mid-game?**
`ReconnectService` (included in the prefab) handles automatic reconnection. Your game must also handle the reconnected state gracefully.

**Q: What engine is supported?**
Unity (WebGL) is the official supported engine with the dedicated plugin.

**Q: Is online play available?**
Not yet. Players must be physically at an arcade cabinet. Online play is on the roadmap.

**Q: My game is more than 10 minutes — what do I do?**
Add a finite mechanism: lives, time boxes, or make level completion achievable in ≤ 10 minutes. After the limit, offer Continue (credits) or redirect to level select.

**Q: Can players pause the game?**
No — pausing is not allowed per submission requirements.

### Key Links

| Resource | URL |
|----------|-----|
| Luxodd Main Site | https://luxodd.com |
| Developer Apply | https://luxodd.com/developer |
| Admin Portal | https://app.luxodd.com |
| Developer Docs | https://docs.luxodd.com |
| GitHub Docs | https://github.com/luxodd/arcade-documentation |
| Plugin Releases | https://github.com/luxodd/unity-plugin/releases |
| Example Game | https://github.com/luxodd/example-game-arcade-shooter |
| Postman API | https://www.postman.com/luxodd-team/luxodd-public-game-dev |
| Sandbox Test | https://app.luxodd.com/selectGame |
| Support Email | admin@luxodd.com |

---

*Last updated: February 2026 | Luxodd Platform v1.x | Plugin v1.0.8 | Unity WebGL*
