using System.Collections.Generic;
using Luxodd.Game.Scripts.Missions;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.Payloads
{
    public class StrategicBettingResultPayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("results")] public List<MissionResultDto> Results { get; set; }
#else
        public List<MissionResultDto> Results { get; set; }
#endif
    }
}
