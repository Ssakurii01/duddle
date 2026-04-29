using Luxodd.Game.Scripts.Input;
using UnityEngine;

namespace Luxodd.Game.Example.Scripts
{
    public class ControlExamplePanelHandler : MonoBehaviour
    {
        [SerializeField] private ControlExamplePanelView _controlExamplePanelView;

        public void ShowPanel()
        {
            _controlExamplePanelView.Show();
        }

        public void HidePanel()
        {
            _controlExamplePanelView.Hide();
        }

        public void SetColorButtonColorToggleState(ArcadeButtonColor buttonColor, bool state)
        {
            _controlExamplePanelView.SetButtonColorToggleState(buttonColor, state);
        }

        public void SetBackButtonClickCallback(System.Action callback)
        {
            _controlExamplePanelView.SetBackButtonClickCallback(callback);
        }

        public void SetArcadeButtonColorToggleState(ArcadeButtonColor buttonColor, bool state)
        {
            _controlExamplePanelView.SetButtonColorToggleState(buttonColor, state);
        }
    }
}
