using System;
using NetFX.Dojo.Rpc;
//+
namespace NetFX.Web
{
    public class CalculatorService : DojoRpcServiceBase
    {
        //- @Add -//
        [DojoOperation("add")]
        public Int32 Add(Int32 n1, Int32 n2)
        {
            return n1 + n2;
        }

        //- @Subtract -//
        [DojoOperation("subtract")]
        public Int32 Subtract(Int32 n1, Int32 n2)
        {
            return n1 - n2;
        }
    }
}