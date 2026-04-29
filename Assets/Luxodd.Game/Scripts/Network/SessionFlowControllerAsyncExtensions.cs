using System.Threading.Tasks;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network
{
    public static class SessionFlowControllerAsyncExtensions
    {
        public static Task<SessionFlowResult> RunAsync(this SessionFlowController controller)
        {
            var taskCompletionSource = new TaskCompletionSource<SessionFlowResult>();

            void OnSuccess()
            {
                var hasPayload = controller.SessionPayloadIsPresent;
                taskCompletionSource.TrySetResult(hasPayload
                    ? SessionFlowResult.EventPath
                    : SessionFlowResult.LegacyPath);
            }

            void OnError()
            {
                taskCompletionSource.TrySetResult(SessionFlowResult.Error);
            }

            controller.ActivateProcess(OnSuccess, OnError);

            return taskCompletionSource.Task;
        }
    }
}