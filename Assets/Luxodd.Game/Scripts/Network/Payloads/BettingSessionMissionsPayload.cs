#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using UnityEngine;

namespace Luxodd.Game.Scripts.Network.Payloads
{
    public class BettingSessionMissionsPayload
    {
#if  NEWTONSOFT_JSON
        [JsonProperty("session_id")] public string SessionId { get; set; }
        [JsonProperty("missions")] public object Missions { get; set; }
#else
        public string SessionId { get; set; }
        public object Missions { get; set; }
#endif
    }
}
