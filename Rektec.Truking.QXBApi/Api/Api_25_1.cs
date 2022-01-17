using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;

namespace Rektec.Truking.QXBApi.Api
{
    /// <summary>
    /// 25.1 动产抵押
    /// </summary>
    public class Api_25_1 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_25_1(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type, entityName, organization)
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
                    var number = data.GetStr("number");
                    var date = data.GetStr("date");
                    var status = data.GetStr("status");
                    var department = data.GetStr("department");
                    var amount = data.GetStr("amount");
                    var type = data.GetStr("type");
                    var period = data.GetStr("period");
                    var scope = data.GetStr("scope");
                    var remarks = data.GetStr("remarks");
                    var debit_type = data.GetStr("debit_type");
                    var debit_currency = data.GetStr("debit_currency");
                    var debit_amount = data.GetStr("debit_amount");
                    var debit_scope = data.GetStr("debit_scope");
                    var debit_period = data.GetStr("debit_period");
                    var debit_remarks = data.GetStr("debit_remarks");
                    var close_date = data.GetStr("close_date");
                    var close_reason = data.GetStr("close_reason");

                    var mortgagees = "";//抵押权人信息
                    var mortgageesArray = data["mortgagees"] as JArray;
                    if (mortgageesArray != null && mortgageesArray.Count > 0)
                    {
                        foreach (JObject jo in mortgageesArray)
                        {
                            if (mortgagees == "")
                            {
                                mortgagees += (jo.GetStr("name"));
                            }
                            else
                            {
                                mortgagees += ("," + jo.GetStr("name"));
                            }
                        }
                    }

                    var guarantees = "";//抵押物信息
                    var guaranteesArray = data["guarantees"] as JArray;
                    if (guaranteesArray != null && guaranteesArray.Count > 0)
                    {
                        foreach (JObject jo in guaranteesArray)
                        {
                            if (guarantees == "")
                            {
                                guarantees += (jo.GetStr("name"));
                            }
                            else
                            {
                                guarantees += (","+jo.GetStr("name"));
                            }
                        }
                    }

                    var seq_no = data.GetStr("seq_no");
                    var disabled = data.GetStr("disabled");
                    Entity entity = new Entity(EntityName);
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_number"] = number;
                    entity["new_no"] = seq_no;
                    if (date != "" && date != "-")
                    {
                        entity["new_date"] = Convert.ToDateTime(date);
                    }
                    entity["new_department"] = department;
                    entity["new_amount"] = amount;
                    entity["new_type"] = type;
                    entity["new_period"] = period;
                    entity["new_scope"] = scope;
                    entity["new_remarks"] = remarks;
                    entity["new_debittype"] = debit_type;
                    entity["new_debitcurrency"] = debit_currency;
                    entity["new_debitamount2"] = debit_amount;
                    entity["new_debitscope"] = debit_scope;
                    entity["new_debitperiod"] = debit_period;
                    entity["new_debitremarks"] = debit_remarks;
                    entity["new_closereason"] = close_reason;
                    if (close_date != "" && close_date != "-")
                    {
                        entity["new_closedate"] = Convert.ToDateTime(close_date);
                    }
                    entity["new_disabled"] = new OptionSetValue(Convert.ToInt32(disabled));
                    entity["new_name"] = mortgagees;
                    entity["new_guarantees"] = guarantees;
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("25.1 动产抵押:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
