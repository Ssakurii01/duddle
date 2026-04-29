using Luxodd.Game.Scripts.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Example.Scripts
{
    public class ColorButtonToggle : MonoBehaviour
    {
        public ArcadeButtonColor ButtonColor => _buttonColor;
        
        [SerializeField] private Toggle _toggle;
        [SerializeField] private ArcadeButtonColor _buttonColor;

        public void SetToggle(bool isOn)
        {
            _toggle.isOn = isOn;
        }

        private void Awake()
        {
            SetToggle(false);
        }
    }
}
