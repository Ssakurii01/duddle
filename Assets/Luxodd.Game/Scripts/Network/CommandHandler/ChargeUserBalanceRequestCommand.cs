using System;
using Luxodd.Game.Scripts.HelpersAndUtils;
using Luxodd.Game.Scripts.Network.Payloads;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class ChargeUserBalanceRequestCommand : BaseCommandHandler
    {
        public ChargeUserBalanceRequestCommand(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;

            var amount = Convert.ToInt32(parameters[0]);
            var pinCode = Convert.ToInt32(parameters[1]);

            var pinCodeHash = PinCodeHasher.HashWithKey(pinCode.ToString(), WebSocketService.SessionToken); 

            var amountPayload = new AmountPayload()
            {
                Amount = amount,
                PinCode = pinCodeHash
            };

            var commandRequest = new CommandRequestJson()
            {
                Type = CommandRequestType.ChargeUserBalanceRequest.ToString(),
                Payload = amountPayload
            };

#if NEWTONSOFT_JSON
            var commandRequestJson = JsonConvert.SerializeObject(commandRequest);

            WebSocketService.SendCommand(CommandRequestType.ChargeUserBalanceRequest, commandRequestJson,
                OnCommandResponseSuccessHandler);
            #endif
        }

        protected override void OnCommandResponseSuccessHandler(CommandRequestHandler responseHandler)
        {
            base.OnCommandResponseSuccessHandler(responseHandler);

            _onCommandCompletedCallback?.Invoke();
        }
    }
}