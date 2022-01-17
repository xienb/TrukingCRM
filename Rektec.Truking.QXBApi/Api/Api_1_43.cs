using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System;
using Truking.CRM.Interface;

namespace Rektec.Truking.QXBApi.Api
{
    public class Api_1_43 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_1_43(string appkey, string secretkey, QXB type,string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type, entityName, organization)
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
                    var name = data.GetStr("name");//股东姓名
                    var stockType = data.GetStr("stockType");//股东类型
                    var identifyType = data.GetStr("identifyType");//类型
                    var identifyNo = data.GetStr("identifyNo");//企业证照号
                    var totalShouldCapi = data.GetStr("totalShouldCapi");//总认缴
                    var totalRealCapi = data.GetStr("totalRealCapi");//总实缴
                    var shouldCapiItems = ArrayToStr(data["shouldCapiItems"] as JArray);//认缴出资列表
                    var realCapiItems = ArrayToStr(data["realCapiItems"] as JArray);//实缴出资列表
                    Entity entity = new Entity(EntityName);
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_name"] = name;
                    entity["new_shareholdertype"] = stockType;
                    entity["new_certificatetype"] = identifyType;
                    entity["new_certificatecode"] = identifyNo;
                    entity["new_capitalcontributions"] = shouldCapiItems;
                    entity["new_paidincapital"] = realCapiItems;
                    entity["new_totalshouldcapi"] = totalShouldCapi;
                    entity["new_shareholderpaid"] = totalRealCapi;
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("1.43工商股东:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }

        /// <summary>
        /// 转字符串
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private string ArrayToStr(JArray array)
        {
            string str = "";
            foreach (JObject jo in array)
            {
                if (string.IsNullOrEmpty(str))
                {
                    str += (string.Format("出资时间：{0}，出资额：{1},出资方式：{2}", jo.GetStr("date"), jo.GetStr("capi"), jo.GetStr("type")));
                }
                else
                {
                    str += ("；" + string.Format("出资时间：{0}，出资额：{1},出资方式：{2}", jo.GetStr("date"), jo.GetStr("capi"), jo.GetStr("type")));
                }
            }
            return str;
        }
    }
}
