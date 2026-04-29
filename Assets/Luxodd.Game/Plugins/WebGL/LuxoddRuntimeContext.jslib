mergeInto(LibraryManager.library, {
  Luxodd_IsMobileRuntime: function () {
    try {
      if (navigator.userAgentData && typeof navigator.userAgentData.mobile === "boolean") {
        return navigator.userAgentData.mobile ? 1 : 0;
      }

      if (window.matchMedia) {
        if (window.matchMedia("(any-pointer: coarse)").matches) return 1;
        if (window.matchMedia("(pointer: coarse)").matches) return 1;
      }

      var mtp = navigator.maxTouchPoints || 0;
      if (mtp > 1) {
        var w = Math.min(window.screen.width || 0, window.innerWidth || 0);
        if (w > 0 && w <= 1366) return 1;
      }

      var ua = (navigator.userAgent || "").toLowerCase();
      var mobile =
        ua.indexOf("mobile") !== -1 ||
        ua.indexOf("android") !== -1 ||
        ua.indexOf("iphone") !== -1 ||
        ua.indexOf("ipad") !== -1 ||
        ua.indexOf("ipod") !== -1;

      return mobile ? 1 : 0;
    } catch (e) {
      return 0;
    }
  },

  // ---------- RAW SIGNALS (for debug only) ----------

  Luxodd_GetMaxTouchPoints: function () {
    try {
      return navigator.maxTouchPoints || 0;
    } catch (e) {
      return 0;
    }
  },
  

  Luxodd_HasCoarsePointer: function () {
    try {
      if (!window.matchMedia) return 0;
      return (
        window.matchMedia("(any-pointer: coarse)").matches ||
        window.matchMedia("(pointer: coarse)").matches
      ) ? 1 : 0;
    } catch (e) {
      return 0;
    }
  },
  
  Luxodd_RegisterViewportChangedCallback: function (goNamePtr, methodNamePtr) {
    try {
      var goName = UTF8ToString(goNamePtr);
      var methodName = UTF8ToString(methodNamePtr);

      function notify() {
        if (typeof SendMessage !== "undefined") {
          SendMessage(goName, methodName, "");
        }
      }

      window.addEventListener("resize", notify);
      window.addEventListener("orientationchange", notify);

      // Mobile browsers often apply final viewport size with delay.
      setTimeout(notify, 0);
      setTimeout(notify, 200);
      setTimeout(notify, 500);

      return 1;
    } catch (e) {
      return 0;
    }
  },
  
  Luxodd_GetVisualViewportInsets: function (
    outLeftPtr,
    outTopPtr,
    outRightPtr,
    outBottomPtr,
    outViewWPtr,
    outViewHPtr
  ) {
    var vv = window.visualViewport;

    var innerW = Math.floor(window.innerWidth);
    var innerH = Math.floor(window.innerHeight);

    var viewW = Math.floor(vv ? vv.width : innerW);
    var viewH = Math.floor(vv ? vv.height : innerH);

    var offX = Math.floor(vv ? vv.offsetLeft : 0);
    var offY = Math.floor(vv ? vv.offsetTop : 0);

    var left = offX;
    var top = offY;
    var right = Math.max(0, innerW - (offX + viewW));
    var bottom = Math.max(0, innerH - (offY + viewH));

    HEAPF32[outLeftPtr >> 2] = left;
    HEAPF32[outTopPtr >> 2] = top;
    HEAPF32[outRightPtr >> 2] = right;
    HEAPF32[outBottomPtr >> 2] = bottom;

    HEAPF32[outViewWPtr >> 2] = viewW;
    HEAPF32[outViewHPtr >> 2] = viewH;
  }

});

