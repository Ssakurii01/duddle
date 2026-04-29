using System;
using System.Collections.Generic;
using System.Linq;
using Luxodd.Game.HelpersAndUtils.Utils;
using Luxodd.Game.Scripts;
using Luxodd.Game.Scripts.HelpersAndUtils;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using Luxodd.Game.Scripts.Input;
using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.CommandHandler;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif
using UnityEngine;

namespace Luxodd.Game.Example.Scripts
{
    public class ExampleStartBehaviour : MonoBehaviour
    {
        [SerializeField] private MainMenuPanelViewHandler _mainMenuPanelViewHandler;
        [SerializeField] private ControlExamplePanelHandler _controlExamplePanelHandler;
        [SerializeField] private MobileOrientationExamplePanelHandler _mobileOrientationExamplePanelHandler;
        
        [SerializeField] private WebSocketService _webSocketService;
        [SerializeField] private WebSocketCommandHandler _webSocketCommandHandler;
        [SerializeField] private HealthStatusCheckService _healthStatusCheckService;
        [SerializeField] private ReconnectService _reconnectService;
        
        [SerializeField] private ControlExampleBehaviour _controlExampleBehaviour;
        
        [SerializeField] private MobileDetectionDemo.MobileDetectionDemo _mobileDetectionDemo;

        [SerializeField] private int _creditsToCharge = 3;
        [SerializeField] private int _creditsToAdd = 5;
        
        [SerializeField] private List<string> _spaceshipNames;
        [SerializeField] private List<int> _levels;

        private readonly FloatProperty _credits = new FloatProperty();
        private readonly CustomProperty<string> _userName = new CustomProperty<string>();
        private readonly BoolProperty _isConnected = new BoolProperty();
        private CustomProperty<string> _rawResponse = new CustomProperty<string>();
        
        private readonly CustomProperty<string> _spaceShipName = new CustomProperty<string>();
        private readonly IntProperty _level = new IntProperty();

        private void Awake()
        {
            PrepareStorageCommands();
            SubscribeToEvents();
            CoroutineManager.DelayedAction(0.5f, () => LoggerHelper.Log("Warming Up CoroutineManager"));
        }

        private void Start()
        {
            PrepareDefault();
        }

        private void PrepareDefault()
        {
            _credits.SetValue(0);
            _userName.SetValue(string.Empty);
            _isConnected.SetValue(false);
            _rawResponse.SetValue(string.Empty);
            _spaceShipName.SetValue(_spaceshipNames.First());
            _level.SetValue(_levels.First());
            
            _controlExamplePanelHandler.HidePanel();
            _mainMenuPanelViewHandler.SetUnityPluginVersion(PluginVersion.Version);
        }
        
