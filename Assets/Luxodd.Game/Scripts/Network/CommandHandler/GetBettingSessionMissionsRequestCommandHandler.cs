using System;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using Luxodd.Game.Scripts.Network.Payloads;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

using UnityEngine;

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class GetBettingSessionMissionsRequestCommandHandler : BaseCommandHandler
    {
        public GetBettingSessionMissionsRequestCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;
            SendStatus = CommandSendStatus.Pending;

            var commandRequest = new CommandRequestJson()
            {
                Type = nameof(CommandRequestType.GetBettingSessionMissionsRequest)
            };
            
#if NEWTONSOFT_JSON
            var commandRequestJson = JsonConvert.SerializeObject(commandRequest);

            WebSocketService.SendCommand(CommandRequestType.GetBettingSessionMissionsRequest, commandRequestJson,
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

            var payloadObject = JsonConvert.DeserializeObject<BettingSessionMissionsPayload>(payloadJson);
            if (payloadObject != null)
            {
                Debug.Log(
                    $"[{DateTime.Now}][{GetType().Name}][{nameof(OnCommandResponseSuccessHandler)}] OK, session type: {payloadObject.SessionId}");
            }

            ResponseHandler.Payload = payloadObject;
#endif
            
            _onCommandCompletedCallback?.Invoke();
        }
    }
}
