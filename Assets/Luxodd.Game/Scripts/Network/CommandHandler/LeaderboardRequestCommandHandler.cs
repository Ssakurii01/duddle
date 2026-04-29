using System;
using Luxodd.Game.Scripts.Game.Leaderboard;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using UnityEngine;

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class LeaderboardRequestCommandHandler : BaseCommandHandler
    {
        public LeaderboardRequestCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;
#if NEWTONSOFT_JSON
            var commandRequest = new CommandRequestJson()
            {
                Type = "leaderboard_request",
                Version = "1.0",
            };

            var commandRequestJson = JsonConvert.SerializeObject(commandRequest);
            commandRequestJson = commandRequestJson.Replace("null", "{}");
            
            WebSocketService.SendCommand(CommandRequestType.LeaderboardRequest, commandRequestJson,
                OnCommandResponseSuccessHandler);
            #endif
            SendStatus = CommandSendStatus.Pending;
        }
        
        protected override void OnCommandResponseSuccessHandler(CommandRequestHandler responseHandler)
        {
            base.OnCommandResponseSuccessHandler(responseHandler);
#if NEWTONSOFT_JSON
            var payloadJson = JsonConvert.SerializeObject(ResponseHandler.Payload);
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnCommandResponseSuccessHandler)}] OK, payload: {payloadJson}");
            var payloadObject = JsonConvert.DeserializeObject<LeaderboardDataResponse>(payloadJson);
            LoggerHelper.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(OnCommandResponseSuccessHandler)}] OK, payloadObject: {payloadObject.CurrentUserData.TotalScore}");

            ResponseHandler.Payload = payloadObject;
#endif
            _onCommandCompletedCallback?.Invoke();
        }
    }
}
