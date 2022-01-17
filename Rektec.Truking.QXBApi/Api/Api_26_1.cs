using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;

namespace Rektec.Truking.QXBApi.Api
{
    /// <summary>
    /// 26.1 股权出质
    /// </summary>
    public class Api_26_1 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_26_1(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type, entityName, organization)
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
                    var pawnee = data.GetStr("pawnee");
                    var remark = data.GetStr("remark");
                    var status = data.GetStr("status");
                    var disabled = data.GetStr("disabled");
                    var number = data.GetStr("number");
                    var pledgor_identify_type = data.GetStr("pledgor_identify_type");
                    var pledgor_identify_no = data.GetStr("pledgor_identify_no");
                    var date = data.GetStr("date");
                    var pledgor = data.GetStr("pledgor");
                    var pledgor_amount = data.GetStr("pledgor_amount");
                    var seq_no = data.GetStr("seq_no");
                    var pledgor_currency = data.GetStr("pledgor_currency");
                    var pledgor_unit = data.GetStr("pledgor_unit");
                    var pawnee_identify_no = data.GetStr("pawnee_identify_no");
                    var object_company = data.GetStr("object_company");
                    var pawnee_identify_type = data.GetStr("pawnee_identify_type");

                    var change_items = data["change_items"] as JObject;

                    Entity entity = new Entity(EntityName);
                    entity["new_name"] = number;
                    entity["new_eqno"] = seq_no;
                    entity["new_pledgor"] = pledgor;
                    entity["new_pledgoridentifytype"] = pledgor_identify_type;
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_pledgoridentifyno"] = pledgor_identify_no;
                    entity["new_pledgoramount"] = pledgor_amount;
                    entity["new_pledgorunit"] = pledgor_unit;
                    entity["new_pledgorcurrency"] = pledgor_currency;
                    entity["new_pawnee"] = pawnee;
                    entity["new_pawneeidentifytype"] = pawnee_identify_type;
                    entity["new_pawneeidentifyno"] = pawnee_identify_no;
                    entity["new_status"] = status;
                    if (date != "" && date != "-")
                    {
                        entity["new_date"] = Convert.ToDateTime(date);
                    }
                    entity["new_remark"] = remark;
                    entity["new_hangeitemsseqno"] = change_items.GetStr("seq_no");
                    entity["new_changecontent"] = change_items.GetStr("change_content");
                    if (change_items.GetStr("change_date") != "" && change_items.GetStr("change_date") != "-")
                    {
                        entity["new_changedate"] = Convert.ToDateTime(change_items.GetStr("change_date"));
                    }
                    entity["new_disabled"] = new OptionSetValue(Convert.ToInt32(disabled));
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("26.1 股权出质:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
