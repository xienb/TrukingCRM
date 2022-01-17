using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;

namespace Rektec.Truking.QXBApi.Api
{
    public class Api_20_3 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_20_3(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type, entityName, organization)
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
                    var property = data.GetStr("property");
                    var created_time = data.GetStr("created_time");
                    var finance_officer = data.GetStr("finance_officer");
                    var name = data.GetStr("name");
                    var credit_no = data.GetStr("credit_no");
                    var oper = data.GetStr("oper");
                    var time = data.GetStr("time");
                    var address = data.GetStr("address");
                    var result = data.GetStr("result");
                    var org_no = data.GetStr("org_no");
                    var url = data.GetStr("url");
                    var agent = data.GetStr("agent");
                    var illegal_fact = data.GetStr("illegal_fact");
                    var updated_time = data.GetStr("updated_time");

                    Entity entity = new Entity(EntityName);
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_name"] = name;
                    entity["new_creditno"] = credit_no;
                    entity["new_orgno"] = org_no;
                    entity["new_address"] = address;
                    entity["new_oper"] = oper;
                    entity["new_financeofficer"] = finance_officer;
                    entity["new_agent"] = agent;
                    entity["new_property"] = property;
                    entity["new_illegalfact"] = illegal_fact;
                    entity["new_result"] = result;
                    entity["new_url"] = url;
                    if (time != "" && time != "-")
                    {
                        entity["new_time"] = Convert.ToDateTime(time);
                    }
                    if (created_time != "" && created_time != "-")
                    {
                        entity["new_createdtime"] = Convert.ToDateTime(created_time);
                    }
                    if (updated_time != "" && updated_time != "-")
                    {
                        entity["new_updatedtime"] = Convert.ToDateTime(updated_time);
                    }
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("20.3重大税收违法:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
