using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Luxodd.Game.Example.Scripts
{
    public class MainMenuPanelViewHandler : MonoBehaviour
    {
        public string SpaceShipName => _storageCommandViewHandler.SpaceshipName;
        public int Level => int.Parse(_storageCommandViewHandler.Level);
        
        [SerializeField] private MainButtonViewHandler _mainButtonViewHandler;
        [SerializeField] private PinCodeEnteringViewHandler _pinCodeEnteringViewHandler;
        [SerializeField] private ProcessingViewHandler _processingViewHandler;
        [SerializeField] private StorageCommandViewHandler _storageCommandViewHandler;
        
        [SerializeField] private TMP_Text _connectionStatusText;
        [SerializeField] private TMP_Text _userNameText;
        [SerializeField] private TMP_Text _creditsCountText;
        [SerializeField] private TMP_Text _rawResponseText;
        [SerializeField] private TMP_Text _spaceshipNameText;
        [SerializeField] private TMP_Text _currentLevelText;
        [SerializeField] private TMP_Text _unityPluginVersionText;
        
        private string _connectionStatusTextFormat;
        private string _userNameTextFormat;
        private string _creditsCountTextFormat;
        private string _spaceshipNameTextFormat;
        private string _currentLevelTextFormat;
        private string _unityPluginVersionTextFormat;

        public void ShowMainMenuPanel()
        {
            gameObject.SetActive(true);
        }

        public void HideMainMenuPanel()
        {
            gameObject.SetActive(false);
        }
        
        public void ShowProcessing()
        {
            _processingViewHandler.Show();
        }

        public void HideProcessing()
        {
            _processingViewHandler.Hide();
        }

        public void ShowPinCodeEnteringView()
        {
            _pinCodeEnteringViewHandler.Show();
        }

        public void HidePinCodeEnteringView()
        {
            _pinCodeEnteringViewHandler.Hide();
        }

        public void SetOnChargeCreditsButtonClicked(Action onChargeCreditsButtonClickedCallback)
        {
            _mainButtonViewHandler.SetOnChargeCreditsButtonClickedCallback(onChargeCreditsButtonClickedCallback);
        }

        public void SetOnSendHealthStatusToServerButton(Action<bool> onSendHealthStatusToServerButtonClickedCallback)
        {
            _mainButtonViewHandler.SetOnSendHealthStatusToServerButtonClickedCallback(onSendHealthStatusToServerButtonClickedCallback);
        }

        public void SetOnConnectToServerButtonClickedCallback(Action onConnectToServerButtonClickedCallback)
        {
            _mainButtonViewHandler.SetOnConnectedToServerButtonClickedCallback(onConnectToServerButtonClickedCallback);
        }

        public void SetOnGetUserProfileButtonClickedCallback(Action onGetUserProfileButtonClickedCallback)
        {
            _mainButtonViewHandler.SetOnGetUserProfileButtonClickedCallback(onGetUserProfileButtonClickedCallback);
        }

        public void SetOnAddCreditsButtonClickedCallback(Action onAddCreditsButtonClickedCallback)
        {
            _mainButtonViewHandler.SetOnAddCreditsButtonClickedCallback(onAddCreditsButtonClickedCallback);
        }

        public void SetOnPinCodeCloseButtonClickedCallback(Action onPinCodeCloseButtonClickedCallback)
        {
            _pinCodeEnteringViewHandler.SetCloseButtonClickedCallback(onPinCodeCloseButtonClickedCallback);
        }

        public void SetOnPinCodeNextButtonClickedCallback(Action onPinCodeNextButtonClickedCallback)
        {
            _pinCodeEnteringViewHandler.SetNextButtonClickedCallback(onPinCodeNextButtonClickedCallback);
        }

        public void SetOnPinCodeSubmittedCallback(Action<string> onPinCodeSubmittedCallback)
        {
            _pinCodeEnteringViewHandler.SetPinCodeSubmittedCallback(onPinCodeSubmittedCallback);
        }

        public void SetConnectionStatusText(string text)
        {
            _connectionStatusText.text = string.Format(_connectionStatusTextFormat, text);
        }

        public void SetUserNameText(string text)
        {
            _userNameText.text = string.Format(_userNameTextFormat, text);
        }

        public void SetCreditsCountText(float creditsCount)
        {
            _creditsCountText.text = string.Format(_creditsCountTextFormat, creditsCount);
        }

        public void SetRawResponseText(string text)
        {
            _rawResponseText.text = text;
        }
        
        public void ShowStorageCommands()
        {
            _storageCommandViewHandler.gameObject.SetActive(true);
        }
        
        public void HideStorageCommands()
        {
            _storageCommandViewHandler.gameObject.SetActive(false);
        }

        public void ShowMainButtons()
        {
            _mainButtonViewHandler.gameObject.SetActive(true);
        }
        
        public void HideMainButtons()
        {
            _mainButtonViewHandler.gameObject.SetActive(false);
        }

        public void SetStorageCommandButtonClickedCallback(Action onStorageCommandButtonClickedCallback)
        {
            _mainButtonViewHandler.SetOnStorageCommandsButtonClickedCallback(onStorageCommandButtonClickedCallback);
        }
        
        public void SetControlTestButtonClickedCallback(Action onControlTestButtonClickedCallback)
        {
            _mainButtonViewHandler.SetOnControlTestButtonClickedCallback(onControlTestButtonClickedCallback);
        }

        public void SetClearStorageButtonCallback(Action onClearStorageButtonClickedCallback)
        {
            _storageCommandViewHandler.SetClearStorageButtonCallback(onClearStorageButtonClickedCallback);
        }
        
        public void SetGetStorageButtonCallback(Action onGetStorageButtonClickedCallback)
        {
            _storageCommandViewHandler.SetGetStorageButtonCallback(onGetStorageButtonClickedCallback);
        }

        public void SetSetStorageButtonCallback(Action onSetStorageButtonClickedCallback)
        {
            _storageCommandViewHandler.SetSetStorageButtonCallback(onSetStorageButtonClickedCallback);
        }

        public void SetBackButtonCallback(Action onBackButtonClickedCallback)
        {
            _storageCommandViewHandler.SetBackButtonCallback(onBackButtonClickedCallback);
        }
        
        public void SetSpaceshipName(string spaceshipName)
        {
            _spaceshipNameText.text = string.Format(_spaceshipNameTextFormat, spaceshipName);
            _storageCommandViewHandler.SetSpaceshipName(spaceshipName);
        }
        
        public void SetLevel(int level)
        {
            _currentLevelText.text = string.Format(_currentLevelTextFormat, level);
            _storageCommandViewHandler.SetLevel(level);
        }

        public void SetSpaceShipNames(List<string> spaceshipNames)
        {
            _storageCommandViewHandler.SetSpaceshipNames(spaceshipNames);
        }
        
        public void SetLevels(List<string> levels)
        {
            _storageCommandViewHandler.SetLevels(levels);
        }

        public void SetUnityPluginVersion(string unityPluginVersion)
        {
            _unityPluginVersionText.text = string.Format(_unityPluginVersionTextFormat, unityPluginVersion);
        }

        public void SetIsMobileTestClickedHandler(Action onIsMobileTestClickedCallback)
        {
            _mainButtonViewHandler.SetIsMobileTestButtonClickedCallback(onIsMobileTestClickedCallback);
        }

        private void Awake()
        {
            _connectionStatusTextFormat = _connectionStatusText.text;
            _userNameTextFormat = _userNameText.text;
            _creditsCountTextFormat = _creditsCountText.text;
            _spaceshipNameTextFormat = _spaceshipNameText.text;
            _currentLevelTextFormat = _currentLevelText.text;
            _unityPluginVersionTextFormat = _unityPluginVersionText.text;
        }
    }
}
