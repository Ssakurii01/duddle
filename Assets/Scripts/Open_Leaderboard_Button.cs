using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tiny helper for the "Leaderboard" button on the main menu.
/// Just attach this to the button GameObject and wire its OnClick
/// to Open_Leaderboard_Button.Open().
/// </summary>
public class Open_Leaderboard_Button : MonoBehaviour
{
    [Tooltip("Name of the leaderboard scene to load.")]
    public string SceneName = "Leaderboard";

    public void Open()
    {
        SceneManager.LoadScene(SceneName);
    }
}
