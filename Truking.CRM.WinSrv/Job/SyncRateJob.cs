using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json.Linq;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.WinSrv.Job
{
    [DisallowConcurrentExecution]
    public class SyncRateJob : IJob
    {
        IOrganizationService OrganizationServiceAdmin;
        public void Execute(IJobExecutionContext context)
        {
            var conStr = SqliteHelper.GetConfig("CRMConnect");
            OrganizationServiceAdmin = new CrmServiceClient(conStr);
            try
            {
                DateTime now = DateTime.Now;
                string reqDate = now.ToString("yyyyMMdd");
                string resp = CommonHelper.GetRate(OrganizationServiceAdmin, reqDate);
                JArray rateList = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(resp);
                if (rateList != null && rateList.Count > 0)
                {
                    foreach (JObject oneRate in rateList)
                    {
                        var FCURR = oneRate.GetStr("FCURR");
                        QueryExpression isExist = new QueryExpression("transactioncurrency");
                        isExist.ColumnSet = new ColumnSet("exchangerate", "isocurrencycode", "transactioncurrencyid");
                        isExist.Criteria.AddCondition("isocurrencycode", ConditionOperator.Equal, FCURR);
                        EntityCollection isExistList = OrganizationServiceAdmin.RetrieveMultiple(isExist);
                        if (isExistList != null && isExistList.Entities.Count > 0)
                        {
                            Entity oneEntity = isExistList[0];
                            oneEntity["exchangerate"] = Convert.ToDecimal(oneRate["UKURS"].ToString());
                            OrganizationServiceAdmin.Update(oneEntity);
                            //Log.Info("SyncRateJob", FCURR + "更新成功");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("SyncRateJob", ex);
            }
        }
    }
}
