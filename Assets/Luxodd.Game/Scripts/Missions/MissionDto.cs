#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Missions
{
    public class MissionDto 
    {
#if NEWTONSOFT_JSON
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("difficulty")] public string Difficulty { get; set; }
        [JsonProperty("hardness")] public int Hardness { get; set; }
        [JsonProperty("bet")] public float Bet { get; set; }
        [JsonProperty("ratio")] public float Ratio { get; set; }
        [JsonProperty("value")] public int Value { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
#else
        
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Difficulty { get; set; }
        public int Hardness { get; set; }
        public float Bet { get; set; }
        public float Ratio { get; set; }
        public int Value { get; set; }
        public int Level { get; set; }
        
#endif
    }
}
