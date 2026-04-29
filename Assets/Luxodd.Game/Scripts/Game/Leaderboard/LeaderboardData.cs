using System.Collections.Generic;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Game.Leaderboard
{
    public class LeaderboardData
    {
#if NEWTONSOFT_JSON
        [JsonProperty("rank")] public int Rank { get; set; }
        [JsonProperty("game_handle")] public string PlayerName { get; set; }
        [JsonProperty("score_total")] public int TotalScore { get; set; }
#else
        public int Rank { get; set; }
        public string PlayerName { get; set; }
        public int TotalScore { get; set; }
#endif
    }

    public class LeaderboardResponseCommand
    {
#if NEWTONSOFT_JSON
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("data")] public LeaderboardDataResponse Data { get; set; }
#else
        public string Type { get; set; }
        public LeaderboardDataResponse Data { get; set; }
#endif
    }

    public class LeaderboardDataResponse
    {
#if NEWTONSOFT_JSON
        [JsonProperty("current_user")] public LeaderboardData CurrentUserData { get; set; }
        [JsonProperty("leaderboard")] public List<LeaderboardData> Leaderboard { get; set; }
#else
        public LeaderboardData CurrentUserData { get; set; }
        public List<LeaderboardData> Leaderboard { get; set; }
#endif
    }
}