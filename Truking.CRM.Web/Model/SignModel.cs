using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Truking.CRM.Web.Model
{
    public class SignModel
    {
        /// <summary>
        /// 市场活动id
        /// </summary>
        public string mstid { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string corp { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string fullname { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string phone { get; set; }
        /// <summary>
        /// 电子邮件
        /// </summary>
        public string emailaddress { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int? num { get; set; }

    }
}