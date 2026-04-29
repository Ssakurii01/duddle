using System;
using System.Collections.Generic;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Example.Scripts
{
    public class ElementButtonSelector : MonoBehaviour
    {
        public int CurrentElementIndex => _currentElementIndex;
        public string CurrentElementValue => _elements[_currentElementIndex];
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _previousButton;
        [SerializeField] private TMP_Text _elementNameText;
        
        private List<string> _elements = new List<string>(); 
        
        private int _currentElementIndex = 0;

        public void SetElements(List<string> elements)
        {
            _elements = elements;
        }

        public void SetCurrentElementValue(string value)
        {
            _currentElementIndex = _elements.IndexOf(value);
            _elementNameText.text = value;
            CheckForButtonInteractable();
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(SetCurrentElementValue)}] OK, value: {value}, current index: {_currentElementIndex}");
        }

        private void Awake()
        {
            _nextButton.onClick.AddListener(OnNextButtonClicked);
            _previousButton.onClick.AddListener(OnPreviousButtonClicked);
        }
        
        private void OnNextButtonClicked()
        {
            if (_currentElementIndex >= _elements.Count - 1) return;
            
            _currentElementIndex++;
            
            CheckForButtonInteractable();
            
            _elementNameText.text = _elements[_currentElementIndex];
        }
        
        private void OnPreviousButtonClicked()
        {
            if (_currentElementIndex <= 0) return;
            
            _currentElementIndex--;
            
            CheckForButtonInteractable();
            _elementNameText.text = _elements[_currentElementIndex];
        }
        
        private void SetNextButtonInteractable(bool isInteractable)
        {
            _nextButton.interactable = isInteractable;
        }
        
        private void SetPreviousButtonInteractable(bool isInteractable)
        {
            _previousButton.interactable = isInteractable;
        }
        
        private void CheckForButtonInteractable()
        {
            SetNextButtonInteractable(_currentElementIndex < _elements.Count - 1);
            SetPreviousButtonInteractable(_currentElementIndex > 0);
        }
    }
}
