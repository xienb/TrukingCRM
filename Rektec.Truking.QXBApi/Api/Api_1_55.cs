using Microsoft.Xrm.Sdk;
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
    public class Api_1_55 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_1_55(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type,entityName, organization)
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
                    var inReason = data.GetStr("inReason");
                    var inDate = data.GetStr("inDate");
                    var outReason = data.GetStr("outReason");
                    var outDate = data.GetStr("outDate");
                    var department = data.GetStr("department");
                    var disabled = data.GetStr("disabled");
                    Entity entity = new Entity(EntityName); ;
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_name"] = inReason;
                    entity["new_moveindate"] = inDate;
                    entity["new_moveoutreason"] = outReason;
                    if (outDate != "" && outDate != "-")
                    {
                        entity["new_moveoutdate"] = Convert.ToDateTime(outDate);
                    }
                    entity["new_punishdept"] = department;
                    entity["new_disabled"] = new OptionSetValue(Convert.ToInt32(disabled));
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("1.55经营异常:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
