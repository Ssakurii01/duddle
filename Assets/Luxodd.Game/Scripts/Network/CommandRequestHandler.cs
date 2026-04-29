using System;

namespace Luxodd.Game.Scripts.Network
{
    public class CommandRequestHandler
    {
        public CommandRequestType CommandRequestType { get; set; }
        public int Id { get; set; }
        public CommandResponse CommandResponse { get; set; }
        public string RawResponse { get; set; }
        public Action<CommandRequestHandler> OnCommandResponseSuccessHandler { get; set; }
    }
}