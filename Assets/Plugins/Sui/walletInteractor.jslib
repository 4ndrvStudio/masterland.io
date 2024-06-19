
mergeInto(LibraryManager.library, {

  getAddress: function () {
    try {
      window.dispatchReactUnityEvent("getAddress");
    } catch (e) {
      console.log(e);
    }
  },

  mintMaster: function () {
    try {
      window.dispatchReactUnityEvent("mintMaster");
    } catch (e) {
      console.log(e);
    }
  },
  
  getMasters: function () {
    try {
      window.dispatchReactUnityEvent("getMasters");
    } catch (e) {
      console.log(e);
    }
  },

  checkMasterExist: function (id) {
    try {
      window.dispatchReactUnityEvent("checkMasterExist",UTF8ToString(id));
    } catch (e) {
      console.log(e);
    }
  },
});


