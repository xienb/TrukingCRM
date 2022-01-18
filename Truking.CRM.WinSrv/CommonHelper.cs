using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.WinSrv
{
    public static class CommonHelper
    {
        public static string GetStr(this JObject jo, string key)
        {
            if (jo.Property(key) != null) return jo[key].ToString();
            else return "";
        }

        public static string GetSapOrder(IOrganizationService orgSvr, string jo)
        {
            cn.truking.sappodev.selectsorder.SI_SEL_SORDER_OUTService sapService = new cn.truking.sappodev.selectsorder.SI_SEL_SORDER_OUTService();
            sapService.Url = GetSysPar(orgSvr, "SAP_API_BaseUrl") + "/XISOAPAdapter/MessageServlet?senderParty=&senderService=BS_ERP_S4&receiverParty=&receiverService=&interface=SI_SEL_SORDER_OUT&interfaceNamespace=urn:erp:select_sorder";
            sapService.Credentials = new NetworkCredential(GetSysPar(orgSvr, "SAP_API_UserID"), GetSysPar(orgSvr, "SAP_API_Password"));
            cn.truking.sappodev.selectsorder.DT_SEL_SORDER_REQ req = new cn.truking.sappodev.selectsorder.DT_SEL_SORDER_REQ();
            req.IT_DATA = jo;
            sapService.Timeout = 360000;//6分钟超时
            cn.truking.sappodev.selectsorder.DT_SEL_SORDER_RSP rep = sapService.SI_SEL_SORDER_OUT(req);
            return rep.ET_RETURN;
        }

        public static string GetRate(IOrganizationService orgSvr, string dt)
        {
            cn.truking.sappodev.rate.SI_EXCHANGE_RATE_OUTService sapService = new cn.truking.sappodev.rate.SI_EXCHANGE_RATE_OUTService();
            sapService.Url = GetSysPar(orgSvr, "SAP_API_BaseUrl") + "/XISOAPAdapter/MessageServlet?senderParty=&senderService=BS_CRM&receiverParty=&receiverService=&interface=SI_EXCHANGE_RATE_OUT&interfaceNamespace=urn:crm:exchange_rate";
            sapService.Credentials = new NetworkCredential(GetSysPar(orgSvr, "SAP_API_UserID"), GetSysPar(orgSvr, "SAP_API_Password"));
            cn.truking.sappodev.rate.DT_EXCHANGE_RATE_REQ req = new cn.truking.sappodev.rate.DT_EXCHANGE_RATE_REQ();
            req.IT_DATA = dt;
            sapService.Timeout = 360000;//6分钟超时
            cn.truking.sappodev.rate.DT_EXCHANGE_RATE_RSP rep = sapService.SI_EXCHANGE_RATE_OUT(req);
            return rep.ET_RETURN;
        }

        public static string GetRecognition(IOrganizationService orgSvr, string queryList)
        {
            cn.truking.sappodev.recognition.SI_CUSTOMERRECOGNITION_OUTService sapService = new cn.truking.sappodev.recognition.SI_CUSTOMERRECOGNITION_OUTService();
            sapService.Url = GetSysPar(orgSvr, "SAP_API_BaseUrl") + "/XISOAPAdapter/MessageServlet?senderParty=&senderService=BS_ERP_S4&receiverParty=&receiverService=&interface=SI_CUSTOMERRECOGNITION_OUT&interfaceNamespace=urn:erp:select_customerrecognition";
            sapService.Credentials = new NetworkCredential(GetSysPar(orgSvr, "SAP_API_UserID"), GetSysPar(orgSvr, "SAP_API_Password"));
            cn.truking.sappodev.recognition.DT_SEL_CUSTOMERRECOGNITION_REQ req = new cn.truking.sappodev.recognition.DT_SEL_CUSTOMERRECOGNITION_REQ();
            req.IT_DATA = queryList;
            sapService.Timeout = 360000;//6分钟超时
            cn.truking.sappodev.recognition.DT_SEL_CUSTOMERRECOGNITION_RSP rep = sapService.SI_CUSTOMERRECOGNITION_OUT(req);
            return rep.ET_RETURN;
        }

        public static DateTime? GetSapTime(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str) || str == "00000000")
                {
                    return null;
                }
                else
                {
                    return Convert.ToDateTime(str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2));
                }
            }
            catch
            {
                return null;
            }
        }

        public static string GetSysPar(IOrganizationService organizationService, string parKey)
        {
            QueryExpression query = new QueryExpression("new_systemparameter");
            query.ColumnSet = new ColumnSet("new_value");
            query.Criteria.AddCondition("new_name", ConditionOperator.Equal, parKey);
            EntityCollection entitylist = organizationService.RetrieveMultiple(query);
            if (entitylist != null && entitylist.Entities.Count > 0)
            {
                Entity updateDt = entitylist.Entities[0];
                return updateDt.GetAttributeValue<string>("new_value");
            }
            else
            {
                return "";
            }
        }

        public static DateTime ToDateTime(string ss)
        {
            return Convert.ToDateTime(ss.Substring(0, 4) + "-" + ss.Substring(4, 2) + "-" + ss.Substring(6, 2) + " " + ss.Substring(8, 2) + ":" + ss.Substring(10, 2) + ":" + ss.Substring(12, 2));
        }

        public static DateTime ToDateTime2(string ss)
        {
            return Convert.ToDateTime(ss.Substring(0, 4) + "-" + ss.Substring(4, 2) + "-" + ss.Substring(6, 2));
        }


        public static int? GetPicklistIntValue(IOrganizationService organizationService, string entityName, string attributeName, string picklistValue)
        {
            int? selectedOptionLabel = null;
            var request = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = attributeName,
                RetrieveAsIfPublished = true
            };
            var response = (RetrieveAttributeResponse)organizationService.Execute(request);
            var metadata = (PicklistAttributeMetadata)response.AttributeMetadata;
            var optionList = metadata.OptionSet.Options.ToArray();

            foreach (OptionMetadata option in optionList)
            {
                if (option.Description.UserLocalizedLabel != null && option.Description.UserLocalizedLabel.Label == picklistValue)
                {
                    if (option.Description.UserLocalizedLabel != null)
                    {
                        selectedOptionLabel = option.Value;
                    }
                }
            }
            return selectedOptionLabel;
        }

        /// <summary>
        /// 查询实体的某一个字段，获得该实体的guid
        /// </summary>
        /// <param name="organizationService"></param>
        /// <param name="entityName"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        public static Guid GetLookUpGuidByStringCode(IOrganizationService organizationService, string entityName, string attributeName, string attributeValue)
        {
            QueryExpression isExistMst = new QueryExpression();
            isExistMst.ColumnSet = new ColumnSet(entityName + "id");
            isExistMst.EntityName = entityName;
            isExistMst.Criteria.AddCondition(attributeName, ConditionOperator.Equal, attributeValue);
            EntityCollection isExistList = organizationService.RetrieveMultiple(isExistMst);
            if (isExistList != null && isExistList.Entities.Count == 1)
            {
                return isExistList.Entities[0].Id;
            }
            else
            {
                return Guid.Empty;
            }
        }

    }
}
