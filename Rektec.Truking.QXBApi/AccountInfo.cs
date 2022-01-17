using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rektec.Truking.QXBApi
{
    /// <summary>
    /// 客户信息
    /// </summary>
    public class AccountInfo
    {
        public Guid id { get; set; }

        public string creditNo { get; set; }

        public string name { get; set; }
    }
}
