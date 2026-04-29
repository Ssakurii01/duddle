using System;
using Luxodd.Game.Scripts.Network.Payloads;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class SendLevelEndCommandHandler : BaseCommandHandler
    {
        public SendLevelEndCommandHandler(WebSocketService webSocketService) : base(webSocketService)
        {
        }

        public override void SendCommand(Action onCommandComplete, params object[] parameters)
        {
            _onCommandCompletedCallback = onCommandComplete;
            //parameters
            //0 - level number
            //1 - score
            //2 - enemies_killed
            //3 - accuracy
            //4 - time_taken
            //5 - completion_percentage
            
            var level = Convert.ToInt32(parameters[0]);
            var score = Convert.ToInt32(parameters[1]);
            //TODO - not used for now
            // var enemiesKilled = Convert.ToInt32(parameters[2]);
            // var accuracy = Convert.ToInt32(parameters[3]);
            // var timeTaken = Convert.ToInt32(parameters[4]);
            // var completionPercentage = Convert.ToInt32(parameters[5]);

            var levelStatisticPayload = new LevelStatisticPayload()
            {
                Level = level,
                Score = score,
                // EnemiesKilled = enemiesKilled,
                // Accuracy = accuracy,
                // TotalSeconds = timeTaken,
                // Progress = completionPercentage
            };
            
            var commandRequest = new CommandRequestJson()
            {
                Type = "level_end",
                Version = "1.0",
                Payload = levelStatisticPayload,
            };

#if NEWTONSOFT_JSON
            var commandRequestJson = JsonConvert.SerializeObject(commandRequest);
            
            WebSocketService.SendCommand(CommandRequestType.LevelEndRequest, commandRequestJson,
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
