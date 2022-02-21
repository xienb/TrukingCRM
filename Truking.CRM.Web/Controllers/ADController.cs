using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rektec.Truking.Http;
using Rektec.Truking.Http.Builder;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using Truking.CRM.Web.Base;
using Truking.CRM.Web.Helper;
using Truking.CRM.Web.Model;
using Truking.CRM.Web.sms;

namespace Truking.CRM.Web.Controllers
{
    public class ADController : TKApiController
    {
        [HttpPost]
        public WebRv ChangeMyPassword(AdModel model)
        {
            WebRv rv = new WebRv();
            try
            {
                string ldapPath = "LDAP://172.16.200.11";
                DirectoryEntry directionEntry = new DirectoryEntry(ldapPath, "truking" + "\\" + model.userName, model.currentPassword);
                if (directionEntry != null)

                {
                    DirectorySearcher search = new DirectorySearcher(directionEntry);
                    search.Filter = "(SAMAccountName=" + model.userName + ")";
                    SearchResult result = search.FindOne();
                    if (result != null)
                    {
                        DirectoryEntry userEntry = result.GetDirectoryEntry();
                        if (userEntry != null)
                        {
                            userEntry.Invoke("ChangePassword", new object[] { model.currentPassword, model.newPassword });
                            userEntry.CommitChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                rv.code = ResponseCode.failure;
                rv.msg = ex.Message;
            }
            return rv;
        }

        [HttpPost]
        public WebRv SendSMS(ResetPwdModel model)
        {
            WebRv rv = new WebRv();
            try
            {
                QueryExpression isExistMst = new QueryExpression();
                isExistMst.ColumnSet = new ColumnSet("mobilephone");
                isExistMst.EntityName = "systemuser";
                isExistMst.Criteria.AddCondition("new_jobnumber", ConditionOperator.Equal, model.jobNumber);
                EntityCollection isExistList = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(isExistMst);
                if (isExistList != null && isExistList.Entities.Count > 0 && isExistList.Entities[0].Contains("mobilephone"))
                {
   
                    //IDictionary<String, String> body = new Dictionary<String, String>();
                    //body.Add("CorpID", "CSJS006535");
                    //body.Add("Pwd", "o281k*o906");
                    var mobilephone = isExistList.Entities[0].GetAttributeValue<string>("mobilephone");
                    //body.Add("Mobile", mobilephone);
                    var newcode = NewCode();
                    //body.Add("Content", $"楚天科技CRM系统验证码{newcode}，请勿泄漏给他人。如非本人操作，请联系系统管理员！");
                    HttpRuntime.Cache.Insert(model.jobNumber, newcode, null, DateTime.Now.AddMinutes(2), TimeSpan.Zero);

                    //var rb = new RequestBuilder("https://sdk2.028lk.com/sdk2/BatchSend2.aspx", HttpVerb.Post).Headers(header).Form(body);
                    //SyncRespEntity sre1 = rb.GoSync();

                    LinkWS LinkWs = new LinkWS();
                    LinkWs.Url = "https://sdk2.028lk.com/sdk2/LinkWS.asmx";
                    string resp = LinkWs.Send2("CSJS006535", "o281k*o906", mobilephone, $"楚天科技CRM系统验证码{newcode}，请勿泄漏给他人。如非本人操作，请联系系统管理员！", "", "");

                    if ( Convert.ToInt32(resp) > 0)
                    {
                        rv.code = ResponseCode.success;
                        rv.msg = "验证码发送成功，"+ resp;
                        rv.data = mobilephone;
                    }
                    else
                    {
                        rv.code = ResponseCode.failure;
                        rv.msg = "验证码发送失败";
                    }
                }
                else
                {
                    rv.code = ResponseCode.failure;
                    rv.msg = "未找到工号对应的手机号，请联系管理员";
                }
            }
            catch (Exception ex)
            {
                rv.code = ResponseCode.failure;
                rv.msg = ex.Message;
            }
            return rv;
        }

        [HttpPost]
        public WebRv ResetPwd(ResetPwdModel model)
        {
            WebRv rv = new WebRv();
            try
            {
                var obj = HttpRuntime.Cache.Get(model.jobNumber);
                var code = "";
                if (obj != null)
                {
                    code = obj.ToString();
                }
                if (code == model.sms)
                {
                    SetPassword("truking", "012338", "ctkj.666", model.jobNumber, model.newPassword);
                    rv.code = ResponseCode.success;
                    rv.msg = "密码修改成功";
                }
                else
                {
                    rv.code = ResponseCode.failure;
                    rv.msg = "验证码错误";
                }
            }
            catch (Exception ex)
            {
                rv.code = ResponseCode.error;
                rv.msg = ex.Message;
            }
            return rv;
        }


        [NonAction]
        public void SetPassword(string domainName, string userAdmin, string adminPwd, string userChange, string userChangePwd)
        {
            try
            {
                string ldapPath = "LDAP://172.16.200.11";
                DirectoryEntry directionEntry = new DirectoryEntry(ldapPath, domainName + "\\" + userAdmin, adminPwd);
                if (directionEntry != null)

                {
                    DirectorySearcher search = new DirectorySearcher(directionEntry);
                    search.Filter = "(SAMAccountName=" + userChange + ")";
                    SearchResult result = search.FindOne();
                    if (result != null)
                    {
                        DirectoryEntry userEntry = result.GetDirectoryEntry();
                        if (userEntry != null)
                        {
                            userEntry.Invoke("SetPassword", new object[] { userChangePwd });
                            userEntry.CommitChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [NonAction]
        public string NewCode()
        {
            string vc = "";
            Random rNum = new Random();//随机生成类
            int num1 = rNum.Next(0, 9);//返回指定范围内的随机数
            int num2 = rNum.Next(0, 9);
            int num3 = rNum.Next(0, 9);
            int num4 = rNum.Next(0, 9);
            int num5 = rNum.Next(0, 9);
            int num6 = rNum.Next(0, 9);
            int[] nums = new int[6] { num1, num2, num3, num4, num5, num6 };
            for (int i = 0; i < nums.Length; i++)//循环添加四个随机生成数
            {
                vc += nums[i].ToString();
            }
            return vc;
        }
    }
}