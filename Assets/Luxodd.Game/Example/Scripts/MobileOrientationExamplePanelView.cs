using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Example.Scripts
{
    public class MobileOrientationExamplePanelView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        
        private System.Action _onBackButtonClickedCallback;

        public void SetBackButtonClickedCallback(System.Action callback)
        {
            _onBackButtonClickedCallback = callback;
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackButtonClickHandler);
            Hide();
        }

        private void OnBackButtonClickHandler()
        {
            _onBackButtonClickedCallback?.Invoke();
        }
    }
}
