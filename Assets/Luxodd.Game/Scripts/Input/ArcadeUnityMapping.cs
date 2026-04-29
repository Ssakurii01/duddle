using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Input
{
    public static class ArcadeUnityMapping
    {
        public static readonly IReadOnlyDictionary<ArcadeButtonColor, KeyCode> ButtonToKeyCode =
            new Dictionary<ArcadeButtonColor, KeyCode>()
            {
                { ArcadeButtonColor.Black, KeyCode.JoystickButton0 },
                { ArcadeButtonColor.Red, KeyCode.JoystickButton1 },
                { ArcadeButtonColor.Green, KeyCode.JoystickButton2 },
                { ArcadeButtonColor.Yellow, KeyCode.JoystickButton3 },
                { ArcadeButtonColor.Blue, KeyCode.JoystickButton4 },
                { ArcadeButtonColor.Purple, KeyCode.JoystickButton5 },
                { ArcadeButtonColor.Orange, KeyCode.JoystickButton8 },
                { ArcadeButtonColor.White, KeyCode.JoystickButton9 },
            };

        public static KeyCode GetKeyCode(ArcadeButtonColor buttonColor)
        {
            return ButtonToKeyCode[buttonColor];
        }
    }
}