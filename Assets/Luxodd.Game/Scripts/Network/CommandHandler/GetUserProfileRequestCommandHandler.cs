using System;
using Luxodd.Game.Scripts.Network.Payloads;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class GetUserProfileRequestCommandHandler : BaseCommandHandler
    {
        public GetUserProfileRequestCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;
            var commandRequest = new CommandRequestJson()
            {
                Type = CommandRequestType.GetProfileRequest.ToString()
            };
#if NEWTONSOFT_JSON
            var commandRequestJson = JsonConvert.SerializeObject(commandRequest);
            WebSocketService.SendCommand(CommandRequestType.GetProfileRequest, commandRequestJson,
                OnCommandResponseSuccessHandler);
            #endif
            SendStatus = CommandSendStatus.Pending;
        }

        protected override void OnCommandResponseSuccessHandler(CommandRequestHandler responseHandler)
        {
            base.OnCommandResponseSuccessHandler(responseHandler);
#if NEWTONSOFT_JSON
            var payloadJson = JsonConvert.SerializeObject(ResponseHandler.Payload);
            var payloadObject = JsonConvert.DeserializeObject<ProfilePayload>(payloadJson);
            ResponseHandler.Payload = payloadObject;
            #endif

            _onCommandCompletedCallback?.Invoke();
        }
    }
}