        private void SubscribeToEvents()
        {
            _credits.AddListener(_mainMenuPanelViewHandler.SetCreditsCountText);
            _userName.AddListener(_mainMenuPanelViewHandler.SetUserNameText);
            _isConnected.AddListener((x)=>_mainMenuPanelViewHandler.SetConnectionStatusText(x ? "Connected" : "Disconnected" ));
            _rawResponse.AddListener(_mainMenuPanelViewHandler.SetRawResponseText);
            
            _spaceShipName.AddListener(_mainMenuPanelViewHandler.SetSpaceshipName);
            _level.AddListener(_mainMenuPanelViewHandler.SetLevel);
            
            _webSocketCommandHandler.OnCommandProcessStateChangeEvent.AddListener(OnCommandProcessStateChange);
            
            _mainMenuPanelViewHandler.SetOnPinCodeCloseButtonClickedCallback(OnPinCodeCloseButtonClickedHandler);
            _mainMenuPanelViewHandler.SetOnConnectToServerButtonClickedCallback(OnConnectToServerButtonClickedHandler);
            _mainMenuPanelViewHandler.SetOnGetUserProfileButtonClickedCallback(OnGetUserProfileButtonClickedHandler);
            _mainMenuPanelViewHandler.SetOnAddCreditsButtonClickedCallback(OnAddCreditsButtonClickedHandler);
            _mainMenuPanelViewHandler.SetOnChargeCreditsButtonClicked(OnChargeCreditsButtonClickedHandler);
            _mainMenuPanelViewHandler.SetOnSendHealthStatusToServerButton(OnSendHealthStatusCommand);
            
            _mainMenuPanelViewHandler.SetStorageCommandButtonClickedCallback(OnStorageCommandsButtonClickedHandler);
            _mainMenuPanelViewHandler.SetBackButtonCallback(OnStorageCommandsBackButtonClickedHandler);
            _mainMenuPanelViewHandler.SetClearStorageButtonCallback(OnStorageCommandsClearButtonClickedHandler);
            _mainMenuPanelViewHandler.SetSetStorageButtonCallback(OnStorageCommandsSaveButtonClickedHandler);
            _mainMenuPanelViewHandler.SetGetStorageButtonCallback(OnStorageCommandsLoadButtonClickedHandler);
            
            _mainMenuPanelViewHandler.SetControlTestButtonClickedCallback(OnControlTestButtonClickedHandler);
            
            _controlExamplePanelHandler.SetBackButtonClickCallback(OnControlTestBackButtonCLickedHandler);
            _controlExampleBehaviour.SetArcadeButtonColorCallback(OnControlArcadeButtonButtonClickedHandler);
            
            _mainMenuPanelViewHandler.SetIsMobileTestClickedHandler(OnIsMobileTestButtonClickedHandler);
            
            _mobileOrientationExamplePanelHandler.SetBackButtonClickedCallback(OnMobileOrientationBackButtonClickedHandler);
        }

        private void OnCommandProcessStateChange(CommandProcessState state)
        {
            if (state == CommandProcessState.Sent)
            {
                _mainMenuPanelViewHandler.ShowProcessing();
            }
            else
            {
                _mainMenuPanelViewHandler.HideProcessing();
            }
        }

        private void OnConnectToServerButtonClickedHandler()
        {
            _mainMenuPanelViewHandler.ShowProcessing();
            _webSocketService.ConnectToServer(OnConnectToServerSuccess, OnConnectToServerFailure);
        }

        private void OnConnectToServerFailure()
        {
            _mainMenuPanelViewHandler.HideProcessing();
            _rawResponse.SetValue("Connection failed");
        }

