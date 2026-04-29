using Luxodd.Game.HelpersAndUtils.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Scripts.HelpersAndUtils.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleButton : MonoBehaviour
    {
        public ISimpleEvent<bool> ToggleEvent => _toggleEvent;
        public bool ToggleValue => _toggle.isOn;
        [SerializeField] private TMP_Text _toggleText;
        
        [SerializeField] private Toggle _toggle;
        
        [SerializeField] private string _onText = "On";
        [SerializeField] private string _offText = "Off";
        
        private SimpleEvent<bool> _toggleEvent = new SimpleEvent<bool>();

        public void SetValue(bool value)
        {
            _toggle.isOn = value;
            OnToggleValueChanged(value);
        }

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void Start()
        {
            OnToggleValueChanged(_toggle.isOn);
        }
        
        private void OnToggleValueChanged(bool value)
        {
            _toggleEvent.Notify(value);

            if (value)
            {
                SetOn();
            }
            else
            {
                SetOff();
            }
        }

        private void SetOn()
        {
            _toggleText.text = _onText;
        }

        private void SetOff()
        {
            _toggleText.text = _offText;
        }
    }
}
