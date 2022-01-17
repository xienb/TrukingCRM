using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Truking.CRM.Web
{
    public enum ResponseCode
    {
        success=0,
        failure=1,
        parsError=2,//参数错误
        signError=3,//签名错误
        error=4 //程序错误
    }
    public class WebRv
    {
        public WebRv()
        {
            code = ResponseCode.success;
        }
        public ResponseCode code { get; set; }
        public string msg { get; set; }
        public object data { get; set; }
    }
}