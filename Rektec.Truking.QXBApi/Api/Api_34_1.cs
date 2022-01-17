using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;

namespace Rektec.Truking.QXBApi.Api
{
    /// <summary>
    /// 34.1 股权冻结
    /// </summary>
    public class Api_34_1 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_34_1(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type, entityName, organization)
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
                    var amount = data.GetStr("amount");
                    var be_executed_person = data.GetStr("be_executed_person");
                    var status = data.GetStr("status");
                    var disabled = data.GetStr("disabled");
                    var eid = data.GetStr("eid");
                    var type = data.GetStr("type");
                    var seq_no = data.GetStr("seq_no");
                    var number = data.GetStr("number");
                    var executive_court = data.GetStr("executive_court");

                    var lose_efficacy = data["lose_efficacy"] as JObject;
                    var detail = data["detail"] as JObject;

                    Entity entity = new Entity(EntityName);
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_name"] = number;
                    entity["new_type"] = type;
                    entity["new_amount_2"] = amount;
                    entity["new_beexecutedperson"] = be_executed_person;
                    entity["new_executivecourt"] = executive_court;
                    entity["new_eid"] = eid;
                    entity["new_disabled"] = new OptionSetValue(Convert.ToInt32(disabled));
                    if (lose_efficacy != null)
                    {
                        if (lose_efficacy.GetStr("date") != "" && lose_efficacy.GetStr("date") != "-")
                        {
                            entity["new_date"] = Convert.ToDateTime(lose_efficacy.GetStr("date"));
                        }
                        entity["new_reason"] = lose_efficacy.GetStr("reason");
                    }
                    if (detail != null)
                    {
                        entity["new_corpname1"] = detail.GetStr("corp_name");
                        entity["new_noticeno1"] = detail.GetStr("notice_no");
                        entity["new_assistitem2"] = detail.GetStr("assist_item");
                        if (detail.GetStr("public_date") != "" && detail.GetStr("public_date") != "-")
                        {
                            entity["new_publicdate1"] = Convert.ToDateTime(detail.GetStr("public_date"));
                        }
                        entity["new_assistidenttype1"] = detail.GetStr("assist_ident_type");
                        if (detail.GetStr("freeze_start_date") != "" && detail.GetStr("freeze_start_date") != "-")
                        {
                            entity["new_freezestartdate"] = Convert.ToDateTime(detail.GetStr("freeze_start_date"));
                        }
                        entity["new_detail_freeze_amount"] = detail.GetStr("freeze_amount");
                        entity["new_freezeyearmonth"] = detail.GetStr("freeze_year_month");
                        entity["new_assistname1"] = detail.GetStr("assist_name");
                        entity["new_assistidentno1"] = detail.GetStr("assist_ident_no");
                        if (detail.GetStr("freeze_end_date") != "" && detail.GetStr("freeze_end_date") != "-")
                        {
                            entity["new_freezeenddate"] = Convert.ToDateTime(detail.GetStr("freeze_end_date"));
                        }
                        entity["new_executecourt1"] = detail.GetStr("execute_court");
                        entity["new_adjudicateno1"] = detail.GetStr("adjudicate_no");
                        entity["new_detaileid"] = detail.GetStr("eid");
                    }
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("34.1 股权冻结:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
