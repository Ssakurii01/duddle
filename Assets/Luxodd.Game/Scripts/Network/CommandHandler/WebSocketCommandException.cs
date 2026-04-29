using System;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network.CommandHandler
{
    public class WebSocketCommandException : Exception
    {
        public int StatusCode { get; private set; }
        public string ServerMessage { get; private set; }

        public WebSocketCommandException(int statusCode, string message)
        : base($"WebSocket command failed. Code: {statusCode}. Message: {message}")
        {
            StatusCode = statusCode;
            ServerMessage = message;
        }
    }
}
