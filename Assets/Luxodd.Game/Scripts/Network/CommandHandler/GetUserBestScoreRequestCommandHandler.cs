using System;

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class GetUserBestScoreRequestCommandHandler : BaseCommandHandler
    {
        public GetUserBestScoreRequestCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
