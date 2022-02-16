using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Truking.CRM.Web.Base;
using Truking.CRM.Web.Helper;

namespace Truking.CRM.Web.Controllers
{
    public class FileSrvController : TKController
    {
        /// <summary>
        /// 大附件上传界面跳转
        /// </summary>
        /// <param name="OrgLCID"></param>
        /// <param name="UserLCID"></param>
        /// <param name="id"></param>
        /// <param name="orgname"></param>
        /// <param name="type"></param>
        /// <param name="typename"></param>
        /// <returns></returns>
        public ActionResult FileListView(string OrgLCID, string UserLCID, string id, string orgname, string type, string typename)
        {
            string entityName = typename;
            string entityId = id.Replace("{", "").Replace("}", "");
            string appsitename = Request.ApplicationPath.Equals("/") ? string.Empty : Request.ApplicationPath;
            string appurl = Request.Url.GetLeftPart(UriPartial.Authority) + appsitename;
            string url = appurl + $"/#/crmUpload/{entityName}/{entityId}";
            return Redirect(url);
        }

        /// <summary>
        /// 获取单据附件个数
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EntityFileCount(string entityName,string entityId)
        {
            WebRv rv = new WebRv();
            int fileCount = 0;
            if (!string.IsNullOrEmpty(entityName) && !string.IsNullOrEmpty(entityId))
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory + $@"tmp\{entityName}\{entityId}";
                if (Directory.Exists(basePath))
                {
                    DirectoryInfo rootDir = new DirectoryInfo(basePath);

                    DirectoryInfo[] directs = rootDir.GetDirectories();
                    fileCount = directs.Length;
                }
            }
            rv.data = fileCount;
            return Json(rv);
        }

        /// <summary>
        /// 文件模板列表跳转
        /// </summary>
        /// <param name="OrgLCID"></param>
        /// <param name="UserLCID"></param>
        /// <param name="id"></param>
        /// <param name="orgname"></param>
        /// <param name="type"></param>
        /// <param name="typename"></param>
        /// <returns></returns>
        public ActionResult BizTmpView(string OrgLCID, string UserLCID, string id, string orgname, string type, string typename)
        {
            string entityName = typename;
            string appsitename = Request.ApplicationPath.Equals("/") ? string.Empty : Request.ApplicationPath;
            string appurl = Request.Url.GetLeftPart(UriPartial.Authority) + appsitename;
            string url = appurl + $"/#/filetemplate/{entityName}";
            return Redirect(url);
        }

