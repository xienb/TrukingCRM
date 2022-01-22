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
        /// <summary>
        /// 日报
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="new_servicearea"></param>
        /// <returns></returns>
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
                    qe.Criteria.AddCondition("createdon", ConditionOperator.GreaterThan, startDT.AddSeconds(-1));
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

        /// <summary>
        /// 服务日志
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="new_servicearea"></param>
        /// <returns></returns>
        public ActionResult ServiceReport(string start, string end, string new_servicearea)
        {
            //new_servicearea  1 国内  2 国际
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
                    QueryExpression qe = new QueryExpression("new_srv_servicereport");
                    qe.Criteria.AddCondition("createdon", ConditionOperator.GreaterThan, startDT.AddSeconds(-1));
                    qe.Criteria.AddCondition("createdon", ConditionOperator.LessThan, endDT.AddDays(1));
                    qe.ColumnSet = new ColumnSet("createdon", "new_servicecontent", "new_name");//创建时间、服务内容、明日计划


                    LinkEntity worker = qe.AddLink("new_srv_worker", "new_srv_worker_id", "new_srv_workerid", JoinOperator.LeftOuter);
                    worker.EntityAlias = "worker";
                    worker.Columns.AddColumns("new_srv_workerid");

                    LinkEntity user = worker.AddLink("systemuser", "new_systemuser_id", "systemuserid", JoinOperator.LeftOuter);
                    user.EntityAlias = "user";
                    user.Columns.AddColumns("fullname", "new_jobnumber");

                    LinkEntity dept = user.AddLink("businessunit", "businessunitid", "businessunitid", JoinOperator.LeftOuter);
                    dept.EntityAlias = "dept";
                    dept.Columns.AddColumns("name");


                    LinkEntity workorder = qe.AddLink("new_srv_workorder", "new_srv_workorder_id", "new_srv_workorderid");
                    workorder.EntityAlias = "workorder";
                    workorder.Columns.AddColumns("new_srv_workorderid", "new_customerid", "new_name", "new_prioritycode", "new_type");//主键、客户单位、服务单号、紧急程度、服务类型


                    LinkEntity acc = workorder.AddLink("account", "new_customerid", "accountid", JoinOperator.LeftOuter);
                    acc.EntityAlias = "acc";
                    acc.Columns.AddColumns("name");

                    LinkEntity country = workorder.AddLink("new_country", "new_country_id", "new_countryid");
                    if (new_servicearea == "1")
                    {
                        country.LinkCriteria.AddCondition("new_code", ConditionOperator.Equal, "40000");
                    }
                    else
                    {
                        country.LinkCriteria.AddCondition("new_code", ConditionOperator.NotEqual, "40000");
                    }

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
                    row.CreateCell(7).SetCellValue("工单号");
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
                        rowi.CreateCell(5).SetCellValue(current.FormattedValues["workorder.new_prioritycode"]); //紧急程度 new_prioritycode
                        rowi.CreateCell(6).SetCellValue(current.GetAttributeValue<string>("new_servicecontent"));//服务内容
                        rowi.CreateCell(7).SetCellValue(current.GetAliasAttributeValue<string>("workorder.new_name"));//工单号
                        rowi.CreateCell(8).SetCellValue(current.GetAttributeValue<string>("new_name"));//明日计划
                        rowi.CreateCell(9).SetCellValue(current.FormattedValues["workorder.new_type"]);//服务类型
                    }
                    MemoryStream BookStream = new MemoryStream();
                    workbook2007.Write(BookStream);
                    BookStream.Seek(0, SeekOrigin.Begin);

                    return File(BookStream, "application/x-xls", "服务日志.xls");
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}