using System;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class SendHealthStatusCheckCommandHandler : BaseCommandHandler
    {
        public SendHealthStatusCheckCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;
            var commandRequest = new CommandRequestJson()
            {
                Type = "health_status_check",
                Version = "1.0",
            };
#if NEWTONSOFT_JSON
            var commandRequestJson = JsonConvert.SerializeObject(commandRequest);
            commandRequestJson = commandRequestJson.Replace("null", "{}");
            WebSocketService.SendCommand(CommandRequestType.HealthStatusCheckRequest, commandRequestJson,
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
