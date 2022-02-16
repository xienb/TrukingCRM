using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Truking.CRM.Web.Base;
using Truking.CRM.Web.Helper;
using Truking.CRM.Web.Model;

namespace Truking.CRM.Web.Controllers
{
    public class FileInfoController : TKApiController
    {
        /// <summary>
        /// 获取所有实体下某一条记录的所有附件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public WebRv EntityFileList(EntityInfo model)
        {
            WebRv rv = new WebRv();
            List<MyFileInfo> list = new List<MyFileInfo>();
            if (!string.IsNullOrEmpty(model.entityName) && !string.IsNullOrEmpty(model.entityId))
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory + $@"tmp\{model.entityName}\{model.entityId}";
                if (Directory.Exists(basePath))
                {
                    DirectoryInfo rootDir = new DirectoryInfo(basePath);

                    DirectoryInfo[] directs = rootDir.GetDirectories();
                    foreach (DirectoryInfo fileDir in directs)
                    {
                        FileInfo[] files = fileDir.GetFiles();
                        if (files.Length > 0)
                        {
                            MyFileInfo myFileInfo = new MyFileInfo();
                            myFileInfo.entityId = model.entityId;
                            myFileInfo.entityName = model.entityName;
                            myFileInfo.fileId = fileDir.Name;
                            myFileInfo.fileName = files[0].Name;
                            myFileInfo.fileSize = files[0].Length;
                            list.Add(myFileInfo);
                        }
                    }
                }
            }
            rv.data = list;
            return rv;
        }

        /// <summary>
        /// 获取附件个数
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public WebRv EntityFileCount(EntityInfo model)
        {
            WebRv rv = new WebRv();
            int fileCount = 0;
            if (!string.IsNullOrEmpty(model.entityName) && !string.IsNullOrEmpty(model.entityId))
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory + $@"tmp\{model.entityName}\{model.entityId}";
                if (Directory.Exists(basePath))
                {
                    DirectoryInfo rootDir = new DirectoryInfo(basePath);

                    DirectoryInfo[] directs = rootDir.GetDirectories();
                    foreach (DirectoryInfo fileDir in directs)
                    {
                        FileInfo[] files = fileDir.GetFiles();
                        fileCount = files.Length;
                    }
                }
            }
            rv.data = fileCount;
            return rv;
        }

        /// <summary>
        /// 通过组织服务获取实体的模板文件列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public WebRv GetEntityTmpList(EntityInfo model)
        {
            WebRv rv = new WebRv();

            QueryExpression bizTmpQe = new QueryExpression("new_bizfiletmp");
            bizTmpQe.ColumnSet = new ColumnSet("new_bizfiletmpid");
            bizTmpQe.Criteria.AddCondition("new_entityname", ConditionOperator.Equal, model.entityName);
            EntityCollection bizEc = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(bizTmpQe);
            if (bizEc != null && bizEc.Entities.Count > 0)
            {
                QueryExpression isExistMst = new QueryExpression("annotation");
                isExistMst.ColumnSet = new ColumnSet("filename", "filesize", "annotationid");
                isExistMst.Criteria.AddCondition("objectid", ConditionOperator.Equal, bizEc.Entities[0].Id);
                EntityCollection fjEc = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(isExistMst);
                if (fjEc != null && fjEc.Entities.Count > 0)
                {


                    List<BizTmp> list = new List<BizTmp>();
                    foreach (Entity oneRow in fjEc.Entities)
                    {
                        BizTmp bizTmp = new BizTmp();
                        bizTmp.fileSize = oneRow.GetAttributeValue<int>("filesize");
                        bizTmp.fileName = oneRow.GetAttributeValue<string>("filename");
                        bizTmp.url = oneRow.GetAttributeValue<Guid>("annotationid").ToString();
                        list.Add(bizTmp);
                    }
                    rv.data = list;
                }
            }
            return rv;
        }

        /// <summary>
        /// 将所有文件块合并
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WebRv Merge(MyFileInfo model)
        {
            WebRv rv = new WebRv();
            var filePath = AppDomain.CurrentDomain.BaseDirectory + $@"tmp\{model.entityName}\{model.entityId}\{model.fileId}";
            var partPath = filePath + @"\part";
            DirectoryInfo partDir = new DirectoryInfo(partPath);
            FileInfo[] partFiles = partDir.GetFiles();
            Array.Sort(partFiles, delegate (FileInfo x, FileInfo y) {
                int x1 = Convert.ToInt32(x.Name);
                int y1 = Convert.ToInt32(y.Name);
                return x1.CompareTo(y1); 
            });
            FileStream stream = File.Create(filePath + Path.DirectorySeparatorChar + model.fileName);


            for (int i = 0; i < partFiles.Length; i++)
            {
                using (FileStream fsRead = new FileStream(partFiles[i].FullName, FileMode.Open))
                {
                    fsRead.CopyTo(stream);
                }
                File.Delete(partFiles[i].FullName);
            }
            Directory.Delete(partPath);
            stream.Close();

            return rv;
        }

        /// <summary>
        /// 删除大附件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public WebRv DeleteFile(MyFileInfo model) 
        {
            WebRv rv = new WebRv();
            var fileDir = AppDomain.CurrentDomain.BaseDirectory + $@"tmp\{model.entityName}\{model.entityId}\{model.fileId}";
            var partDir = AppDomain.CurrentDomain.BaseDirectory + $@"tmp\{model.entityName}\{model.entityId}\{model.fileId}\part";
            var filePath = fileDir + Path.DirectorySeparatorChar + model.fileName;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            if (Directory.Exists(partDir))
            {
                Directory.Delete(partDir);
            }
            if (Directory.Exists(fileDir))
            {
                Directory.Delete(fileDir);
            }
            return rv;
        }

        /// <summary>
        /// 生成一个新文件，返回fileId（暂时不使用，采用前端生成）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public WebRv NewFile(EntityInfo model)
        {
            WebRv rv = new WebRv();
            var guid = Guid.NewGuid().ToString("N");
            var basePath = AppDomain.CurrentDomain.BaseDirectory + $@"tmp\{model.entityName}\{model.entityId}\{guid}";
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            var partPath = basePath + @"\part";
            if (!Directory.Exists(partPath))
            {
                Directory.CreateDirectory(partPath);
            }
            rv.data = guid;
            return rv;
        }
    }
}