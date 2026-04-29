#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Missions
{
    public class MissionResultDto
    {
#if NEWTONSOFT_JSON
        [JsonProperty("mission_id")] public string MissionId { get; set; }
        [JsonProperty("outcome")] public string Outcome { get; set; }
#else
        public string MissionId { get; set; }
        public string Outcome { get; set; }
#endif
    }
}
