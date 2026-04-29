using System;
using Luxodd.Game.Scripts.Network.Payloads;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class SendLevelBeginCommandHandler : BaseCommandHandler
    {
        public SendLevelBeginCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;
            //parameters
            //0 - level number
            
            var level = Convert.ToInt32(parameters[0]);

            var levelStatisticPayload = new LevelStatisticPayload()
            {
                Level = level,
            };
            
            var commandRequest = new CommandRequestJson()
            {
                Type = "level_begin",
                Version = "1.0",
                Payload = levelStatisticPayload,
            };

#if NEWTONSOFT_JSON
            var commandRequestJson = JsonConvert.SerializeObject(commandRequest);
            
            WebSocketService.SendCommand(CommandRequestType.LevelBeginRequest, commandRequestJson,
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