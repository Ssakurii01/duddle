using UnityEngine;

/// <summary>
/// Wraps the legacy Input Manager calls in try/catch so the code still
/// compiles and runs when the project's Active Input Handling is set to
/// "Input System Package (New)" only — in that mode UnityEngine.Input
/// throws an exception. Returns zero / false on failure, letting calls
/// to ArcadeControls / Keyboard.current pick up the slack instead.
/// </summary>
public static class Safe_Input
{
    public static float GetAxisRaw(string axisName)
    {
        try { return Input.GetAxisRaw(axisName); }
        catch { return 0f; }
    }

    public static float GetAxis(string axisName)
    {
        try { return Input.GetAxis(axisName); }
        catch { return 0f; }
    }

    public static bool GetKey(KeyCode key)
    {
        try { return Input.GetKey(key); }
        catch { return false; }
    }

    public static bool GetKeyDown(KeyCode key)
    {
        try { return Input.GetKeyDown(key); }
        catch { return false; }
    }

    public static bool GetKeyUp(KeyCode key)
    {
        try { return Input.GetKeyUp(key); }
        catch { return false; }
    }
}
