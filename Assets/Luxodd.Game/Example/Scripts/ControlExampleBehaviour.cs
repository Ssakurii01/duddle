using System;
using Luxodd.Game.Scripts.Input;
using UnityEngine;

namespace Luxodd.Game.Example.Scripts
{
    public class ControlExampleBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _speed;

        private bool _isActive;

        private System.Action<ArcadeButtonColor, bool> _onArcadeButtonClickStatusChanged;
        
        public void SetArcadeButtonColorCallback(Action<ArcadeButtonColor, bool> callback)
        {
            _onArcadeButtonClickStatusChanged = callback;
        }
        
        public void ActivateProcess()
        {
            if (_isActive) return;
            Debug.Log($"[{GetType().Name}][{nameof(ActivateProcess)}] OK");
            
            _isActive = true;
        }

        public void DeactivateProcess()
        {
            if  (!_isActive) return;
            Debug.Log($"[{GetType().Name}][{nameof(DeactivateProcess)}] OK");
            _isActive = false;
        }

        private void Update()
        {
            if (!_isActive) return;
            
            var stick = ArcadeControls.GetStick();
            MoveToDirection2D(stick);

            CheckButtonStatus(ArcadeButtonColor.Black);
            CheckButtonStatus(ArcadeButtonColor.Green);
            CheckButtonStatus(ArcadeButtonColor.Blue);
            CheckButtonStatus(ArcadeButtonColor.Orange);
            CheckButtonStatus(ArcadeButtonColor.Purple);
            CheckButtonStatus(ArcadeButtonColor.Red);
            CheckButtonStatus(ArcadeButtonColor.White);
            CheckButtonStatus(ArcadeButtonColor.Yellow);
            
        }

        private void NotifyButtonColorChanged(ArcadeButtonColor color, bool state)
        {
            _onArcadeButtonClickStatusChanged?.Invoke(color, state);
        }
        
        private void MoveToDirection2D(ArcadeStick stick)
        {
            var direction = stick.Vector;
            //Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(MoveToDirection2D)}] OK, arcadeStick: {stick}");
            _target.position += new Vector3(direction.x, direction.y) * (_speed * Time.deltaTime);
        }

        private bool CheckButtonStatus(ArcadeButtonColor color)
        {
            var result = false;
            if (ArcadeControls.GetButton(color) || ArcadeControls.GetButtonDown(color))
            {
                result = true;
            }
            else if (ArcadeControls.GetButtonUp(color))
            {
                result = false;
            }
            
            NotifyButtonColorChanged(color, result);
            
            return result;
        }
    }
    
    
    
    
}
