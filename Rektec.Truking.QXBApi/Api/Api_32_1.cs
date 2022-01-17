using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;

namespace Rektec.Truking.QXBApi.Api
{
    public class Api_32_1 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_32_1(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type,entityName, organization)
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
                    var date = data.GetStr("date");
                    var content = data.GetStr("content");
                    var department = data.GetStr("department");
                    var publish_date = data.GetStr("publish_date");
                    var based_on = data.GetStr("based_on");
                    var description = data.GetStr("description");
                    var disabled = data.GetStr("disabled");
                    var illegal_type = data.GetStr("illegal_type");
                    var number = data.GetStr("number");
                    var seq_no = data.GetStr("seq_no");
                    Entity entity = new Entity(EntityName);
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_seqno"] = seq_no;
                    entity["new_name"] = number;
                    entity["new_illegaltype"] = illegal_type;
                    entity["new_content"] = content;
                    entity["new_department"] = department;
                    if (date != "" && date != "-")
                    {
                        entity["new_date"] = Convert.ToDateTime(date);
                    }
                    entity["new_publishdate"] = publish_date;
                    if (based_on != "" && based_on != "-")
                    {
                        entity["new_basedon"] = Convert.ToDateTime(based_on);
                    }
                    entity["new_disabled"] = new OptionSetValue(Convert.ToInt32(disabled));
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("32.1 行政处罚:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
