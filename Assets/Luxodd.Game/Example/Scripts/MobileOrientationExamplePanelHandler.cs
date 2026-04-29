using UnityEngine;

namespace Luxodd.Game.Example.Scripts
{
    public class MobileOrientationExamplePanelHandler : MonoBehaviour
    {
        [SerializeField] private MobileOrientationExamplePanelView _panelView;
    

        public void SetBackButtonClickedCallback(System.Action callback)
        {
            _panelView.SetBackButtonClickedCallback(callback);
        }

        public void ShowPanel()
        {
            _panelView.Show();
        }

        public void HidePanel()
        {
            _panelView.Hide();
        }
    }
}
