using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rektec.Truking.QXBApi.Api
{
    public class Api_1_49 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_1_49(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type,entityName, organization)
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
                    var name = data.GetStr("name");
                    var operName = data.GetStr("operName");
                    var eid = data.GetStr("eid");
                    var status = data.GetStr("status");
                    var belongOrg = data.GetStr("belongOrg");
                    var startDate = data.GetStr("startDate");
                    var registCapi = data.GetStr("registCapi");

                    Entity entity = new Entity(EntityName);
                    entity["new_name"] = name;
                    entity["new_opername"] = operName;
                    entity["new_eid"] = eid;
                    entity["new_status"] = status;
                    entity["new_belongorg"] = belongOrg;
                    if (startDate != "" && startDate != "-")
                    {
                        entity["new_startdate"] = Convert.ToDateTime(startDate);
                    }
                    entity["new_registcapi2"] = registCapi;
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("1.49分支机构:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
