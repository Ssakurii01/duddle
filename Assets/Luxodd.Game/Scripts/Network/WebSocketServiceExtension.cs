using System;
using System.Threading.Tasks;

namespace Luxodd.Game.Scripts.Network
{
    public static class WebSocketServiceExtension 
    {
        public static Task ConnectToServerAsync(this WebSocketService webSocketService)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            webSocketService.ConnectToServer(onSuccessCallback: () =>
            {
                tcs.TrySetResult(true);
            },
                onErrorCallback: () =>
                {
                    tcs.TrySetException(new Exception($"WebSocket connection failed"));   
                });
            return tcs.Task;
        }

    }
}
