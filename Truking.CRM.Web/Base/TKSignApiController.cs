using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Truking.CRM.Web.Helper;

namespace Truking.CRM.Web.Base
{
    /// <summary>
    /// 签名及时间戳校验  防止重复提交等实现
    /// </summary>
    public class TKSignApiController : ApiController
    {
        public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            var action = controllerContext.RouteData.Values["action"].ToString().ToLower();
            List<string> actionlist = new List<string>() { "ofd2image", "ofd2pdf" };//部分接口特殊处理
            if (!actionlist.Contains(action))
            {
                var httpContext = (HttpContextWrapper)controllerContext.Request.Properties["MS_HttpContext"];
                try
                {
                    var sign = httpContext.Request.Headers["sign"];
                    string source = httpContext.Request.Headers["source"];//pb特殊处理
                    var appkey = httpContext.Request.Params["appkey"];
                    var timestamp = httpContext.Request.Params["timestamp"];
                    if (string.IsNullOrEmpty(sign) || string.IsNullOrEmpty(timestamp))
                    {
                        WebRv rv1 = new WebRv();
                        rv1.code = ResponseCode.parsError;
                        rv1.msg = @"sign\timestamp不能为空";
                        return Task.Factory.StartNew(() => { return controllerContext.Request.CreateResponse<WebRv>(rv1); }, cancellationToken);
                    }
                    var checkSign = EncryptHelper.MD5Encrypt(appkey + "" + timestamp);
                    if (checkSign != sign)
                    {
                        WebRv rv1 = new WebRv();
                        rv1.code = ResponseCode.failure;
                        rv1.msg = @"签名验证失败";
                        return Task.Factory.StartNew(() => { return controllerContext.Request.CreateResponse<WebRv>(rv1); }, cancellationToken);
                    }
                    using (var st = httpContext.Request.InputStream)
                    {
                        if (st.Length > 0)
                        {
                            StreamReader sr = new StreamReader(st);
                            string bodyStr = sr.ReadToEnd();
                            sr.Close();
                            var bodySource = "";
                            if (source == "pb")
                            {
                                bodySource = bodyStr;
                            }
                            else
                            {
                                bodySource = CommonHelper.AesDecrypt(bodyStr, sign.Substring(0, 16));
                            }

                            byte[] bf = UTF8Encoding.UTF8.GetBytes(bodySource);
                            controllerContext.Request.Content = new StreamContent(new MemoryStream(bf));
                            controllerContext.Request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        }
                    }
                    return base.ExecuteAsync(controllerContext, cancellationToken);
                }

                catch (Exception e)
                {
                    WebRv rv1 = new WebRv();
                    rv1.code = ResponseCode.error;
                    rv1.msg = e.Message + ":" + e.StackTrace;
                    return Task.Factory.StartNew(() => { return controllerContext.Request.CreateResponse<WebRv>(rv1); }, cancellationToken);
                }
            }
            else
            {
                return base.ExecuteAsync(controllerContext, cancellationToken);
            }
        }
    }
}