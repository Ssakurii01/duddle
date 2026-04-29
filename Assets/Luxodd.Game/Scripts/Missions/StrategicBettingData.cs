
using System.Collections.Generic;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Missions
{
    public class StrategicBettingData
    {
#if NEWTONSOFT_JSON
        [JsonProperty("level_id")] public int LevelId { get; set; }
        [JsonProperty("missions")] public List<MissionBettingInfo> Missions { get; set; }
        [JsonProperty("level_difficulty")] public DifficultyLevel LevelDifficulty { get; set; }
#else  
        public int LevelId { get; set; }
        public List<MissionBettingInfo> Missions { get; set; }
        public DifficultyLevel LevelDifficulty { get; set; }
#endif
    }

    public class MissionBettingInfo
    {
#if NEWTONSOFT_JSON
        [JsonProperty("id")] public string MissionId { get; set; }
        [JsonProperty("bet")] public float Bet { get; set; }
        [JsonProperty("calculated_hardness")] public float CalculatedHardness { get; set; }
#else
        public string MissionId { get; set; }
        public float Bet { get; set; }
        public float CalculatedHardness { get; set; }
#endif
    }
}
