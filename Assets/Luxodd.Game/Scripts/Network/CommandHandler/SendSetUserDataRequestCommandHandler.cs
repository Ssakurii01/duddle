using System;
using Luxodd.Game.Scripts.Network.Payloads;
#if NEWTONSOFT_JSON   
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class SendSetUserDataRequestCommandHandler : BaseCommandHandler
    {
        public SendSetUserDataRequestCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;

            var data = parameters[0];
            
            var userDataPayload = new UserDataPayload()
            {
                Data = data
            };

            var commandRequest = new CommandRequestJson()
            {
                Type = nameof(CommandRequestType.SetUserDataRequest),
                Payload = userDataPayload
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
