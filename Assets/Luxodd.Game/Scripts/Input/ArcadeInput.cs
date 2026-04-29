using UnityEngine;

namespace Luxodd.Game.Scripts.Input
{
    public enum ArcadeButtonColor
    {
        Black,
        Red,
        Green,
        Yellow,
        Blue,
        Purple,
        Orange,
        White
    }

    public static class ArcadeInput
    {
        public static bool GetButton(ArcadeButtonColor buttonColor) =>
            UnityEngine.Input.GetKey(ArcadeUnityMapping.GetKeyCode(buttonColor));
        
        public static bool GetButtonDown(ArcadeButtonColor buttonColor) =>
            UnityEngine.Input.GetKeyDown(ArcadeUnityMapping.GetKeyCode(buttonColor));
        
        public static bool GetButtonUp(ArcadeButtonColor buttonColor) =>
            UnityEngine.Input.GetKeyUp(ArcadeUnityMapping.GetKeyCode(buttonColor));
        
        public static float Horizontal => UnityEngine.Input.GetAxis("Horizontal");
        public static float Vertical => UnityEngine.Input.GetAxis("Vertical");
    }
}
