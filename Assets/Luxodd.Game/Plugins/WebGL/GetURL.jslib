mergeInto(LibraryManager.library, {
 
    GetURLFromQueryStr: function () {
        var queryStr = window.location.search; // only query string
        var bufferSize = lengthBytesUTF8(queryStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(queryStr, buffer, bufferSize);
        return buffer;
    },
	
	ReceiveJWTFromParent: function () {
        window.addEventListener('message', function (event) {
            //
			console.log("receive message event: " + event.data);
			
            var jwt = event.data.jwt; // get token
            if (jwt) {
                var bufferSize = lengthBytesUTF8(jwt) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(jwt, buffer, bufferSize);
                Module.ccall('OnReceiveJWT', null, ['number'], [buffer]); // call unity method
            }
        });
    },
	
	NotifyParentGameReady: function () {
		console.log("Send gameReady message");
        window.parent.postMessage({ type: 'gameReady' }, '*');
    }
});