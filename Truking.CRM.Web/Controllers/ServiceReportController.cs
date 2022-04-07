using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Truking.CRM.Web.Base;
using Truking.CRM.Web.Helper;

namespace Truking.CRM.Web.Controllers
{
    public class SVRFileInfo
    {
        public string Id { get; set; }
        public string filename { get; set; }
    }

    /// <summary>
    /// 服务报告
    /// </summary>
    public class ServiceReportController : TKController
    {
        [HttpPost]
        public JsonResult GetWorkOrd(string id)
        {
            var org = OrganizationServiceInstance.Instance.OrgService;
            var new_srv_workorder = org.Retrieve("new_srv_workorder", new Guid(id), new ColumnSet("new_name", "new_type", "new_accepttime", "new_projectmgn_id", "new_projecttype", "new_srv_projectstage_id", "new_country_id", "new_srv_worker_id", "new_phone", "new_customerid", "new_contact", "new_feedbacktel", "new_memo", "new_longrange", "new_productgroup_id", "new_productmodel_id", "new_estimate", "new_begintime", "new_endtime"));
            object new_srv_workerObj = new object();
            if (new_srv_workorder.Contains("new_srv_worker_id"))
            {
                new_srv_workerObj = org.Retrieve("new_srv_worker", new_srv_workorder.GetAttributeValue<EntityReference>("new_srv_worker_id").Id, new ColumnSet("new_phone"));
            }
            QueryExpression qe1 = new QueryExpression("new_srv_siteservicecontent");
            qe1.ColumnSet = new ColumnSet("new_name", "new_treatmentmethod");
            qe1.Criteria.AddCondition("new_srv_workorder_id", ConditionOperator.Equal, new_srv_workorder.Id);
            var ec1 = org.RetrieveMultiple(qe1);

            QueryExpression isExistMst = new QueryExpression("annotation");
            isExistMst.ColumnSet = new ColumnSet("filename");
            isExistMst.Criteria.AddCondition("objectid", ConditionOperator.Equal, new_srv_workorder.Id);
            isExistMst.Criteria.AddCondition("isdocument", ConditionOperator.Equal, true);
            isExistMst.Criteria.AddCondition("mimetype", ConditionOperator.Equal, "image/jpeg");
            EntityCollection isExistList = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(isExistMst);
            List<SVRFileInfo> fileList = new List<SVRFileInfo>();
            if (isExistList != null && isExistList.Entities.Count > 0)
            {
                foreach (var en in isExistList.Entities)
                {
                    fileList.Add(new SVRFileInfo()
                    {
                        Id = en.Id.ToString(),
                        filename = en.GetAttributeValue<string>("filename")
                    });
                }
            }
            var rv = new
            {
                model = new_srv_workorder,
                new_srv_worker = new_srv_workerObj,
                new_srv_siteservicecontent = ec1.Entities,
                files = fileList
            };
            return Json(rv);
        }
    }
}