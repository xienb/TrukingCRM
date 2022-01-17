using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;

namespace Rektec.Truking.QXBApi.Api
{
    public class Api_50_5 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_50_5(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type, entityName, organization)
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
                    var logo = data.GetStr("logo");
                    var education = data.GetStr("education");
                    var brief = data.GetStr("brief");
                    var position = data.GetStr("position");
                    var work = data.GetStr("work");
                    var name = data.GetStr("name");
                    Entity entity = new Entity(EntityName);
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_name"] = name;
                    entity["new_position"] = position;
                    entity["new_brief"] = brief;
                    entity["new_education"] = education;
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("50.5 核心团队:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
