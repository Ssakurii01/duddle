using System;
using Luxodd.Game.Scripts.Input;
using UnityEngine;

namespace Luxodd.Game.Example.Scripts.ControlTest
{
    /// <summary>
    /// Reads arcade hardware input via ArcadeControls and translates it into gameplay actions.
    /// </summary>
    public sealed class ArcadePlayerControlAdapter : MonoBehaviour, IPlayerControlAdapter
    {
        public Vector2 MovementVector { get; private set; }

        public event Action JumpButtonPressed;
        public event Action UseItemButtonPressed;

        public bool IsFireButtonPressed { get; private set; }

        private void Update()
        {
            var stickData = ArcadeControls.GetStick();
            MovementVector = stickData.Vector;

            // Fire - hold Red
            if (ArcadeControls.GetButtonDown(ArcadeButtonColor.Red))
            {
                IsFireButtonPressed = true;
            }
            else if (ArcadeControls.GetButtonUp(ArcadeButtonColor.Red))
            {
                IsFireButtonPressed = false;
            }

            // Jump - Black
            if (ArcadeControls.GetButtonDown(ArcadeButtonColor.Black))
            {
                JumpButtonPressed?.Invoke();
            }

            // Use item - Green
            if (ArcadeControls.GetButtonDown(ArcadeButtonColor.Green))
            {
                UseItemButtonPressed?.Invoke();
            }
        }
    }
}