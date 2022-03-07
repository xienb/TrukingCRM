using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.WinSrv.Job
{
    [DisallowConcurrentExecution]
    public class ProjectmgnJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var conStr = SqliteHelper.GetConfig("CRMConnect");
                CrmServiceClient conn = new CrmServiceClient(conStr);
                IOrganizationService OrganizationServiceAdmin = (IOrganizationService)conn.OrganizationWebProxyClient ??
                                                      conn.OrganizationServiceProxy;
                QueryExpression query = new QueryExpression("new_srv_projectmgn");
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                query.Criteria.AddCondition("new_planenddate", ConditionOperator.NotNull);
                query.Criteria.AddCondition("new_actualenddate", ConditionOperator.Null);
                query.ColumnSet = new ColumnSet("new_planenddate", "new_constructionstatus");
                EntityCollection entitylist = OrganizationServiceAdmin.RetrieveMultiple(query);
                foreach (Entity entity in entitylist.Entities)
                {
                    var new_planenddate = entity.GetAttributeValue<DateTime>("new_planenddate");
                    if (DateTime.Now > new_planenddate)
                    {
                        entity["new_constructionstatus"] = new OptionSetValue(5);
                        OrganizationServiceAdmin.Update(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("ProjectmgnJob", ex);
            }
        }
    }
}
