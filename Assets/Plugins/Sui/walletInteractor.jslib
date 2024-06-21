
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
      window.dispatchReactUnityEvent("checkMasterExist", UTF8ToString(id));
    } catch (e) {
      console.log(e);
    }
  },

  getLands: function () {
    try {
      window.dispatchReactUnityEvent("getLands");
    } catch (e) {
      console.log(e);
    }
  },
  getLand: function (landAddress) {
    try {
      window.dispatchReactUnityEvent("getLand",UTF8ToString(landAddress));
    } catch (e) {
      console.log(e);
    }
  },

  getResidentLicense: function (master) {
    try {
      window.dispatchReactUnityEvent("getResidentLicense",UTF8ToString(master));
    } catch (e) {
      console.log(e);
    }
  },

  registerResidentLicense:function (master, landId) {
    try {
      window.dispatchReactUnityEvent("registerResidentLicense", UTF8ToString(master), UTF8ToString(landId));
    } catch (e) {
      console.log(e);
    }
  },

  unregisterResidentLicense:function (master) {
    try {
      window.dispatchReactUnityEvent("unregisterResidentLicense", UTF8ToString(master));
    } catch (e) {
      console.log(e);
    }
  },

});


