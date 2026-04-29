using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Example.Scripts
{
    public class StorageCommandViewHandler : MonoBehaviour
    {
        public string SpaceshipName => _spaceshipNameElementButtonSelector.CurrentElementValue;
        public string Level => _levelElementButtonSelector.CurrentElementValue;
        
        [SerializeField] private Button _clearStorageButton;
        [SerializeField] private Button _getStorageButton;
        [SerializeField] private Button _setStorageButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private ElementButtonSelector _spaceshipNameElementButtonSelector;
        [SerializeField] private ElementButtonSelector _levelElementButtonSelector;
        
        private System.Action _onClearStorageButtonClicked;
        private System.Action _onGetStorageButtonClicked;
        private System.Action _onSetStorageButtonClicked;
        private System.Action _onBackButtonClicked;
        
        public void SetClearStorageButtonCallback(System.Action onClearStorageButtonClicked)
        {
            _onClearStorageButtonClicked = onClearStorageButtonClicked;
        }
        
        public void SetGetStorageButtonCallback(System.Action onGetStorageButtonClicked)
        {
            _onGetStorageButtonClicked = onGetStorageButtonClicked;
        }
        
        public void SetSetStorageButtonCallback(System.Action onSetStorageButtonClicked)
        {
            _onSetStorageButtonClicked = onSetStorageButtonClicked;
        }
        
        public void SetBackButtonCallback(System.Action onBackButtonClicked)
        {
            _onBackButtonClicked = onBackButtonClicked;
        }
        
        public void SetSpaceshipNames(List<string> spaceshipNames)
        {
            _spaceshipNameElementButtonSelector.SetElements(spaceshipNames);
        }
        
        public void SetLevels(List<string> levels)
        {
            _levelElementButtonSelector.SetElements(levels);
        }

        public void SetSpaceshipName(string spaceshipName)
        {
            _spaceshipNameElementButtonSelector.SetCurrentElementValue(spaceshipName);
        }

        public void SetLevel(int level)
        {
            _levelElementButtonSelector.SetCurrentElementValue(level.ToString());
        }

        private void Awake()
        {
            _clearStorageButton.onClick.AddListener(OnClearStorageButtonClicked);
            _getStorageButton.onClick.AddListener(OnGetStorageButtonClicked);
            _setStorageButton.onClick.AddListener(OnSetStorageButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }
        
        private void OnClearStorageButtonClicked()
        {
            _onClearStorageButtonClicked?.Invoke();
        }
        
        private void OnGetStorageButtonClicked()
        {
            _onGetStorageButtonClicked?.Invoke();
        }
        
        private void OnSetStorageButtonClicked()
        {
            _onSetStorageButtonClicked?.Invoke();
        }
        
        private void OnBackButtonClicked()
        {
            _onBackButtonClicked?.Invoke();
        }
    }
}
