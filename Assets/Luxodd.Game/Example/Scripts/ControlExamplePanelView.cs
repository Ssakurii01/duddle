using Luxodd.Game.Scripts.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Example.Scripts
{
    public class ControlExamplePanelView : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private ColorButtonToggle[] _toggles;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetButtonColorToggleState(ArcadeButtonColor buttonColor, bool state)
        {
            foreach (var colorButtonToggle in _toggles)
            {
                if (colorButtonToggle.ButtonColor != buttonColor) continue;
                colorButtonToggle.SetToggle(state);
                break;
            }
        }

        public void SetBackButtonClickCallback(System.Action callback)
        {
            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(()=> callback?.Invoke());
        }
    }
}
