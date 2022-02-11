using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Truking.CRM.Web.Model
{
    /// <summary>
    /// 修改域控密码
    /// </summary>
    public class AdModel
    {
        public string userName { get; set; }
        public string currentPassword { get; set; }
        public string newPassword { get; set; }
    }

    /// <summary>
    /// 重置域控密码
    /// </summary>
    public class ResetPwdModel
    {
        public string jobNumber { get; set; }
        public string mobile { get; set; }
        public string sms { get; set; }
        public string newPassword { get; set; }
    }
}