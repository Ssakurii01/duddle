using UnityEngine;

public class File_Manager : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Data_Manager.Set_HighScore(PlayerPrefs.GetInt(HighScoreKey, 0));
        Leaderboard_Manager.Load();
    }

    public static void Save_Info()
    {
        PlayerPrefs.SetInt(HighScoreKey, Data_Manager.Get_HighScore());
        PlayerPrefs.Save();
    }
}
