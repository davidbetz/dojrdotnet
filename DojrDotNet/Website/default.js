var Service = {
    callback: function(r) {
        alert(r);
    }
};

var calcProxy = new dojo.rpc.JsonService('json/calc/?smd');
var timeProxy = new dojo.rpc.JsonService('json/time/?smd');

calcProxy.add(2, 3).addCallback(Service.callback);

timeProxy.getServerTime('yyyy').addCallback(Service.callback);
timeProxy.getServerTimeStamp( ).addCallback(Service.callback);