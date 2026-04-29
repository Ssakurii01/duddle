mergeInto(LibraryManager.library, {
    ConnectWebSocket: function (urlPtr) {
        var url = UTF8ToString(urlPtr);
        var socket = new WebSocket(url);

        window.UnityWebSocket = socket;

        socket.onopen = function () {
            console.log("WebSocket connected to " + url);
            if (typeof unityInstance !== "undefined") {
                unityInstance.SendMessage('WebSocketLibraryWrapper', 'OnWebSocketOpen');
            } else {
                console.error("unityInstance is not defined");
            }
        };

        socket.onmessage = function (event) {
            console.log("Message received: " + event.data);
            if (typeof unityInstance !== "undefined") {
                unityInstance.SendMessage('WebSocketLibraryWrapper', 'OnWebSocketMessage', event.data);
            } else {
                console.error("unityInstance is not defined");
            }
        };

        socket.onclose = function (event) {
            console.log("WebSocket connection closed, code and reason:", event.code, event.reason);
            if (typeof unityInstance !== "undefined") {
                unityInstance.SendMessage('WebSocketLibraryWrapper', 'OnWebSocketClose', event.code);
            } else {
                console.error("unityInstance is not defined");
            }
            window.UnityWebSocket = null;
        };

        socket.onerror = function (error) {
            console.error("WebSocket connection error: ", error);
            if (typeof unityInstance !== "undefined") {
                unityInstance.SendMessage('WebSocketLibraryWrapper', 'OnWebSocketError', error.type);
            } else {
                console.error("unityInstance is not defined");
            }
        };
    },

    SendWebSocketMessage: function (messagePtr) {
        var message = UTF8ToString(messagePtr);
        if (window.UnityWebSocket) {
            console.log("Sending message: " + message);
            window.UnityWebSocket.send(message);
        } else {
            console.error("No active WebSocket connection");
        }
    },

    CloseWebSocket: function () {
        if (window.UnityWebSocket) {
            console.log("Closing WebSocket connection...");
            window.UnityWebSocket.close();
        } else {
            console.error("No active WebSocket connection to close");
        }
    },

    NavigateToHome: function () {
        var currentUrl = window.location.origin;
        var homeUrl = currentUrl + "/home";
        console.log("Navigating to: " + homeUrl);
        window.location.href = homeUrl;
    },

    SendSessionEndMessage: function () {
        console.log("Sending session_end postMessage to parent window");
        window.parent.postMessage({
            type: "session_end"
        }, "*");
    },
	
	SendSessionOptionsMessageWithAction: function (actionPtr) {
        var action = UTF8ToString(actionPtr);
        if (!action) {
            console.warn("SendSessionOptionsMessageWithAction: empty action");
            return;
        }
        console.log("Sending session_options with action:", action);
        window.parent.postMessage({ type: "session_options", action: action }, "*");
    }
});
