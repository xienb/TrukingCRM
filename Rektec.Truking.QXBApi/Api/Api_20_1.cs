using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Truking.CRM.Interface;
using System;

namespace Rektec.Truking.QXBApi.Api
{
    public class Api_20_1 : PageApi
    {
        IOrganizationService org;
        ILog log;
        public Api_20_1(string appkey, string secretkey, QXB type, string entityName, IOrganizationService organization, ILog logger) : base(appkey, secretkey, type, entityName, organization)
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
                    var overdue_period = data.GetStr("overdue_period");
                    var is_history = data.GetStr("is_history");
                    var pub_department = data.GetStr("pub_department");
                    var taxpayer_type = data.GetStr("taxpayer_type");


                    var pub_date = data.GetStr("pub_date");
                    var area = data.GetStr("area");
                    var address = data.GetStr("address");
                    var oper_name = data.GetStr("oper_name");

                    var taxpayer_num = data.GetStr("taxpayer_num");
                    var overdue_amount = data.GetStr("overdue_amount");
                    var overdue_type = data.GetStr("overdue_type");
                    var oper_id_num = data.GetStr("oper_id_num");
                    var curr_overdue_amount = data.GetStr("curr_overdue_amount");

                    Entity entity = new Entity(EntityName);
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_taxpayertype"] = taxpayer_type;
                    entity["new_taxpayernum"] = taxpayer_num;
                    entity["new_opername"] = oper_name;
                    entity["new_operidnum"] = oper_id_num;
                    entity["new_address"] = address;
                    entity["new_overduetype"] = overdue_type;
                    entity["new_overdueamount2"] = overdue_amount;
                    entity["new_curroverdueamount"] = curr_overdue_amount;
                    entity["new_area"] = area;
                    entity["new_overdueperiod"] = overdue_period;
                    entity["new_pubdepartment"] = pub_department;

                    if (pub_date != "" && pub_date != "-")
                    {
                        entity["new_pubdate"] = Convert.ToDateTime(pub_date);
                    }
                    entity["new_ishistory"] = is_history == "1" ? true : false;
                    org.Create(entity);
                }
                catch (Exception ex)
                {
                    log?.ErrorMsg("20.1 欠税信息:" + ex.Message + ex.StackTrace);
                    continue;
                }
            }
        }
    }
}
