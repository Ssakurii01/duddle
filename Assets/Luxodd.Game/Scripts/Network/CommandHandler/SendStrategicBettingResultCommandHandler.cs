using System;
using System.Collections.Generic;
using Luxodd.Game.Scripts.Missions;
using Luxodd.Game.Scripts.Network.Payloads;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class SendStrategicBettingResultCommandHandler : BaseCommandHandler
    {
        public SendStrategicBettingResultCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;
            
            var missionsResults = parameters[0] as List<MissionResultDto>;

            var strategicBettingResult = new StrategicBettingResultPayload()
            {
                Results = missionsResults,
            };

            var commandRequest = new CommandRequestJson()
            {
                Type = nameof(CommandRequestType.SendStrategicBettingResultRequest),
                Version = "1.0",
                Payload = strategicBettingResult
            };
            
#if NEWTONSOFT_JSON
            
            var commandRequestJson = JsonConvert.SerializeObject(commandRequest);
            WebSocketService.SendCommand(CommandRequestType.SetUserDataRequest, commandRequestJson,
                OnCommandResponseSuccessHandler);
#endif
            
            SendStatus = CommandSendStatus.Pending;
        }

        protected override void OnCommandResponseSuccessHandler(CommandRequestHandler responseHandler)
        {
            base.OnCommandResponseSuccessHandler(responseHandler);
            
            _onCommandCompletedCallback?.Invoke();
        }
    }
}
