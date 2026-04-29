using Luxodd.Game.HelpersAndUtils.Utils;

namespace Luxodd.Game.Scripts.HelpersAndUtils.Logger
{
    public class DebugLoggerEnableEvent : IEventData
    {
        public bool IsLogEnabled { get; set; }
        public bool IsWarningLogEnabled { get; set; }
    }
    
    public class DebugLoggerSetupEvent : IEventData
    {
        public bool IsLogEnabled { get; set; }
        public bool IsWarningLogEnabled { get; set; }
    }
}
