using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
    /// <summary>
    /// Excel导出
    /// </summary>
    public class ExcelController : TKController
    {
        public ActionResult DailyReport(string start, string end, string new_servicearea)
        {
            try
            {
                if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end) || string.IsNullOrEmpty(new_servicearea))
                {
                    return Content("参数错误");
                }
                else
                {
                    DateTime startDT = Convert.ToDateTime(start);
                    DateTime endDT = Convert.ToDateTime(end);
                    int area = Convert.ToInt32(new_servicearea);
                    QueryExpression qe = new QueryExpression("new_daily_report");
                    qe.ColumnSet = new ColumnSet("createdon", "new_degreeurgency", "new_type", "new_finish_work", "new_coordinate_work", "new_schedule");
                    qe.Criteria.AddCondition("new_servicearea", ConditionOperator.Equal, area);
                    qe.Criteria.AddCondition("createdon", ConditionOperator.GreaterThan, startDT);
                    qe.Criteria.AddCondition("createdon", ConditionOperator.LessThan, endDT.AddDays(1));

                    LinkEntity user = qe.AddLink("systemuser", "ownerid", "systemuserid", JoinOperator.LeftOuter);
                    user.EntityAlias = "user";
                    user.Columns.AddColumns("fullname", "new_jobnumber");

                    LinkEntity dept = user.AddLink("businessunit", "businessunitid", "businessunitid", JoinOperator.LeftOuter);
                    dept.EntityAlias = "dept";
                    dept.Columns.AddColumns("name");

                    LinkEntity acc = qe.AddLink("account", "new_account_id", "accountid", JoinOperator.LeftOuter);
                    acc.EntityAlias = "acc";
                    acc.Columns.AddColumns("name");

                    var ec = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(qe);
                    int count = ec.Entities.Count;

                    HSSFWorkbook workbook2007 = new HSSFWorkbook(); //创建xlsx工作簿
                    ISheet SheetOne = workbook2007.CreateSheet();
                    IRow row = SheetOne.CreateRow(0);
                    row.CreateCell(0).SetCellValue("日期");
                    row.CreateCell(1).SetCellValue("姓名");
                    row.CreateCell(2).SetCellValue("工号");
                    row.CreateCell(3).SetCellValue("服务大区");
                    row.CreateCell(4).SetCellValue("客户单位");
                    row.CreateCell(5).SetCellValue("紧急程度");
                    row.CreateCell(6).SetCellValue("服务内容");
                    row.CreateCell(7).SetCellValue("工作障碍");
                    row.CreateCell(8).SetCellValue("明天计划");
                    row.CreateCell(9).SetCellValue("服务类型");

                    for (int i = 0; i < count; i++)
                    {
                        Entity current = ec.Entities[i];
                        IRow rowi = SheetOne.CreateRow(i + 1);
                        rowi.CreateCell(0).SetCellValue(current.GetAttributeValue<DateTime>("createdon").ToLocalTime().ToString("yyyy/MM/dd"));
                        rowi.CreateCell(1).SetCellValue(current.GetAliasAttributeValue<string>("user.fullname"));
                        rowi.CreateCell(2).SetCellValue(current.GetAliasAttributeValue<string>("user.new_jobnumber"));
                        rowi.CreateCell(3).SetCellValue(current.GetAliasAttributeValue<string>("dept.name"));
                        rowi.CreateCell(4).SetCellValue(current.GetAliasAttributeValue<string>("acc.name"));
                        rowi.CreateCell(5).SetCellValue(current.GetPicklistName("new_degreeurgency"));
                        rowi.CreateCell(6).SetCellValue(current.GetAttributeValue<string>("new_finish_work"));
                        rowi.CreateCell(7).SetCellValue(current.GetAttributeValue<string>("new_coordinate_work"));
                        rowi.CreateCell(8).SetCellValue(current.GetAttributeValue<string>("new_schedule"));
                        rowi.CreateCell(9).SetCellValue(current.GetPicklistName("new_type"));
                    }
                    MemoryStream BookStream = new MemoryStream();
                    workbook2007.Write(BookStream);
                    BookStream.Seek(0, SeekOrigin.Begin);//输出之前调用Seek（偏移量，游标位置）方法：获取文件流的长度

                    return File(BookStream, "application/x-xls", "日报.xls");
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}