        /// <summary>
        /// 下载crm附件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetFile(string id)
        {
            try
            {
                QueryExpression isExistMst = new QueryExpression("annotation");
                isExistMst.ColumnSet = new ColumnSet("filename", "documentbody", "isdocument", "mimetype", "filesize");
                isExistMst.Criteria.AddCondition("annotationid", ConditionOperator.Equal, new Guid(id));
                EntityCollection isExistList = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(isExistMst);
                if (isExistList != null && isExistList.Entities.Count > 0)
                {
                    Entity fileEntity = isExistList.Entities[0];

                    bool isDoc = fileEntity.GetAttributeValue<Boolean>("isdocument");
                    if (isDoc)
                    {
                        var mimetype = fileEntity.GetAttributeValue<string>("mimetype");
                        var filesize = fileEntity.GetAttributeValue<int>("filesize");
                        var fileName = fileEntity.GetAttributeValue<string>("filename");
                        var documentbody = fileEntity.GetAttributeValue<string>("documentbody");
                        byte[] pReadByte = Convert.FromBase64String(documentbody);
                        var contenttype = GetContentType(fileName);
                        return File(pReadByte, contenttype, fileName);
                    }
                    else
                    {
                        return Content("该id不是文件");
                    }
                }
                else
                {
                    return Content("文件不存在");
                }

            }
            catch (Exception ex)
            {
                return Content(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 下载大附件
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public void DownloadFile(string entityName, string entityId, string fileId)
        {
            try
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory + $@"tmp\{entityName}\{entityId}\{fileId}";
                if (Directory.Exists(basePath))
                {
                    DirectoryInfo dir = new DirectoryInfo(basePath);
                    FileInfo[] files = dir.GetFiles();
                    if (files.Length > 0)
                    {
                        //var stream = System.IO.File.OpenRead(files[0].FullName);
                        //var contenttype = GetContentType(files[0].Name);
                        //return File(stream, contenttype, files[0].Name);

                        const long ChunkSize = 1024000;//100K 每次读取文件，只读取1000K，这样可以缓解服务器的压力
                        byte[] buffer = new byte[ChunkSize];
                        Response.Clear();
                        System.IO.FileStream iStream = System.IO.File.OpenRead(files[0].FullName);
                        long dataLengthToRead = iStream.Length;//获取下载的文件总大小
                        Response.ContentType = GetContentType(files[0].Name);
                        Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(files[0].Name));
                        while (dataLengthToRead > 0 && Response.IsClientConnected)
                        {
                            int lengthRead = iStream.Read(buffer, 0, Convert.ToInt32(ChunkSize));//读取的大小
                            Response.OutputStream.Write(buffer, 0, lengthRead);
                            Response.Flush();
                            dataLengthToRead = dataLengthToRead - lengthRead;
                        }
                        Response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 大文件上传接口
        /// </summary>
        /// <returns></returns>
        public JsonResult Upload()
        {
            try
            {
                if (System.Web.HttpContext.Current.Request.Files.Count > 0)
                {
                    var chunkNumber = System.Web.HttpContext.Current.Request.Form["chunkNumber"];//块编号
                    var chunkSize = System.Web.HttpContext.Current.Request.Form["chunkSize"];//块大小
                    var currentChunkSize = System.Web.HttpContext.Current.Request.Form["currentChunkSize"];//当前块大小
                    var totalSize = System.Web.HttpContext.Current.Request.Form["totalSize"];//总大小
                    var identifier = System.Web.HttpContext.Current.Request.Form["identifier"];//文件唯一标识
                    var filename = System.Web.HttpContext.Current.Request.Form["filename"];//文件名
                    var relativePath = System.Web.HttpContext.Current.Request.Form["relativePath"];//文件相对路径
                    var totalChunks = System.Web.HttpContext.Current.Request.Form["totalChunks"];//总块数
                    var entityName = System.Web.HttpContext.Current.Request.Form["entityName"];//实体名
                    var entityId = System.Web.HttpContext.Current.Request.Form["entityId"];//实体id
                    if (string.IsNullOrEmpty(chunkNumber) ||
                        string.IsNullOrEmpty(chunkNumber) ||
                        string.IsNullOrEmpty(chunkNumber) ||
                        string.IsNullOrEmpty(chunkNumber) ||
                        string.IsNullOrEmpty(chunkNumber) ||
                        string.IsNullOrEmpty(chunkNumber) ||
                        string.IsNullOrEmpty(chunkNumber) ||
                        string.IsNullOrEmpty(chunkNumber) ||
                        string.IsNullOrEmpty(chunkNumber) ||
                        string.IsNullOrEmpty(chunkNumber))
                    {
                        //return Json(new WebRv() { code = ResponseCode.parsError, msg = "参数错误" });
                        throw new Exception("参数错误");
                    }
                    else
                    {
                        var basePath = AppDomain.CurrentDomain.BaseDirectory + $@"tmp\{entityName}\{entityId}\{identifier}";
                        var partPath = basePath + @"\part";
                        if (!Directory.Exists(basePath))
                        {
                            Directory.CreateDirectory(basePath);
                        }
                        if (!Directory.Exists(partPath))
                        {
                            Directory.CreateDirectory(partPath);
                        }

                        HttpPostedFile file = System.Web.HttpContext.Current.Request.Files[0];
                        file.SaveAs(partPath + Path.DirectorySeparatorChar + chunkNumber);

                        return Json(new WebRv() { code = ResponseCode.success });
                    }
                }
                else
                {
                    return Json(new WebRv() { code = ResponseCode.parsError });
                }
            }
            catch (Exception ex)
            {
                throw ex;
                // return Json(new WebRv() { code = ResponseCode.error, msg = ex.Message + ex.StackTrace });
            }
        }

        [NonAction]
        private string GetContentType(string asr_filename)
        {
            var contenttype = "application/octet-stream";
            if (asr_filename.Contains(".doc") || asr_filename.Contains(".docx"))
            {
                contenttype = "application/msword";
            }
            else if (asr_filename.Contains(".xls") || asr_filename.Contains(".xlsx"))
            {
                contenttype = "application/x-xls";
            }
            else if (asr_filename.Contains(".pdf"))
            {
                contenttype = "application/pdf";
            }
            else if (asr_filename.Contains(".jpg"))
            {
                contenttype = "application/x-jpg";
            }
            else if (asr_filename.Contains(".png"))
            {
                contenttype = "image/png";
            }
            else if (asr_filename.Contains(".txt"))
            {
                contenttype = "text/plain";
            }
            return contenttype;
        }
    }
}