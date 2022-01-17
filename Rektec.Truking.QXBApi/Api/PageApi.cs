using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rektec.Truking.QXBApi.Api
{
    /// <summary>
    /// 分页API基类
    /// </summary>
    public abstract class PageApi : NOPageApi
    {
        public PageApi(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization) : base(appkey, secretkey, type, entityName, organization)
        {
        }

        /// <summary>
        /// 分页处理
        /// </summary>
        /// <param name="account"></param>
        public void Update(AccountInfo account, ILog log = null)
        {
            log?.InfoMsg($"分页处理Update1_{EntityName}");
            DeleteAll(account);
            log?.InfoMsg($"分页处理Update2_{EntityName}");
            log?.InfoMsg($"分页处理Update3_{EntityName}");
            var resp0 = ExcuteRequest(account.creditNo, "0").ToQxbRvPage();
            if (resp0.status == "200")
            {
                Save(account, resp0.data.items);
                //所有分页只取第一页20条，后面的数据不取，如需要，可以把下面代码放出来
                //var total = resp0.data.total;
                //var pageSize = 20;//默认20
                //if (resp0.data.num != null)
                //{
                //    pageSize = resp0.data.num.Value;
                //}
                //var pageCount = (total + pageSize - 1) / pageSize;
                //for (int i = 1; i < pageCount; i++)
                //{
                //    var respi = ExcuteRequest(account.creditNo, i.ToString()).ToQxbRvPage();
                //    if (respi.status == "200")
                //    {
                //        Save(account, respi.data.items);
                //    }
                //}
            }
        }

        /// <summary>
        /// 保存逻辑
        /// </summary>
        /// <param name="account"></param>
        /// <param name="items"></param>
        public abstract void Save(AccountInfo account, JArray items);
    }
}
