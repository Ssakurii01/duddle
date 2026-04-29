#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network
{
    public class UserBalancePayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("balance")] public float Balance { get; set; }
#else
        public float Balance { get; set; }
#endif
    }
}