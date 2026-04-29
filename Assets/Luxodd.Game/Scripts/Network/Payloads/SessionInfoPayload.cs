#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
namespace Luxodd.Game.Scripts.Network.Payloads
{
    public class SessionInfoPayload
    {
#if  NEWTONSOFT_JSON
        [JsonProperty("session_type")] public string SessionType { get; set; }
        [JsonProperty("data")]public object Data { get; set; }
#else
        public string SessionType { get; set; }
        public object Data { get; set; }
#endif
    }
}
