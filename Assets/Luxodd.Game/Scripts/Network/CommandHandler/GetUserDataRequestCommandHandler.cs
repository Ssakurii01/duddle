using System;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using Luxodd.Game.Scripts.Network.Payloads;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using UnityEngine;

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class GetUserDataRequestCommandHandler : BaseCommandHandler
    {
        public GetUserDataRequestCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;
            SendStatus = CommandSendStatus.Pending;

            var commandRequest = new CommandRequestJson()
            {
                Type = nameof(CommandRequestType.GetUserDataRequest),
                Version = "1.0",
            };

#if NEWTONSOFT_JSON
            var commandRequestJson = JsonConvert.SerializeObject(commandRequest);
            commandRequestJson = commandRequestJson.Replace("null", "{}");

            WebSocketService.SendCommand(CommandRequestType.GetUserDataRequest, commandRequestJson,
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
            var payloadObject = JsonConvert.DeserializeObject<UserDataPayload>(payloadJson);
            if (payloadObject != null)
            {
                LoggerHelper.Log(
                    $"[{DateTime.Now}][{GetType().Name}][{nameof(OnCommandResponseSuccessHandler)}] OK, payloadObject: {payloadObject.Data}");
            }

            ResponseHandler.Payload = payloadObject;
#endif

            _onCommandCompletedCallback?.Invoke();
            
        }
    }
}
