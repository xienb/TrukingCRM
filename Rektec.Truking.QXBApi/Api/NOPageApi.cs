using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Rektec.Truking.Http;
using Rektec.Truking.Http.Builder;
using Rektec.Truking.QXBApi.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rektec.Truking.QXBApi.Api
{
    /// <summary>
    /// 企信宝API
    /// </summary>
    public class NOPageApi : ApiBase
    {
        private string _url;
        IOrganizationService org;
        string entityName;

        public string EntityName
        {
            get { return entityName; }
        }
        public NOPageApi(string appkey, string secretkey, QXB type, string en = null, IOrganizationService organization = null) : base(appkey, secretkey)
        {
            _url = ApiUrl.Dic[type];
            org = organization;
            entityName = en;
        }

        /// <summary>
        /// 查询前先删除所有
        /// </summary>
        /// <param name="account"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public bool DeleteAll(AccountInfo account)
        {
            if (org != null && entityName != null)
            {
                QueryExpression delQuery = new QueryExpression(entityName);
                delQuery.Criteria.AddCondition("new_account_id", ConditionOperator.Equal, account.id);
                EntityCollection delList = org.RetrieveMultiple(delQuery);
                if (delList.Entities != null && delList.Entities.Count > 0)
                {
                    var request1 = new ExecuteTransactionRequest();
                    request1.ReturnResponses = true;
                    var requestCollection = new OrganizationRequestCollection();
                    foreach (var delOne in delList.Entities)
                    {
                        requestCollection.Add(new DeleteRequest() { Target = new EntityReference(entityName, delOne.Id) });
                    }
                    request1.Requests = requestCollection;
                    org.Execute(request1);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 通用请求
        /// </summary>
        /// <param name="pars"></param>
        /// <returns></returns>
        public string ExcuteRequest(params string[] pars)
        {
            for (int i = 0; i < pars.Length; i++)
            {
                pars[i] = UrlEncode(pars[i]);
            }
            var apiUlr = string.Format(_url, pars);
            var rb = new RequestBuilder(apiUlr, HttpVerb.Get);
            SyncRespEntity sre1 = SetHeader(rb).GoSync();
            if (sre1.WebEx != null)
            {
                throw sre1.WebEx;
            }
            else
            {
                return sre1.Resp;
            }
        }
    }
}
