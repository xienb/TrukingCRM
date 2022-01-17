using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Truking.CRM.Interface;

namespace Rektec.Truking.QXBApi.Api
{
    public class Api_1_47 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_1_47(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type,entityName, organization)
        {
            org = organization;
            log = logger;
        }
        public override void Save(AccountInfo account, JArray items)
        {
            foreach (JObject data in items)
            {
                try
                {
                    log.InfoMsg("进入变更记录save");
                    var changeDate = data.GetStr("changeDate");//变更日期
                    var changeItem = data.GetStr("changeItem");//变更项目
                    var afterContent = data.GetStr("afterContent");//变更后内容
                    var tag = data.GetStr("tag");//历史信息标签
                    var beforeContent = data.GetStr("beforeContent");//变更前内容
                    Entity entity = new Entity(EntityName);
                    entity["new_name"] = changeItem;
                    entity["new_alterdate"] = Convert.ToDateTime(changeDate);
                    entity["new_beforecontent"] = beforeContent;
                    entity["new_aftercontent"] = afterContent;
                    entity["new_historylabel"] = tag;
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("1.47变更记录:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
