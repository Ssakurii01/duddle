#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.Payloads
{
    public class ProfilePayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("username")] public string UserName { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("profile_picture")] public string ProfilePicture { get; set; }
#else
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string ProfilePicture { get; set; }
#endif
    }
}