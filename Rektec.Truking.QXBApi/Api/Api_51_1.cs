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
    public class Api_51_1 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_51_1(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type, entityName, organization)
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
                    var document_no = data.GetStr("document_no");//
                    var punishment_date = data.GetStr("punishment_date");//
                    var punishment_dept = data.GetStr("punishment_dept");//
                    var disabled = data.GetStr("disabled");//
                    var id = data.GetStr("id");//
                    var punishment_type = data.GetStr("punishment_type");//
                    Entity entity = new Entity(EntityName);
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_documentno"] = document_no;
                    entity["new_punishmentdate"] = punishment_date;
                    entity["new_punishmentdept"] = punishment_dept;
                    entity["new_punishmenttype"] = punishment_type;
                    entity["new_id"] = id;
                    entity["new_disabled"] = new OptionSetValue(Convert.ToInt32(disabled));
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("51.1环保处罚列表:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
