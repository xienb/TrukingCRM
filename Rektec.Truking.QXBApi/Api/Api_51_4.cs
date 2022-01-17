using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;

namespace Rektec.Truking.QXBApi.Api
{
    public class Api_51_4 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_51_4(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type, entityName, organization)
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
                    var app_name_eid = data.GetStr("app_name_eid");
                    var id = data.GetStr("id");
                    var name = data.GetStr("name");
                    var disabled = data.GetStr("disabled");
                    var case_kind = data.GetStr("case_kind");
                    var court = data.GetStr("court");
                    var management_agency_eid = data.GetStr("management_agency_eid");
                    var pub_date = data.GetStr("pub_date");
                    var case_no = data.GetStr("case_no");
                    var agency_principal = data.GetStr("agency_principal");
                    var name_eid = data.GetStr("name_eid");
                    var app_name = data.GetStr("app_name");
                    var management_agency = data.GetStr("management_agency");
                    Entity entity = new Entity(EntityName);
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_id"] = id;
                    entity["new_caseno"] = case_no;
                    entity["new_casekind"] = new OptionSetValue(Convert.ToInt32(case_kind));
                    entity["new_court"] = court;
                    if (pub_date != "" && pub_date != "-")
                    {
                        entity["new_pubdate"] = Convert.ToDateTime(pub_date);
                    }
                    entity["new_appname"] = app_name;
                    entity["new_appnameeid"] = app_name_eid;
                    entity["new_name"] = name;
                    entity["new_nameeid"] = name_eid;
                    entity["new_managementagency"] = management_agency;
                    entity["new_management"] = management_agency_eid;
                    entity["new_agencyprincipal"] = agency_principal;
                    entity["new_disabled"] = new OptionSetValue(Convert.ToInt32(disabled));
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("51.4 破产信息:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
