using System;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public enum CommandSendStatus
    {
        None = 0,
        Pending,
        Send
    }

    public enum CommandResponseStatus
    {
        None = 0,
        Ok,
        Error,
    }

    public abstract class BaseCommandHandler
    {
        protected WebSocketService WebSocketService;
        public CommandSendStatus SendStatus { get; protected set; }
        public CommandResponseStatus ResponseStatus { get; protected set; }
        public int StatusCode {get; protected set;}

        public string ErrorMessage { get; protected set; }

        public CommandResponse ResponseHandler { get; protected set; }
        public string RawResponse { get; protected set; }

        protected System.Action _onCommandCompletedCallback;

        protected BaseCommandHandler(WebSocketService webSocketService)
        {
            WebSocketService = webSocketService;
        }

        public abstract void SendCommand(Action onCommandComplete, params object[] parameters);

        protected virtual void OnCommandResponseSuccessHandler(CommandRequestHandler responseHandler)
        {
            SendStatus = CommandSendStatus.Send;

            RawResponse = responseHandler.RawResponse;

            LoggerHelper.Log(
                $"[{DateTime.Now}][{GetType().Name}][{nameof(OnCommandResponseSuccessHandler)}] OK, raw response: " +
                $"{RawResponse}, status: {responseHandler.CommandResponse.StatusCode}");

            ResponseHandler = responseHandler.CommandResponse;
            StatusCode = responseHandler.CommandResponse.StatusCode;
            ResponseStatus = responseHandler.CommandResponse.StatusCode == 200
                ? CommandResponseStatus.Ok
                : CommandResponseStatus.Error;
        }

        protected void OnCommandResponseFailureHandler(string errorMessage)
        {
            SendStatus = CommandSendStatus.Send;
            ResponseStatus = CommandResponseStatus.Error;
            ErrorMessage = errorMessage;

            _onCommandCompletedCallback?.Invoke();
        }

        public static BaseCommandHandler Build(WebSocketService webSocketService, CommandRequestType commandRequestType)
        {
            switch (commandRequestType)
            {
                case CommandRequestType.GetProfileRequest:
                    return new GetUserProfileRequestCommandHandler(webSocketService);

                case CommandRequestType.GetUserBalanceRequest:
                    return new GetUserBalanceRequestCommandHandler(webSocketService);

                case CommandRequestType.AddBalanceRequest:
                    return new AddBalanceRequestCommandHandler(webSocketService);

                case CommandRequestType.ChargeUserBalanceRequest:
                    return new ChargeUserBalanceRequestCommand(webSocketService);

                case CommandRequestType.GameStatsRequest:
                    break;

                case CommandRequestType.HealthStatusCheckRequest:
                    return new SendHealthStatusCheckCommandHandler(webSocketService);

                case CommandRequestType.LevelBeginRequest:
                    return new SendLevelBeginCommandHandler(webSocketService);

                case CommandRequestType.LevelEndRequest:
                    return new SendLevelEndCommandHandler(webSocketService);
                
                case CommandRequestType.GetUserBestScoreRequest:
                    return new GetUserBestScoreRequestCommandHandler(webSocketService);
                
                case CommandRequestType.GetUserRecentGamesRequest:
                    return new GetUserRecentGamesRequestHandler(webSocketService);
                
                case CommandRequestType.LeaderboardRequest:
                    return new LeaderboardRequestCommandHandler(webSocketService);
                
                case CommandRequestType.SetUserDataRequest:
                    return new SendSetUserDataRequestCommandHandler(webSocketService);
                
                case CommandRequestType.GetUserDataRequest:
                    return new GetUserDataRequestCommandHandler(webSocketService);

                case CommandRequestType.GetGameSessionInfoRequest:
                    return new GetGameSessionInfoRequestCommandHandler(webSocketService);
                
                case CommandRequestType.SendStrategicBettingResultRequest:
                    return new SendStrategicBettingResultCommandHandler(webSocketService);
                
                case CommandRequestType.GetBettingSessionMissionsRequest:
                    return new GetBettingSessionMissionsRequestCommandHandler(webSocketService);
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(commandRequestType), commandRequestType, null);
            }

            return null;
        }
    }
}