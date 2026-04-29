using Luxodd.Game.HelpersAndUtils.Utils;

namespace Luxodd.Game.Scripts.Network
{
    public class DebugHealthCheckStatusEvent : IEventData
    {
        public bool IsOn { get; set; }
    }
}
