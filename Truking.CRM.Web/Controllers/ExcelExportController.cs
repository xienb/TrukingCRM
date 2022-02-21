using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Truking.CRM.Web.Base;
using Truking.CRM.Web.Helper;

namespace Truking.CRM.Web.Controllers
{
    /// <summary>
    /// Excel 导出接口
    /// </summary>
    public class ExcelExportController : TKController
    {
        public class PicInfo
        {
            public byte[] data { get; set; }
            public string type { get; set; }
            public string mimetype { get; set; }
        }

        /// <summary>
        /// 上传excel，并识别excel中的数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Upload()
        {
            WebRv rv = new WebRv();
            try
            {
                string mainId = System.Web.HttpContext.Current.Request.Params["mainId"];
                bool append = Convert.ToBoolean(System.Web.HttpContext.Current.Request.Params["append"]);
                if (string.IsNullOrEmpty(mainId))
                {
                    rv.code = ResponseCode.parsError;
                    rv.msg = "参数错误";
                }
                else
                {
                    var org = OrganizationServiceInstance.Instance.OrgService;
                    Entity mainEntity = org.Retrieve("new_srv_accessoriesdemand", Guid.Parse(mainId), new ColumnSet(false));
                    if (mainEntity == null)
                    {
                        rv.code = ResponseCode.failure;
                        rv.msg = "主档不存在";
                    }
                    else
                    {
                        if (System.Web.HttpContext.Current.Request.Files.Count == 0)
                        {
                            rv.code = ResponseCode.failure;
                            rv.msg = "文件未上传";
                        }
                        else
                        {
                            HttpPostedFile file = System.Web.HttpContext.Current.Request.Files[0];
                            IWorkbook workbook = new XSSFWorkbook(file.InputStream);
                            XSSFSheet sheet = (XSSFSheet)workbook.GetSheetAt(0);

                            var documentPartList = sheet.GetRelations();
                            Dictionary<int, PicInfo> picDic = new Dictionary<int, PicInfo>();
                            foreach (var documentPart in documentPartList)
                            {
                                if (documentPart is XSSFDrawing)
                                {
                                    var drawing = (XSSFDrawing)documentPart;
                                    var shapeList = drawing.GetShapes();
                                    foreach (var shape in shapeList)
                                    {
                                        if (shape is XSSFPicture)
                                        {
                                            var picture = (XSSFPicture)shape;
                                            int column = picture.ClientAnchor.Col1;
                                            int row = picture.ClientAnchor.Row1;
                                            byte[] picdata = picture.PictureData.Data;
                                            var picType = picture.PictureData.PictureType;//PNG  JPG
                                            if (!picDic.ContainsKey(row))
                                            {
                                                picDic.Add(row, new PicInfo() { data = picdata, type = picType.ToString(),mimetype = picture.PictureData.MimeType });
                                            }
                                        }
                                    }
                                }
                            }
                            if (!append)
                            {
                                //BulkDeleteRequest bulkDeleteRequest = new BulkDeleteRequest();//批量删除是异步操作，导致会把新增的数据删除掉
                                //批量删除附件
                                QueryExpression deleteAnoQuery = new QueryExpression("annotation");
                                deleteAnoQuery.ColumnSet = new ColumnSet(false);
                                var linkLine = deleteAnoQuery.AddLink("new_srv_accessoriesdemandline", "objectid", "new_srv_accessoriesdemandlineid");
                                linkLine.EntityAlias = "line";
                                linkLine.Columns = new ColumnSet("new_srv_accessoriesdemand_id");
                                var linkMain = linkLine.AddLink("new_srv_accessoriesdemand", "new_srv_accessoriesdemand_id", "new_srv_accessoriesdemandid");
                                linkMain.EntityAlias = "main";
                                linkMain.Columns = new ColumnSet("new_srv_accessoriesdemandid");
                                FilterExpression filter = new FilterExpression(LogicalOperator.And);
                                filter.AddCondition("main", "new_srv_accessoriesdemandid", ConditionOperator.Equal, mainEntity.Id);
                                deleteAnoQuery.Criteria.AddFilter(filter);
                                var ecDelete1 = org.RetrieveMultiple(deleteAnoQuery);
                                if (ecDelete1 != null)
                                {
                                    foreach (var en1 in ecDelete1.Entities)
                                    {
                                        org.Delete("annotation", en1.Id);
                                    }
                                }
                                //批量删除
                                QueryExpression deleteQuery = new QueryExpression("new_srv_accessoriesdemandline");
                                deleteQuery.ColumnSet = new ColumnSet(false);
                                deleteQuery.Criteria.AddCondition("new_srv_accessoriesdemand_id", ConditionOperator.Equal, mainEntity.Id);
                                var ecDelete2 = org.RetrieveMultiple(deleteQuery);
                                if (ecDelete2 != null)
                                {
                                    foreach (var en2 in ecDelete2.Entities)
                                    {
                                        org.Delete("new_srv_accessoriesdemandline", en2.Id);
                                    }
                                }
                                //bulkDeleteRequest.QuerySet = new QueryExpression[] {
                                //    deleteAnoQuery,
                                //    deleteQuery
                                //};
                                //bulkDeleteRequest.JobName = "批量删除客户备件需求及明细";
                                //bulkDeleteRequest.ToRecipients = new Guid[] { OrganizationServiceInstance.Instance.UserId };
                                //bulkDeleteRequest.CCRecipients = new Guid[] { };
                                //bulkDeleteRequest.RecurrencePattern = String.Empty;
                                //bulkDeleteRequest.StartDateTime = DateTime.Now;
                                //bulkDeleteRequest.SendEmailNotification = false;
                                //org.Execute(bulkDeleteRequest);
                            }
                            //表头
                            Dictionary<string, int> headerDic = new Dictionary<string, int>();
                            IRow rowHeader = sheet.GetRow(0);
                            for (int i = 0; i < rowHeader.LastCellNum; i++)
                            {
                                headerDic.Add(getCellValue(sheet, rowHeader.GetCell(i)), i);
                            }

                            //逐条新增，并传附件
                            for (int i = 1; i <= sheet.LastRowNum; i++)
                            {
                                IRow row = sheet.GetRow(i);
                                if (row == null)
                                {
                                    continue;
                                }
                                try
                                {
                                    // 0-11
                                    Entity newLine = new Entity("new_srv_accessoriesdemandline");
                                    newLine["new_srv_accessoriesdemand_id"] = new EntityReference("new_srv_accessoriesdemand", mainEntity.Id);
                                    var col0 = getCellValue(sheet, row.GetCell(headerDic["序号"]));
                                    var col1 = getCellValue(sheet, row.GetCell(headerDic["零件名称"]));
                                    var col2 = getCellValue(sheet, row.GetCell(headerDic["零件型号"]));
                                    var col3 = getCellValue(sheet, row.GetCell(headerDic["物料编码"]));
                                    var sl = getCellValue(sheet, row.GetCell(headerDic["数量"]));
                                    Regex reg = new Regex(@"[\u4e00-\u9fa5]");
                                    var col4 = reg.Replace(sl, "");
                                    var col5 = getCellValue(sheet, row.GetCell(headerDic["品牌厂家"]));
                                    var col6 = getCellValue(sheet, row.GetCell(headerDic["所用设备型号"]));
                                    var datecell = row.GetCell(headerDic["设备出厂年月"]);
                                    if (datecell == null) continue;
                                    var col7 = datecell.DateCellValue;
                                    var col8 = getCellValue(sheet, row.GetCell(headerDic["厂区"]));
                                    var col9 = getCellValue(sheet, row.GetCell(headerDic["使用对接人"]));
                                    var col10 = getCellValue(sheet, row.GetCell(headerDic["使用对接人联系方式"]));
                                    var col11 = getCellValue(sheet, row.GetCell(headerDic["备注（安装位置、相关说明等）"]));

                                    if (string.IsNullOrEmpty(col4) || string.IsNullOrEmpty(col6) || string.IsNullOrEmpty(col8))
                                    {
                                        //必填项为空
                                        continue;
                                    }
                                    newLine["new_serialnumber"] = Convert.ToInt32(col0);//序号
                                    newLine["new_equipmentmodel"] = col1;//零件名称
                                    newLine["new_partmodel"] = col2;//零件型号
                                    newLine["new_materialcode"] = col3;//物料编码
                                    newLine["new_quantity"] = Convert.ToInt32(col4);//数量
                                    newLine["new_manufacturer"] = col5;//品牌厂家
                                    newLine["new_name"] = col6;//所用设备型号
                                    newLine["new_deliverydate"] = col7;//设备出厂年月
                                    newLine["new_plantarea"] = col8;//厂区
                                    newLine["new_buttjoint"] = col9;//使用对接人
                                    newLine["new_buttjointtelephone"] = col10;//使用对接人联系方式
                                    newLine["new_remarks"] = col11;

                                    Guid newId = org.Create(newLine);//保存单行
                                    //保存单行对应附件
                                    if (picDic.ContainsKey(i))
                                    {
                                        var picInfo = picDic[i];
                                        Entity fjEntity = new Entity("annotation");
                                        fjEntity["isdocument"] = true;
                                        fjEntity["mimetype"] = picInfo.mimetype;
                                        fjEntity["filename"] = newId.ToString("n")+"."+picInfo.type;
                                        fjEntity["filesize"] = picInfo.data.Length;
                                        fjEntity["documentbody"] = Convert.ToBase64String(picInfo.data);
                                        fjEntity["objectid"] = new EntityReference("new_srv_accessoriesdemandline",newId);
                                        org.Create(fjEntity);
                                    }
                                }
                                catch (Exception e)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
                return Json(rv);
            }
            catch (Exception ex)
            {
                rv.code = ResponseCode.error;
                rv.msg = ex.Message + ex.StackTrace;
                return Json(rv);
            }
        }

        /// <summary>
        /// 获取单元格值
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        [NonAction]
        private string getCellValue(ISheet sheet, ICell cell)
        {
            if (cell == null) { return ""; }
            if (cell.IsMergedCell)
            {
                for (int ii = 0; ii < sheet.NumMergedRegions; ii++)
                {
                    var cellrange = sheet.GetMergedRegion(ii);
                    if (cell.ColumnIndex >= cellrange.FirstColumn && cell.ColumnIndex <= cellrange.LastColumn
                        && cell.RowIndex >= cellrange.FirstRow && cell.RowIndex <= cellrange.LastRow)
                    {
                        var tmpcell = sheet.GetRow(cellrange.FirstRow).GetCell(cellrange.FirstColumn);
                        if (tmpcell == null) { return ""; }
                        else
                        {
                            return tmpcell.ToString();
                        }
                    }
                }
            }
            return cell.ToString();
        }
    }
}