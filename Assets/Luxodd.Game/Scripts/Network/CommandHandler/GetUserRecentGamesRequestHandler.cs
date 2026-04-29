using System;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class GetUserRecentGamesRequestHandler : BaseCommandHandler
    {
        public GetUserRecentGamesRequestHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
