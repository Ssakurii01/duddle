mergeInto(LibraryManager.library, {
    InitLuxoddSessionListener: function (goNamePtr) {
        //Store state in в window.__LuxoddSessionState
        var S = window.__LuxoddSessionState || (window.__LuxoddSessionState = {
            listenerAdded: false,
            targetGO: "LuxoddSessionBridge",
            token: null,
            wsUrl: null
        });

        // Name of the GameObject from Unity
        try {
            var goName = UTF8ToString(goNamePtr || 0);
            if (goName) {
                S.targetGO = goName;
            }
        } catch (e) {
            console.warn("[LuxoddSession] Failed to read GO name:", e);
        }

        if (S.listenerAdded) {
            return; // already initialized
        }
        S.listenerAdded = true;

        function sendToUnity(method, arg) {
            if (typeof unityInstance !== "undefined") {
                // arg should be string
                unityInstance.SendMessage(S.targetGO, method, arg || "");
            } else {
                console.warn("[LuxoddSession] unityInstance is not defined, drop", method, arg);
            }
        }

        function onLuxoddSession(e) {
            try {
                var d = e && e.detail;
                if (!d || typeof d !== "object") {
                    console.warn("[LuxoddSession] invalid event.detail:", d);
                    return;
                }

                if (typeof d.token === "string") {
                    S.token = d.token;
                } else {
                    S.token = null;
                }

                if (typeof d.wsUrl === "string") {
                    S.wsUrl = d.wsUrl;
                } else {
                    S.wsUrl = null;
                }

                var payload = JSON.stringify({
                    token: S.token,
                    wsUrl: S.wsUrl
                });

                console.log("[LuxoddSession] received session:", payload);
                sendToUnity("OnLuxoddSession", payload);
            } catch (err) {
                console.error("[LuxoddSession] error in onLuxoddSession:", err);
            }
        }

        window.addEventListener("luxodd:session", onLuxoddSession);
        console.log("[LuxoddSession] listener added for 'luxodd:session'");
    },

    GetLuxoddSessionToken: function () {
        var S = window.__LuxoddSessionState || {};
        var token = S.token || "";
        var size = lengthBytesUTF8(token) + 1;
        var buffer = _malloc(size);
        stringToUTF8(token, buffer, size);
        return buffer;
    },

    GetLuxoddSessionWsUrl: function () {
        var S = window.__LuxoddSessionState || {};
        var wsUrl = S.wsUrl || "";
        var size = lengthBytesUTF8(wsUrl) + 1;
        var buffer = _malloc(size);
        stringToUTF8(wsUrl, buffer, size);
        return buffer;
    }
});