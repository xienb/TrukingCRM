//using EDoc2.IAppService;
//using EDoc2.Sdk;
//using EDoc2.Sdk.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Truking.CRM.Helper;
using Truking.CRM.Web.Base;

namespace Truking.CRM.Web.Controllers
{
    /// <summary>
    /// Edoc附件相关
    /// </summary>
    public class EdocController: TKController
    {
        //[HttpPost]
        //public JsonResult Upload()
        //{
        //    try
        //    {
        //        if (System.Web.HttpContext.Current.Request.Files.Count > 0)
        //        {
        //            var guidp = Guid.NewGuid().ToString("N");
        //            var basePath = AppDomain.CurrentDomain.BaseDirectory + $@"tmp\{guidp}\";
        //            if (!Directory.Exists(basePath))
        //            {
        //                Directory.CreateDirectory(basePath);
        //            }
        //            HttpPostedFile file = System.Web.HttpContext.Current.Request.Files[0];

        //            Stream stm = file.InputStream;
        //            //var fullpath = basePath + file.FileName;
        //            //file.SaveAs(fullpath);//保存文件

        //            SdkBaseInfo.BaseUrl = "http://edoc.truking.cn:8088";
        //            string token = "0034fb8fe789c5b34ed2b160598198ec86c8";
        //            String path = @"D:\code\dotnet\Truking.CRM\Truking.CRM.Web\tmp\f1b027b6f1304afd95417ff8ecc1800a\楚天_服务器配置建议_v1.7.xlsx";
        //            var parentFolderId = 1428481;
        //            var resp = Uploader.UploadFile(token, path, parentFolderId);



        //            //SdkBaseInfo.BaseUrl = AppConfig.Get("edocUrl");
        //            //IFolderAppService folderAppService = ServiceContainer.GetService<IFolderAppService>();
        //            //string token = AppConfig.Get("edocToken");
        //            //var parentFolderId = 1428481;
        //            //var resp = Uploader.UploadFile(token, file.FileName, stm, parentFolderId, UpgradeStrategy.Rename);

        //            return Json(new
        //            {
        //                s = true,
        //                msg = "成功",
        //                guid = guidp,
        //                edocResp = ""
        //            });
        //        }
        //        else
        //        {
        //            return Json(new { s = false, msg = "文件为空" });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new { s = false, msg = e.Message });
        //    }
        //}

        //[HttpGet]
        //public JsonResult Test()
        //{
        //    try
        //    {

        //        SdkBaseInfo.BaseUrl = "http://edoc.truking.cn:8088";
        //        string token = "0034fb8fe789c5b34ed2b160598198ec86c8";
        //        string path = @"D:\Users\Desktop\33.docx";
        //        int parentFolderId = 1428481;
        //        UploadFileResult resp = Uploader.UploadFile(token, path, parentFolderId);



        //        return Json(new
        //        {
        //            s = true,
        //            msg = "成功",
        //            edocResp = ""
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            s = false,
        //            msg = ex.Message+ex.StackTrace,
        //            edocResp = ""
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpGet]
        public JsonResult Test2()
        {
            return Json(new
            {
                s = true,
                msg = "成功",
                edocResp = ""
            }, JsonRequestBehavior.AllowGet);
        }
    }
}