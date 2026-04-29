using System;
using Luxodd.Game.Scripts.HelpersAndUtils.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Example.Scripts
{
    public class MainButtonViewHandler : MonoBehaviour
    {
        [SerializeField] private Button _connectToServerButton;
        [SerializeField] private ToggleButton _sendHealthStatusToServerButton;
        [SerializeField] private Button _getUserProfileButton;
        [SerializeField] private Button _addCreditsButton;
        [SerializeField] private Button _chargeCreditsButton;
        [SerializeField] private Button _storageCommandsButton;
        [SerializeField] private Button _controllTestButton;
        [SerializeField] private Button _isMobileTestButton;

        private Action _onConnectedToServerButtonClickedCallback;
        private Action _onGetUserProfileButtonClickedCallback;
        private Action _onAddCreditsButtonClickedCallback;
        private Action _onChargeCreditsButtonClickedCallback;
        private Action _onStorageCommandsButtonClickedCallback;
        private Action _onControlTestButtonClickedCallback;
        private Action<bool> _onSendHealthStatusToServerButtonClickedCallback;
        private Action _isMobileTestButtonClickedCallback;

        public void SetOnConnectedToServerButtonClickedCallback(Action onConnectedToServerButtonClickedCallback)
        {
            _onConnectedToServerButtonClickedCallback = onConnectedToServerButtonClickedCallback;
        }

        public void SetOnGetUserProfileButtonClickedCallback(Action onGetUserProfileButtonClickedCallback)
        {
            _onGetUserProfileButtonClickedCallback = onGetUserProfileButtonClickedCallback;
        }

        public void SetOnAddCreditsButtonClickedCallback(Action onAddCreditsButtonClickedCallback)
        {
            _onAddCreditsButtonClickedCallback = onAddCreditsButtonClickedCallback;
        }

        public void SetOnChargeCreditsButtonClickedCallback(Action onChargeCreditsButtonClickedCallback)
        {
            _onChargeCreditsButtonClickedCallback = onChargeCreditsButtonClickedCallback;
        }

        public void SetOnSendHealthStatusToServerButtonClickedCallback(
            Action<bool> onSendHealthStatusToServerButtonClickedCallback)
        {
            _onSendHealthStatusToServerButtonClickedCallback = onSendHealthStatusToServerButtonClickedCallback;
        }
        
        public void SetOnStorageCommandsButtonClickedCallback(Action onStorageCommandsButtonClickedCallback)
        {
            _onStorageCommandsButtonClickedCallback = onStorageCommandsButtonClickedCallback;
        }
        
        public void SetOnControlTestButtonClickedCallback(Action onControlTestButtonClickedCallback)
        {
            _onControlTestButtonClickedCallback = onControlTestButtonClickedCallback;
        }

        public void SetIsMobileTestButtonClickedCallback(Action onIsMobileTestButtonClickedCallback)
        {
            _isMobileTestButtonClickedCallback = onIsMobileTestButtonClickedCallback;
        }
        
        private void Awake()
        {
            _connectToServerButton.onClick.AddListener(OnConnectToServerButtonClickedHandler);
            _getUserProfileButton.onClick.AddListener(OnGetUserProfileButtonClickedHandler);
            _addCreditsButton.onClick.AddListener(OnAddCreditsButtonClickedHandler);
            _chargeCreditsButton.onClick.AddListener(OnChargeCreditsButtonClickedHandler);
            _sendHealthStatusToServerButton.ToggleEvent.AddListener(OnSendHealthStatusToServerButtonClickedHandler);
            _storageCommandsButton.onClick.AddListener(OnStorageCommandsButtonClickedHandler);
            _controllTestButton.onClick.AddListener(OnControlTestButtonClickedHandler);
            _isMobileTestButton.onClick.AddListener(OnIsMobileTestButtonClickedHandler);
            
        }
        
        private void OnConnectToServerButtonClickedHandler()
        {
            _onConnectedToServerButtonClickedCallback?.Invoke();    
        }

        private void OnGetUserProfileButtonClickedHandler()
        {
            _onGetUserProfileButtonClickedCallback?.Invoke();
        }

        private void OnAddCreditsButtonClickedHandler()
        {
            _onAddCreditsButtonClickedCallback?.Invoke();
        }

        private void OnChargeCreditsButtonClickedHandler()
        {
            _onChargeCreditsButtonClickedCallback?.Invoke();
        }

        private void OnSendHealthStatusToServerButtonClickedHandler(bool value)
        {
            _onSendHealthStatusToServerButtonClickedCallback?.Invoke(value);
        }
        
        private void OnStorageCommandsButtonClickedHandler()
        {
            _onStorageCommandsButtonClickedCallback?.Invoke();
        }

        private void OnControlTestButtonClickedHandler()
        {
            _onControlTestButtonClickedCallback?.Invoke();
        }

        private void OnIsMobileTestButtonClickedHandler()
        {
            _isMobileTestButtonClickedCallback?.Invoke();
        }
    }
}
