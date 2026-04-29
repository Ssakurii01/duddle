using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Example.Scripts
{
    public class PinCodeEnteringViewHandler : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _pinCodeInputField;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private int _minimalPinCount = 5;
        
        private Action _onCloseButtonClickedCallback;
        private Action _onNextButtonClickedCallback;
        private Action<string> _onPinCodeSubmittedCallback;

        public void SetCloseButtonClickedCallback(Action onCloseButtonClickedCallback)
        {
            _onCloseButtonClickedCallback = onCloseButtonClickedCallback;
        }

        public void SetNextButtonClickedCallback(Action onNextButtonClickedCallback)
        {
            _onNextButtonClickedCallback = onNextButtonClickedCallback;
        }

        public void SetPinCodeSubmittedCallback(Action<string> onPinCodeSubmittedCallback)
        {
            _onPinCodeSubmittedCallback = onPinCodeSubmittedCallback;
        }
        
        public void Show()
        {
            _pinCodeInputField.text = string.Empty;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void Awake()
        {
            Hide();
            
            _closeButton.onClick.AddListener(OnCloseButtonClickedHandler);
            _nextButton.onClick.AddListener(OnNextButtonClickedHandler);
            _pinCodeInputField.onSubmit.AddListener(OnPinCodeSubmitted);
            _pinCodeInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        }

        private void OnCloseButtonClickedHandler()
        {
            _onCloseButtonClickedCallback?.Invoke();
        }

        private void OnNextButtonClickedHandler()
        {
            _onNextButtonClickedCallback?.Invoke();
            _onPinCodeSubmittedCallback?.Invoke(_pinCodeInputField.text);
        }

        private void OnPinCodeSubmitted(string pinCode)
        {
            _onPinCodeSubmittedCallback?.Invoke(pinCode);
        }

        private void OnInputFieldValueChanged(string input)
        {
            _nextButton.interactable = input.Length >= _minimalPinCount;
        }
    }
}
