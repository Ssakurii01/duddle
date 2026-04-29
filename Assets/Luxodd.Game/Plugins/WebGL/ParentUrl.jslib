mergeInto(LibraryManager.library, {
    GetParentHost: function () {
        var host;

        try {
            host = window.parent.location.host;
        } catch (e) {
            if (document.referrer) {
                var ref = new URL(document.referrer);
                host = ref.host;
            } else {
                host = window.location.hostname + ":8080";
            }
        }

        // --- НОВЫЙ способ вернуть строку в Unity ---
        var length = lengthBytesUTF8(host) + 1;   // +1 для терминального 0
        var buffer = _malloc(length);
        stringToUTF8(host, buffer, length);
        return buffer;
    },

    GetWebSocketProtocol: function () {
        var proto = (window.location.protocol === "https:") ? "wss:" : "ws:";

        var length = lengthBytesUTF8(proto) + 1;
        var buffer = _malloc(length);
        stringToUTF8(proto, buffer, length);
        return buffer;
    }
});