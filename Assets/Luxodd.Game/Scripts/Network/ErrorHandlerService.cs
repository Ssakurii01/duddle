using UnityEngine;

namespace Luxodd.Game.Scripts.Network
{
    public enum ErrorReason
    {
        None,
        ConnectionError,
        CreditsError,
        GameError,
    }
    
    public class ErrorHandlerService : MonoBehaviour
    {
        [SerializeField] private WebSocketService _webSocketService;
        
        public void HandleError(string message, string error)
        {
            _webSocketService.BackToSystemWithError(message, error);
        }

        public void HandleConnectionError(string message)
        {
            _webSocketService.BackToSystemWithError(message, ErrorReason.ConnectionError.ToString());
        }

        public void HandleCreditsError(string message)
        {
            _webSocketService.BackToSystemWithError(message, ErrorReason.CreditsError.ToString());
        }

        public void HandleGameError(string message)
        {
            _webSocketService.BackToSystemWithError(message, ErrorReason.GameError.ToString());
        }
    }
}
