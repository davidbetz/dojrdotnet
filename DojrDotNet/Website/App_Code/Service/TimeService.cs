using System;
using NetFX.Dojo.Rpc;
//+
namespace NetFX.Web
{
    public class TimeService : DojoRpcServiceBase
    {
        //- @GetServerTime -//
        [DojoOperation("getServerTime")]
        public String GetServerTime(String format)
        {
            return DateTime.Now.ToString(format);
        }

        //- @GetServerTimeStamp -//
        [DojoOperation("getServerTimeStamp")]
        public Double GetServerTimeStamp()
        {
            DateTime o = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan d = DateTime.Now - o;
            //+
            return Math.Floor(d.TotalSeconds);
        }
    }
}