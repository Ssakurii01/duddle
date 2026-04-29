using Luxodd.Game.Scripts.Input;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace Luxodd.Game
{
    public static class ArcadeControls
    {
        
        public static ArcadeInputConfigAsset Config { get; set; }

        public static bool GetButton(ArcadeButtonColor buttonColor)
        {
#if ENABLE_INPUT_SYSTEM
            if (IsNewInputActive())
                return GetButton_New(buttonColor);
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return GetButton_Legacy(buttonColor);
#else
            return false;
#endif
        }

        public static bool GetButtonDown(ArcadeButtonColor buttonColor)
        {
#if ENABLE_INPUT_SYSTEM
            if (IsNewInputActive())
                return GetButtonDown_New(buttonColor);
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return GetButtonDown_Legacy(buttonColor);
#else
            return false;
#endif
        }

        public static bool GetButtonUp(ArcadeButtonColor buttonColor)
        {
#if ENABLE_INPUT_SYSTEM
            if (IsNewInputActive())
                return GetButtonUp_New(buttonColor);
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return GetButtonUp_Legacy(buttonColor);
#else
            return false;
#endif
        }

        /// <summary>
        /// Returns stick axes as ArcadeStick (Vector2 internally).
        /// Uses Input System if enabled/active; otherwise uses Legacy Input Manager axes.
        /// </summary>
        public static ArcadeStick GetStick()
        {
            var config = Config;

            var deadZone = config ? config.DeadZone : 0.15f;
            var invertX = config && config.InvertX;
            var invertY = config && config.InvertY;

            Vector2 raw;

#if ENABLE_INPUT_SYSTEM
            if (IsNewInputActive())
            {
                raw = GetStick_New();
            }
            else
#endif
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                raw = GetStick_Legacy(config);
#else
                raw = Vector2.zero;
#endif
            }

            if (invertX) raw.x *= -1f;
            if (invertY) raw.y *= -1f;

            var deadZoneVector = ApplyDeadZone(raw, deadZone);
            return new ArcadeStick(deadZoneVector.x, deadZoneVector.y);
        }


        // Legacy Input Manager implementation

#if ENABLE_LEGACY_INPUT_MANAGER
        private static bool GetButton_Legacy(ArcadeButtonColor buttonColor) =>
            UnityEngine.Input.GetKey(ArcadeUnityMapping.GetKeyCode(buttonColor));

        private static bool GetButtonDown_Legacy(ArcadeButtonColor buttonColor) =>
            UnityEngine.Input.GetKeyDown(ArcadeUnityMapping.GetKeyCode(buttonColor));

        private static bool GetButtonUp_Legacy(ArcadeButtonColor buttonColor) =>
            UnityEngine.Input.GetKeyUp(ArcadeUnityMapping.GetKeyCode(buttonColor));

        private static Vector2 GetStick_Legacy(ArcadeInputConfigAsset config)
        {
            var xAxis = config ? config.HorizontalAxisName : "Horizontal";
            var yAxis = config ? config.VerticalAxisName : "Vertical";

            var xValue = SafeGetAxis_Legacy(xAxis);
            var yValue = SafeGetAxis_Legacy(yAxis);

            return new Vector2(xValue, yValue);
        }

        private static float SafeGetAxis_Legacy(string axisName)
        {
            try
            {
                return UnityEngine.Input.GetAxis(axisName);
            }
            catch
            {
                return 0f;
            }
        }
#endif


        // New Input System implementation (Joystick preferred; Gamepad fallback)
        
#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Detects whether the Input System is active in runtime.
        /// In Unity 6 with Active Input Handling = "Input System Package (New)",
        /// calling UnityEngine.Input will throw, so we route through InputSystem.
        /// </summary>
        private static bool IsNewInputActive()
        {
            // In projects where Input System is enabled, InputSystem.settings is non-null.
            return InputSystem.settings != null;
        }

        private static bool GetButton_New(ArcadeButtonColor buttonColor)
        {
            var ctrl = MapColorToButtonControl(buttonColor);
            return ctrl != null && ctrl.isPressed;
        }

        private static bool GetButtonDown_New(ArcadeButtonColor buttonColor)
        {
            var ctrl = MapColorToButtonControl(buttonColor);
            return ctrl != null && ctrl.wasPressedThisFrame;
        }

        private static bool GetButtonUp_New(ArcadeButtonColor buttonColor)
        {
            var ctrl = MapColorToButtonControl(buttonColor);
            return ctrl != null && ctrl.wasReleasedThisFrame;
        }

        private static Vector2 GetStick_New()
        {
            // Prefer Joystick (HID/generic arcade controller)
            var js = GetArcadeJoystick();
            if (js != null)
            {
                // Joystick has "stick" (Vector2)
                return js.stick.ReadValue();
            }

            // Fallback to Gamepad
            var pad = Gamepad.current;
            if (pad != null)
            {
                // Default: leftStick
                return pad.leftStick.ReadValue();
            }

            return Vector2.zero;
        }

        private static Joystick GetArcadeJoystick()
        {
            // Often arcade controllers show up as Joystick (HID).
            return Joystick.current;
        }

        /// <summary>
        /// Maps ArcadeButtonColor to a ButtonControl.
        /// 1) Tries Joystick button index mapping (your confirmed JoystickButtonX mapping).
        /// 2) Falls back to common Gamepad mapping when device is recognized as Gamepad.
        /// </summary>
        private static ButtonControl MapColorToButtonControl(ArcadeButtonColor color)
        {
            var js = GetArcadeJoystick();
            if (js != null)
            {
                var index = ColorToJoystickButtonIndex(color);
                if (index >= 0)
                {
                    var btn = js.TryGetChildControl<ButtonControl>($"button{index}");
                    if (btn != null) return btn;
                }
            }
            
            var pad = Gamepad.current;
            if (pad != null)
            {
                return ColorToGamepadButton(color, pad);
            }

            return null;
        }

        /// <summary>
        /// We are use this mapping:
        /// Black=0, Red=1, Green=2, Yellow=3, Blue=4, Purple=5, Orange=8, White=9
        /// </summary>
        private static int ColorToJoystickButtonIndex(ArcadeButtonColor color)
        {
            return color switch
            {
                ArcadeButtonColor.Black  => 0,
                ArcadeButtonColor.Red    => 1,
                ArcadeButtonColor.Green  => 2,
                ArcadeButtonColor.Yellow => 3,
                ArcadeButtonColor.Blue   => 4,
                ArcadeButtonColor.Purple => 5,
                ArcadeButtonColor.Orange => 8,
                ArcadeButtonColor.White  => 9,
                _ => -1
            };
        }

        /// <summary>
        /// Fallback mapping for Gamepad devices.
        /// Adjust if your arcade controller maps colors differently in Gamepad mode.
        /// </summary>
        private static ButtonControl ColorToGamepadButton(ArcadeButtonColor color, Gamepad pad)
        {
            return color switch
            {
                // Common ABXY scheme:
                ArcadeButtonColor.Black  => pad.buttonSouth, // A / Cross
                ArcadeButtonColor.Red    => pad.buttonEast,  // B / Circle
                ArcadeButtonColor.Blue   => pad.buttonWest,  // X / Square
                ArcadeButtonColor.Yellow => pad.buttonNorth, // Y / Triangle

                // Optional extra buttons:
                ArcadeButtonColor.Green  => pad.leftShoulder,
                ArcadeButtonColor.Purple => pad.rightShoulder,
                ArcadeButtonColor.White  => pad.startButton,
                ArcadeButtonColor.Orange => pad.selectButton,

                _ => null
            };
        }
#endif
        
        // Helpers

        private static Vector2 ApplyDeadZone(Vector2 input, float deadZone)
        {
            if (deadZone <= 0f) return input;

            var magnitude = input.magnitude;
            if (magnitude < deadZone) return Vector2.zero;

            var scaled = (magnitude - deadZone) / (1f - deadZone);
            return input.normalized * Mathf.Clamp01(scaled);
        }
    }
}
