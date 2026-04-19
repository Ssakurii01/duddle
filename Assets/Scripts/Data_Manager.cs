public class Data_Manager
{
    private static int High_Score = 0;

    public static void Set_HighScore(int Score)
    {
        High_Score = Score;
    }

    public static int Get_HighScore()
    {
        return High_Score;
    }
}
