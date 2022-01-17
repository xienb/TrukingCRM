using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Truking.CRM.Helper;
using Truking.CRM.Web.Base;
using Truking.CRM.Web.Helper;
using Truking.CRM.Web.Model;

namespace Truking.CRM.Web.Controllers
{
    /// <summary>
    /// 市场活动响应
    /// </summary>
    public class CampaignresponseController : TKApiController
    {
        [HttpPost]
        public WebRv Save(SignModel model)
        {
            WebRv rv = new WebRv();
            try
            {
                if (string.IsNullOrEmpty(model.mstid))
                {
                    rv.code = ResponseCode.parsError;
                    rv.msg = "参数错误";
                }
                else
                {
                    //mstid 对应 codename字段，不是guid
                    QueryExpression isExistMst = new QueryExpression();
                    isExistMst.EntityName = "campaign";
                    isExistMst.Criteria.AddCondition("codename", ConditionOperator.Equal, model.mstid);
                    EntityCollection isExistList = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(isExistMst);
                    if (isExistList != null && isExistList.Entities.Count > 0)
                    {
                        var entity = new Entity("campaignresponse");
                        entity["regardingobjectid"] = new EntityReference("campaign", isExistList.Entities[0].Id);
                        entity["companyname"] = model.corp;
                        entity["lastname"] = model.fullname;
                        entity["telephone"] = model.phone;
                        entity["subject"] = model.fullname+ "_"+ model.corp;
                        if (model.num != null)
                        {
                            entity["new_joinusercount"] = model.num.Value;
                        }
                        OrganizationServiceInstance.Instance.OrgService.Create(entity);
                    }
                    else
                    {
                        rv.code = ResponseCode.failure;
                        rv.msg = "市场代码不存在";
                    }
                }
            }
            catch (Exception ex)
            {
                rv.code = ResponseCode.error;
                rv.msg = ex.Message;
            }
            return rv;
        }

    }
}