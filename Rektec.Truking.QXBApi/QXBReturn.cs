using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rektec.Truking.QXBApi
{
    /// <summary>
    /// 启信宝通用返回
    /// </summary>
    public class QXBReturn
    {
        public string status { get; set; }
        public string sign { get; set; }
        public string message { get; set; }
        public Object data { get; set; }
    }

    /// <summary>
    /// 分页
    /// </summary>
    public class Page
    {
        public JArray items { get; set; }
        public int total { get; set; }

        public int? num { get; set; }
    }

    /// <summary>
    /// 启信宝分页数据
    /// </summary>
    public class QXBReturnPage
    {
        public string status { get; set; }
        public string sign { get; set; }
        public string message { get; set; }
        public Page data { get; set; }
    }
}
