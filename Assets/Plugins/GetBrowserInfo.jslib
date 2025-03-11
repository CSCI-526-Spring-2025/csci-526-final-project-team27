mergeInto(LibraryManager.library, {
  GetBrowserInfo: function(gameObjectNamePtr) {
    var gameObjectName = Pointer_stringify(gameObjectNamePtr);
    // 通过 userAgent 获取浏览器信息
    var browserInfo = navigator.userAgent;
    SendMessage(gameObjectName, "OnBrowserInfoReceived", browserInfo);
  }
});