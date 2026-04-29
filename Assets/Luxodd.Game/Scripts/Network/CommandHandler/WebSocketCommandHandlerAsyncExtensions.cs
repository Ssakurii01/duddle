using System.Threading.Tasks;
using Luxodd.Game.Scripts.Game.Leaderboard;
using Luxodd.Game.Scripts.Missions;
using Luxodd.Game.Scripts.Network.Payloads;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public static class WebSocketCommandHandlerAsyncExtensions 
    {
        private static Task WrapAsync(this WebSocketCommandHandler handler,
            System.Action<System.Action, System.Action<int, string>> register)
        {
            var tcs = new TaskCompletionSource<bool>();

            void OnSuccess()
            {
                tcs.SetResult(true);
            }

            void OnFailure(int code, string message)
            {
                tcs.TrySetException(new WebSocketCommandException(code, message));
            }

            register(OnSuccess, OnFailure);
            
            return tcs.Task;
        }

        private static Task<T> WrapAsync<T>(this WebSocketCommandHandler handler,
            System.Action<System.Action<T>, System.Action<int, string>> register)
        {
            var tcs = new TaskCompletionSource<T>();

            void OnSuccess(T result)
            {
                tcs.TrySetResult(result);
            }

            void OnFailure(int code, string message)
            {
                tcs.TrySetException(new WebSocketCommandException(code, message));
            }

            register(OnSuccess, OnFailure);
            
            return tcs.Task;
        }

        public static Task<string> GetProfileAsync(this WebSocketCommandHandler handler)
        {
            return handler.WrapAsync<string>(handler.SendProfileRequestCommand);
        }

        public static Task<float> GetUserBalanceAsync(this WebSocketCommandHandler handler)
        {
            return handler.WrapAsync<float>(handler.SendUserBalanceRequestCommand);
        }

        public static Task SendAddBalanceAsync(this WebSocketCommandHandler handler, int amount, int pinCode)
        {
            return handler.WrapAsync((onSuccess, onFailure) =>
                handler.SendAddBalanceRequestCommand(amount, pinCode,  onSuccess, onFailure));
        }
        
        public static Task ChargeUserBalanceAsync(this WebSocketCommandHandler handler, int amount, int pinCode)
        {
            return handler.WrapAsync((onSuccess, onFailure) =>
                handler.SendChargeUserBalanceRequestCommand(amount, pinCode,  onSuccess, onFailure));
        }

        public static Task HealthCheckAsync(this WebSocketCommandHandler handler)
        {
            return handler.WrapAsync(handler.SendHealthCheckStatusCommand);
        }

        public static Task<LeaderboardDataResponse> GetLeaderboardAsync(this WebSocketCommandHandler handler)
        {
            return handler.WrapAsync<LeaderboardDataResponse>(handler.SendLeaderboardRequestCommand);
        }

        public static Task SendLevelBeginAsync(this WebSocketCommandHandler handler, int level)
        {
            return handler.WrapAsync((onSuccess, onFailure) =>
            {
                handler.SendLevelBeginRequestCommand(level, onSuccess, onFailure);
            });
        }

        public static Task SendLevelEndAsync(this WebSocketCommandHandler handler, int level, int score)
        {
            return handler.WrapAsync((onSuccess, onFailure) =>
            {
                handler.SendLevelEndRequestCommand(level, score, onSuccess, onFailure);
            });
        }

        public static Task<object> GetUserDataAsync(this WebSocketCommandHandler handler)
        {
            return handler.WrapAsync<object>(handler.SendGetUserDataRequestCommand);
        }

        public static Task SendUserDataAsync(this WebSocketCommandHandler handler, object data)
        {
            return handler.WrapAsync((onSuccess, onFailure) =>
            {
                handler.SendSetUserDataRequestCommand(data, onSuccess, onFailure);
            });
        }

        public static Task<SessionInfoPayload> GetGameSessionInfoAsync(this WebSocketCommandHandler handler)
        {
            return handler.WrapAsync<SessionInfoPayload>(handler.SendGetGameSessionInfoRequestCommand);
        }

        public static Task<BettingSessionMissionsPayload> GetBettingSessionMissionsAsync(
            this WebSocketCommandHandler handler)
        {
            return handler.WrapAsync<BettingSessionMissionsPayload>(handler
                .SendGetBettingSessionMissionsRequestCommand);
        }

        public static Task SendStrategicBettingResultAsync(this WebSocketCommandHandler handler,
            System.Collections.Generic.List<MissionResultDto> missionResultList)
        {
            return handler.WrapAsync((onSuccess, onFailure) =>
            {
                handler.SendStrategicBettingResultRequest(missionResultList, onSuccess, onFailure);
            });
        }
    }
}
