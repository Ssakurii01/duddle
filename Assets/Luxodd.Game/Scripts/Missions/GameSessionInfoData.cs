#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    public class GameSessionInfoData 
    {
#if  NEWTONSOFT_JSON
        [JsonProperty("level_difficulty")] public string LevelDifficulty { get; set; }
        [JsonProperty("level_id")] public int LevelId { get; set; }
        [JsonProperty("note")] public string Note { get; set; }
        [JsonProperty("session_id")] public string GameSessionId { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("total_bet_amount")] public float TotalBetAmount { get; set; }
        [JsonProperty("total_potential_win")] public float TotalPotentialWin { get; set; }
#else
        public string LevelDifficulty { get; set; }
        public int LevelId { get; set; }
        public string Note { get; set; }
        public string GameSessionId { get; set; }
        public string Status { get; set; }
        public float TotalBetAmount { get; set; }
        public float TotalPotentialWin { get; set; }
#endif
    }
}
