using System;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using Luxodd.Game.Scripts.Network.Payloads;
using UnityEngine;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class GetGameSessionInfoRequestCommandHandler : BaseCommandHandler
    {
        public GetGameSessionInfoRequestCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;
            SendStatus = CommandSendStatus.Pending;

            var commandRequest = new CommandRequestJson()
            {
                Type = nameof(CommandRequestType.GetGameSessionInfoRequest),
                Version = "1.0"
            };

#if NEWTONSOFT_JSON
            var commandRequestJson = JsonConvert.SerializeObject(commandRequest);
            commandRequestJson = commandRequestJson.Replace("null", "{}");


            WebSocketService.SendCommand(CommandRequestType.GetGameSessionInfoRequest, commandRequestJson,
                OnCommandResponseSuccessHandler);
#endif
        }

        protected override void OnCommandResponseSuccessHandler(CommandRequestHandler responseHandler)
        {
            base.OnCommandResponseSuccessHandler(responseHandler);
#if NEWTONSOFT_JSON
            var payloadJson = JsonConvert.SerializeObject(ResponseHandler.Payload);
            LoggerHelper.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(OnCommandResponseSuccessHandler)}] OK, payload: {payloadJson}");

            var payloadObject = JsonConvert.DeserializeObject<SessionInfoPayload>(payloadJson);
            if (payloadObject != null)
            {
                Debug.Log(
                    $"[{DateTime.Now}][{GetType().Name}][{nameof(OnCommandResponseSuccessHandler)}] OK, session type: {payloadObject.SessionType}");
            }

            ResponseHandler.Payload = payloadObject;
#endif

            _onCommandCompletedCallback?.Invoke();
        }
    }
}