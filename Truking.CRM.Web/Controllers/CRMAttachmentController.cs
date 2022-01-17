using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Truking.CRM.Web.Base;

namespace Truking.CRM.Web.Controllers
{
    /// <summary>
    /// Dynamic365 CRM 附件相关
    /// </summary>
    public class CRMAttachmentController: TKController
    {
        [HttpGet]
        public JsonResult Test1()
        {
            throw new Exception("abc");
            return Json(new
            {
                s = true,
                msg = "成功",
                edocResp = ""
            }, JsonRequestBehavior.AllowGet);
        }
    }
}