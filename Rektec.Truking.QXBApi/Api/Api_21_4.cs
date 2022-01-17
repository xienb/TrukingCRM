using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;

namespace Rektec.Truking.QXBApi.Api
{
    public class Api_21_4 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_21_4(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type,entityName, organization)
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
                    var id = data.GetStr("id");//明细id
                    var api_21_6 = new NOPageApi(Appkey, Secretkey, QXB.api_21_6,EntityName,org);
                    var qxbRv = api_21_6.ExcuteRequest(id).ToQxbRv();
                    if (qxbRv.status == "200")
                    {
                        var root = qxbRv.data as JObject;
                        var tender = root.GetStr("tender");
                        var date = root.GetStr("date");
                        var agency = root.GetStr("agency");
                        var content = root.GetStr("content");
                        var proj_num = root.GetStr("proj_num");
                        var nace_code = root.GetStr("nace_code");
                        var nace = root.GetStr("nace");
                        var title = root.GetStr("title");
                        var tender_method = root.GetStr("tender_method");
                        var url = root.GetStr("url");
                        var winner = root.GetStr("winner");
                        var location = root.GetStr("location");
                        var type = root.GetStr("type");
                        Entity entity = new Entity(EntityName);
                        entity["new_account_id"] = new EntityReference("account", account.id);
                        entity["new_name"] = title;
                        entity["new_id"] = id;
                        entity["new_url"] = url;
                        entity["new_winner"] = winner;
                        entity["new_tender"] = tender;
                        entity["new_location"] = location;
                        entity["new_type"] = type;
                        entity["new_agency"] = agency;
                        entity["new_content"] = content;
                        if (date != "" && date != "-")
                        {
                            entity["new_date"] = Convert.ToDateTime(date);
                        }
                        org.Create(entity);
                    }
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("21.6招投标详情:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
