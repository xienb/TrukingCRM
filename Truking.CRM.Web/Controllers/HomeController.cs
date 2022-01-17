using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Truking.CRM.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //读取html文件内容
            String filepath = Server.MapPath("~/index.html");
            if (System.IO.File.Exists(filepath))
            {
                StreamReader sr = new StreamReader(Server.MapPath("~/index.html"));
                String htmlContent = sr.ReadToEnd();
                sr.Close();
                return Content(htmlContent);
            }
            else 
            {
                return Content("页面不存在");
            }
        }

        public ActionResult BaiduApi()
        {
            return View("BaiduApi");
        }
    }
}
