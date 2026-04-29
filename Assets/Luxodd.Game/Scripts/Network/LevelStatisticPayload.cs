#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network
{
    public class LevelStatisticPayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("level")]public int Level { get; set; }
        [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]public int Score { get; set; }
        [JsonProperty("accuracy", NullValueHandling = NullValueHandling.Ignore)]public int Accuracy { get; set; }
        [JsonProperty("time_taken", NullValueHandling = NullValueHandling.Ignore)]public int TotalSeconds { get; set; }
        [JsonProperty("enemies_killed", NullValueHandling = NullValueHandling.Ignore)]public int EnemiesKilled { get; set; }
        [JsonProperty("completion_percentage", NullValueHandling = NullValueHandling.Ignore)]public int Progress { get; set; }
#else
        public int Level { get; set; }
        public int Score { get; set; }
        public int Accuracy { get; set; }
        public int TotalSeconds { get; set; }
        public int EnemiesKilled { get; set; }
        public int Progress { get; set; }
#endif
    }
}