        private void OnConnectToServerSuccess()
        {
            Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnConnectToServerSuccess)}] OK");
            _mainMenuPanelViewHandler.HideProcessing();
            //update status
            _isConnected.SetValue(_webSocketService.IsConnected);
        }
        
        private void OnReconnectionServiceStatusChanged(ReconnectionState reconnectionState)
        {
            switch (reconnectionState)
            {
                case ReconnectionState.Connecting:
                    _mainMenuPanelViewHandler.ShowProcessing();
                    break;
                case ReconnectionState.Connected:
                    _mainMenuPanelViewHandler.HideProcessing();
                    break;
                case ReconnectionState.ConnectingFailed:
                    _mainMenuPanelViewHandler.HideProcessing();
                    break;
            }
        }

        private void OnSendHealthStatusCommand(bool isOn)
        {
            if (isOn)
            {
                _healthStatusCheckService.Activate();
            }
            else
            {
                _healthStatusCheckService.Deactivate();
            }
        }

        private void OnGetUserProfileButtonClickedHandler()
        {
            _mainMenuPanelViewHandler.ShowProcessing();
            _webSocketCommandHandler.SendProfileRequestCommand(OnGetUserProfileSuccess, OnGetUserProfileFailure);
            _webSocketCommandHandler.SendUserBalanceRequestCommand(OnGetUserBalanceSuccess, OnGetUserBalanceFailure);
        }

        private void OnGetUserProfileSuccess(string response)
        {
            _mainMenuPanelViewHandler.HideProcessing();
            _userName.SetValue(response);
        }

        private void OnGetUserProfileFailure(int code, string response)
        {
            _mainMenuPanelViewHandler.HideProcessing();
            _rawResponse.SetValue(response);
        }

        private void OnGetUserBalanceSuccess(float credits)
        {
            _credits.SetValue(credits);
        }

        private void OnGetUserBalanceFailure(int code,  string response)
        {
            _rawResponse.SetValue(response);
        }

        private void OnPinCodeCloseButtonClickedHandler()
        {
            _mainMenuPanelViewHandler.HidePinCodeEnteringView();
        }

        private void OnAddCreditsButtonClickedHandler()
        {
            _mainMenuPanelViewHandler.SetOnPinCodeSubmittedCallback(OnAddBalancePinCodeSubmittedHandler);
            _mainMenuPanelViewHandler.ShowPinCodeEnteringView();
        }

        private void OnAddBalancePinCodeSubmittedHandler(string pinCode)
        {
            var pinCodeInt = int.Parse(pinCode);
            _webSocketCommandHandler.SendAddBalanceRequestCommand(_creditsToAdd, pinCodeInt, OnAddBalanceSuccess, OnAddBalanceFailure);
            _mainMenuPanelViewHandler.HidePinCodeEnteringView();
        }

        private void OnAddBalanceFailure(int code, string message)
        {
            _rawResponse.SetValue($"Add balance issue: code:{code}, message:{message}");
        }

        private void OnAddBalanceSuccess()
        {
            _rawResponse.SetValue("Adding credits success");
            _webSocketCommandHandler.SendUserBalanceRequestCommand(OnGetUserBalanceSuccess, OnGetUserBalanceFailure);
        }

        private void OnChargeCreditsButtonClickedHandler()
        {
            _mainMenuPanelViewHandler.SetOnPinCodeSubmittedCallback(OnChargeCreditsPinCodeSubmittedHandler);
            _mainMenuPanelViewHandler.ShowPinCodeEnteringView();
        }

        private void OnChargeCreditsPinCodeSubmittedHandler(string pinCode)
        {
            var pinCodeInt = int.Parse(pinCode);
            _webSocketCommandHandler.SendChargeUserBalanceRequestCommand(_creditsToCharge, pinCodeInt, OnChargeCreditsSuccess, OnChargeCreditsFailure);
            _mainMenuPanelViewHandler.HidePinCodeEnteringView();
        }

        private void OnChargeCreditsSuccess()
        {
            _rawResponse.SetValue("Charge credits success");
            _webSocketCommandHandler.SendUserBalanceRequestCommand(OnGetUserBalanceSuccess, OnGetUserBalanceFailure);
        }

        private void OnChargeCreditsFailure(int code, string message)
        {
            _rawResponse.SetValue(message);
        }

        private void OnIsMobileTestButtonClickedHandler()
        {
            Debug.Log($"[{GetType().Name}][{nameof(OnIsMobileTestButtonClickedHandler)}] OK");
            _mobileOrientationExamplePanelHandler.ShowPanel();
            _mobileDetectionDemo.Activate();
        }

        private void OnMobileOrientationBackButtonClickedHandler()
        {
            _mobileDetectionDemo.Activate();
            _mobileOrientationExamplePanelHandler.HidePanel();
        }
        
        #region For Storage

        private void PrepareStorageCommands()
        {
            _mainMenuPanelViewHandler.SetSpaceShipNames(_spaceshipNames);
            _mainMenuPanelViewHandler.SetLevels(_levels.ConvertAll<string>( x=> x.ToString()));
            
            OnStorageCommandsBackButtonClickedHandler();
        }

        private UserStorageData GetUserStorageData()
        {
            return new UserStorageData
            {
                SpaceshipName = _mainMenuPanelViewHandler.SpaceShipName,
                Level = _mainMenuPanelViewHandler.Level
            };
        }

        private void OnStorageCommandsButtonClickedHandler()
        {
            _mainMenuPanelViewHandler.HideMainButtons();
            _mainMenuPanelViewHandler.ShowStorageCommands();
        }

        private void OnControlTestButtonClickedHandler()
        {
            _mainMenuPanelViewHandler.HideMainMenuPanel();
            _controlExamplePanelHandler.ShowPanel();
            _controlExampleBehaviour.ActivateProcess();
        }

        private void OnControlTestBackButtonCLickedHandler()
        {
            _controlExamplePanelHandler.HidePanel();
            _mainMenuPanelViewHandler.ShowMainMenuPanel();
            _controlExampleBehaviour.DeactivateProcess();
        }

        private void OnControlArcadeButtonButtonClickedHandler(ArcadeButtonColor buttonColor, bool state)
        {
            _controlExamplePanelHandler.SetArcadeButtonColorToggleState(buttonColor, state);
        }

        private void OnStorageCommandsBackButtonClickedHandler()
        {
            _mainMenuPanelViewHandler.HideStorageCommands();
            _mainMenuPanelViewHandler.ShowMainButtons();
        }

        private void OnStorageCommandsSaveButtonClickedHandler()
        {
            _webSocketCommandHandler.SendSetUserDataRequestCommand(GetUserStorageData(),
                OnSendSetUserDataRequestSuccessHandler, OnSendSetUserDataRequestFailureHandler);
            _rawResponse.SetValue("Send User State to server");
        }

        private void OnStorageCommandsLoadButtonClickedHandler()
        {
            _webSocketCommandHandler.SendGetUserDataRequestCommand(OnSendGetUserDataRequestSuccessHandler, OnSendGetUserDataRequestFailureHandler);   
        }

        private void OnStorageCommandsClearButtonClickedHandler()
        {
            _spaceShipName.SetValue(_spaceshipNames.First());
            _level.SetValue(_levels.First());
            _webSocketCommandHandler.SendSetUserDataRequestCommand(null,
                OnSendSetUserDataRequestSuccessHandler, OnSendSetUserDataRequestFailureHandler);
        }

        private void OnSendSetUserDataRequestSuccessHandler()
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnSendSetUserDataRequestSuccessHandler)}] OK");
            _rawResponse.SetValue("Send User State to server success");
        }
        
        private void OnSendSetUserDataRequestFailureHandler(int code, string error)
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnSendSetUserDataRequestFailureHandler)}] OK, code:{code}, failure:{error}");
            _rawResponse.SetValue($"Send User State to server failure, code:{code}, failure:{error}");
        }

        private void OnSendGetUserDataRequestSuccessHandler(object data)
        {
            #if NEWTONSOFT_JSON
            var userDataPayload = (UserDataPayload)data;
            var userDataRaw = userDataPayload.Data;
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnSendGetUserDataRequestSuccessHandler)}] OK, dataRaw:{userDataRaw}, data: {data}");
            var userDataObject = (JObject)userDataRaw;
            var userStorageData = JsonConvert.DeserializeObject<UserStorageData>(userDataObject["user_data"]?.ToString() ?? string.Empty);
            
            if (userStorageData == null) return;
            
            _spaceShipName.SetValue(userStorageData.SpaceshipName);
            _level.SetValue(userStorageData.Level);
            _rawResponse.SetValue($"Get User State from server success, spaceShipName:{userStorageData.SpaceshipName}, level:{userStorageData.Level}");
            #endif
        }

        private void OnSendGetUserDataRequestFailureHandler(int code, string error)
        {
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnSendSetUserDataRequestFailureHandler)}] OK, code: {code}, error:{error}");
            _rawResponse.SetValue($"Get User State from server failure, code: {code}, error:{error}");
        }
        
        #endregion
    }

    public class UserStorageData
    {
        #if NEWTONSOFT_JSON
        [JsonProperty("spaceship_name")] public string SpaceshipName { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
        #else
        public string SpaceshipName { get; set; }
        public int Level { get; set; }
        #endif
    }
}
