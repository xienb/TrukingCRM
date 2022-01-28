using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Truking.CRM.Web.Base;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Truking.CRM.Web.Helper;
using System.IO;

namespace Truking.CRM.Web.Controllers
{
    public class Excel2Controller : TKApiController
    {
        [HttpPost]
        public WebRv SalesLog(String[] idlist)
        {
            WebRv rv = new WebRv();
            QueryExpression qe = new QueryExpression("appointment");
            qe.Criteria.AddCondition("activityid", ConditionOperator.In, idlist);
            qe.ColumnSet = new ColumnSet(true);

            //jobtitle
            LinkEntity contact = qe.AddLink("contact", "new_contact_id", "contactid",JoinOperator.LeftOuter);
            contact.EntityAlias = "contact";
            contact.Columns.AddColumns("jobtitle", "fullname");



            var ec = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(qe);
            int count = ec.Entities.Count;
            if (count > 0)
            {
                HSSFWorkbook workbook2007 = new HSSFWorkbook(); //创建xlsx工作簿
                ISheet SheetOne = workbook2007.CreateSheet();
                IRow row = SheetOne.CreateRow(0);
                row.CreateCell(0).SetCellValue("开始时间");
                row.CreateCell(1).SetCellValue("结束时间");
                row.CreateCell(2).SetCellValue("状态");
                row.CreateCell(3).SetCellValue("负责人");
                row.CreateCell(4).SetCellValue("大区");
                row.CreateCell(5).SetCellValue("主题");
                row.CreateCell(6).SetCellValue("工作项");
                row.CreateCell(7).SetCellValue("机会点名称");
                row.CreateCell(8).SetCellValue("客户名称");
                row.CreateCell(9).SetCellValue("客户");
                row.CreateCell(10).SetCellValue("职位");
                row.CreateCell(11).SetCellValue("工作目标/内容");
                row.CreateCell(12).SetCellValue("完成情况说明");
                row.CreateCell(13).SetCellValue("待办/计划");
                row.CreateCell(14).SetCellValue("需要支持/协助的工作");
                row.CreateCell(15).SetCellValue("出差人员身体状态");
                for (int i = 0; i < count; i++)
                {
                    Entity current = ec.Entities[i];
                    IRow rowi = SheetOne.CreateRow(i + 1);
                    rowi.CreateCell(0).SetCellValue(current.Contains("actualstart") ? current.GetAttributeValue<DateTime>("actualstart").ToLocalTime().ToString("yyyy/MM/dd HH:mm"):"");
                    rowi.CreateCell(1).SetCellValue(current.Contains("actualend") ? current.GetAttributeValue<DateTime>("actualend").ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "");
                    rowi.CreateCell(2).SetCellValue(current.Contains("statecode") ? current.FormattedValues["statecode"] : "");
                    rowi.CreateCell(3).SetCellValue(current.Contains("ownerid") ? current.GetAttributeValue<EntityReference>("ownerid").Name : "");
                    rowi.CreateCell(4).SetCellValue(current.Contains("new_businessunit_id") ? current.GetAttributeValue<EntityReference>("new_businessunit_id").Name : "");
                    rowi.CreateCell(5).SetCellValue(current.GetAttributeValue<string>("subject"));
                    rowi.CreateCell(6).SetCellValue(current.Contains("new_worklist") ? current.FormattedValues["new_worklist"] : "");
                    rowi.CreateCell(7).SetCellValue(current.Contains("new_opportunity_id") ? current.GetAttributeValue<EntityReference>("new_opportunity_id").Name : "");
                    rowi.CreateCell(8).SetCellValue(current.Contains("new_account_id") ? current.GetAttributeValue<EntityReference>("new_account_id").Name : "");
                    rowi.CreateCell(9).SetCellValue(current.GetAliasAttributeValue<string>("contact.fullname"));
                    rowi.CreateCell(10).SetCellValue(current.GetAliasAttributeValue<string>("contact.jobtitle"));
                    rowi.CreateCell(11).SetCellValue(current.GetAttributeValue<string>("new_result"));
                    rowi.CreateCell(12).SetCellValue(current.GetAttributeValue<string>("new_goalofwork"));
                    rowi.CreateCell(13).SetCellValue(current.GetAttributeValue<string>("new_keyactions"));
                    rowi.CreateCell(14).SetCellValue(current.GetAttributeValue<string>("new_needssupport"));
                    rowi.CreateCell(15).SetCellValue(current.GetAttributeValue<string>("new_physicalstatus"));
                }
                //AppDomain.CurrentDomain.BaseDirectory + @"saleslog\" + id;
                var baseDir = AppDomain.CurrentDomain.BaseDirectory + "saleslog";
                if (!System.IO.Directory.Exists(baseDir))
                {
                    System.IO.Directory.CreateDirectory(baseDir);
                }
                string id = Guid.NewGuid().ToString("n") + ".xls";
                string file = baseDir + @"\" + id;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
                    {
                        workbook2007.Write(fs);
                    }
                }
                workbook2007.Close();
                rv.data = id;
            }
            return rv;
        }
    }
}