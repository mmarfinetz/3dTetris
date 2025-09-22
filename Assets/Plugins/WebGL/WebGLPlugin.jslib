mergeInto(LibraryManager.library, {
    IsMobile: function() {
        return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
    },

    SetCanvasSize: function(width, height) {
        var canvas = document.getElementById('unity-canvas');
        if (canvas) {
            canvas.width = width;
            canvas.height = height;
        }
    },

    ShowAlert: function(str) {
        window.alert(UTF8ToString(str));
    },

    GetWebGLMemorySize: function() {
        return HEAP8.length;
    },

    OpenURLInNewTab: function(url) {
        window.open(UTF8ToString(url), '_blank');
    },

    GetUserAgent: function() {
        var ua = navigator.userAgent;
        var buffer = _malloc(lengthBytesUTF8(ua) + 1);
        writeStringToMemory(ua, buffer);
        return buffer;
    }
});