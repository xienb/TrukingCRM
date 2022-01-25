using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using Truking.CRM.Web.Helper;

namespace Truking.CRM.Web.Base
{
    /// <summary>
    /// 提供给客户单位调用的接口基类，实现校验逻辑
    /// </summary>
    public class TKAccountApiController : TKApiController
    {
        public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            var httpContext = (HttpContextWrapper)controllerContext.Request.Properties["MS_HttpContext"];
            try
            {
                var sign = httpContext.Request.Headers["sign"];
                var appkey = httpContext.Request.Headers["appkey"];
                var timestamp = httpContext.Request.Headers["timestamp"];
                if (string.IsNullOrEmpty(sign) || string.IsNullOrEmpty(appkey) || string.IsNullOrEmpty(timestamp))
                {
                    WebRv rv = new WebRv();
                    rv.code = ResponseCode.parsError;
                    rv.msg = @"sign\appkey\timestamp不能为空";
                    return Task.Factory.StartNew(() => { return controllerContext.Request.CreateResponse<WebRv>(rv); }, cancellationToken);
                }
                if (!CommonHelper.IsTimeStampRight(timestamp))
                {
                    WebRv rv = new WebRv();
                    rv.code = ResponseCode.parsError;
                    rv.msg = @"时间戳不正确";
                    return Task.Factory.StartNew(() => { return controllerContext.Request.CreateResponse<WebRv>(rv); }, cancellationToken);
                }

                QueryExpression isExistMst = new QueryExpression("new_account_auth");
                isExistMst.ColumnSet = new ColumnSet("new_appkey", "new_secretkey", "new_account_id");
                isExistMst.Criteria.AddCondition("new_appkey", ConditionOperator.Equal, appkey);
                EntityCollection isExistList = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(isExistMst);
                if (isExistList != null && isExistList.Entities.Count > 0)
                {
                    var secretkey = isExistList.Entities[0].GetAttributeValue<string>("new_secretkey");
                    var new_account_id = isExistList.Entities[0].GetAttributeValue<EntityReference>("new_account_id").Id.ToString();
                    var checkSign = EncryptHelper.MD5Encrypt(appkey + timestamp + secretkey);
                    if (checkSign != sign)
                    {
                        WebRv rv = new WebRv();
                        rv.code = ResponseCode.signError;
                        rv.msg = @"签名错误";
                        return Task.Factory.StartNew(() => { return controllerContext.Request.CreateResponse<WebRv>(rv); }, cancellationToken);
                    }
                    else
                    {
                        using (var st = httpContext.Request.InputStream)
                        {
                            if (st.Length > 0)
                            {
                                StreamReader sr = new StreamReader(st);
                                string bodyStr = sr.ReadToEnd();
                                sr.Close();
         
                                byte[] bf = UTF8Encoding.UTF8.GetBytes(bodyStr);
                                controllerContext.Request.Content = new StreamContent(new MemoryStream(bf));
                                httpContext.Request.Headers.Add("new_account_id", new_account_id);
                                controllerContext.Request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                            }
                        }
                        return base.ExecuteAsync(controllerContext, cancellationToken);
                    }          
                }
                else
                {
                    WebRv rv = new WebRv();
                    rv.code = ResponseCode.failure;
                    rv.msg = @"appkey不存在";
                    return Task.Factory.StartNew(() => { return controllerContext.Request.CreateResponse<WebRv>(rv); }, cancellationToken);
                }
            }
            catch (Exception e)
            {
                WebRv rv = new WebRv();
                rv.code = ResponseCode.error;
                rv.msg = e.Message + ":" + e.StackTrace;
                return Task.Factory.StartNew(() => { return controllerContext.Request.CreateResponse<WebRv>(rv); }, cancellationToken);
            }
        }
    }
}