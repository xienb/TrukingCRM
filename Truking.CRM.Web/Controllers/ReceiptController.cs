using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Truking.CRM.Web.Base;
using Truking.CRM.Web.Helper;

namespace Truking.CRM.Web.Controllers
{
    /// <summary>
    /// 收货单接口
    /// </summary>
    public class ReceiptController : TKAccountApiController
    {
        /// <summary>
        /// 按修改时间段 分页获取发货单列表
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost]
        public WebRv QueryList(JObject json)
        {
            WebRv rv = new WebRv();
            try
            {
                var start = json.GetStr("start");
                var end = json.GetStr("end");
                var pageIndex = json.GetStr("pageIndex");
                var new_account_id = GetHeader("new_account_id");
                QueryExpression isExistMst = new QueryExpression("new_ord_shipment");
                isExistMst.ColumnSet = new ColumnSet("new_ord_shipmentid", "new_name", "new_shipping_date", "modifiedon");
                isExistMst.Criteria.AddCondition("new_account_id", ConditionOperator.Equal, new Guid(new_account_id));
                isExistMst.Criteria.AddCondition("modifiedon", ConditionOperator.OnOrAfter, Convert.ToDateTime(start));
                isExistMst.Criteria.AddCondition("modifiedon", ConditionOperator.OnOrBefore, Convert.ToDateTime(end));
                LinkEntity contract = isExistMst.AddLink("new_contract", "new_contract_id", "new_contractid", JoinOperator.LeftOuter);
                contract.EntityAlias = "contract";
                contract.Columns.AddColumns("new_customercontractno", "new_contractno");//第一个是客户合同号


                isExistMst.PageInfo = new PagingInfo()
                {
                    PageNumber = Convert.ToInt32(pageIndex),
                    Count = 5,
                    ReturnTotalRecordCount = true
                };//分页参数
                isExistMst.AddOrder("modifiedon", OrderType.Ascending);
                var ec = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(isExistMst);
                if (ec != null && ec.Entities.Count > 0)
                {
                    JObject rootOut = new JObject();
                    rootOut.Add("total", ec.TotalRecordCount);//总条数
                    JArray main = new JArray();
                    foreach (Entity oneMain in ec.Entities)
                    {
                        JObject root = new JObject();
                        root.Add("new_name", oneMain.GetAttributeValue<string>("new_name"));
                        //if (main.Contains("new_shipping_date"))
                        //{
                        //    root.Add("new_shipping_date", oneMain.GetAttributeValue<DateTime>("new_shipping_date").ToLocalTime().ToString("yyyy-MM-dd"));
                        //}
                        //root.Add("new_customercontractno", oneMain.GetAliasAttributeValue<string>("contract.new_customercontractno"));
                        root.Add("modifiedon", oneMain.GetAttributeValue<DateTime>("modifiedon").ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));

                        //HandleDetail(oneMain, root);

                        main.Add(root);
                    }
                    rootOut.Add("main", main);
                    rv.data = rootOut;
                }
            }
            catch (Exception ex)
            {
                rv.code = ResponseCode.error;
                rv.msg = ex.Message;
            }
            return rv;
        }

        /// <summary>
        /// 获取单条数据
        /// new_ord_shipment 、 new_ord_shipmentline
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost]
        public WebRv GetOne(JObject json)
        {
            WebRv rv = new WebRv();
            try
            {
                var new_name = json.GetStr("new_name");
                var new_account_id = GetHeader("new_account_id");
                QueryExpression isExistMst = new QueryExpression("new_ord_shipment");
                isExistMst.ColumnSet = new ColumnSet("new_ord_shipmentid", "new_name", "new_shipping_date", "modifiedon");
                isExistMst.Criteria.AddCondition("new_name", ConditionOperator.Equal, new_name);
                isExistMst.Criteria.AddCondition("new_account_id", ConditionOperator.Equal, new Guid(new_account_id));

                LinkEntity contract = isExistMst.AddLink("new_contract", "new_contract_id", "new_contractid", JoinOperator.LeftOuter);
                contract.EntityAlias = "contract";
                contract.Columns.AddColumns("new_customercontractno", "new_contractno");//第一个是客户合同号

                var ec = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(isExistMst);
                if (ec != null && ec.Entities.Count > 0)
                {
                    var main = ec.Entities[0];
                    JObject root = new JObject();
                    root.Add("new_name", new_name);
                    if (main.Contains("new_shipping_date"))
                    {
                        root.Add("new_shipping_date", main.GetAttributeValue<DateTime>("new_shipping_date").ToLocalTime().ToString("yyyy-MM-dd"));
                    }
                    root.Add("new_customercontractno", main.GetAliasAttributeValue<string>("contract.new_customercontractno"));
                    root.Add("modifiedon", main.GetAttributeValue<DateTime>("modifiedon").ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));

                    HandleDetail(main, root);
                    rv.data = root;
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
        private static void HandleDetail(Entity main, JObject root)
        {
            QueryExpression qe_detail = new QueryExpression("new_ord_shipmentline");
            qe_detail.Criteria.AddCondition("new_ord_shipment_id", ConditionOperator.Equal, main.Id);
            qe_detail.ColumnSet = new ColumnSet("modifiedon", "new_qty", "new_productno");//修改时间，数量，产品编码

            LinkEntity product = qe_detail.AddLink("product", "new_productno", "productnumber", JoinOperator.LeftOuter);
            product.EntityAlias = "product";
            product.Columns.AddColumns("name", "productnumber", "new_brand", "new_oldcode", "new_drawingnumber");//产品名称、编码、品牌、旧料号、图号
                                                                                                                 //型号
            LinkEntity productmodel = product.AddLink("new_productmodel", "new_productmodel_id", "new_productmodelid", JoinOperator.LeftOuter);
            productmodel.EntityAlias = "productmodel";
            productmodel.Columns.AddColumns("new_name");
            //单位
            LinkEntity uom = qe_detail.AddLink("uom", "new_uom_id", "uomid", JoinOperator.LeftOuter);
            uom.EntityAlias = "uom";
            uom.Columns.AddColumns("name");
            var ec_detail = OrganizationServiceInstance.Instance.OrgService.RetrieveMultiple(qe_detail);
            if (ec_detail != null && ec_detail.Entities.Count > 0)
            {
                JArray detail = new JArray();
                //明细
                foreach (Entity oneLine in ec_detail.Entities)
                {
                    JObject oneRow = new JObject();
                    oneRow.Add("modifiedon", oneLine.GetAttributeValue<DateTime>("modifiedon").ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));//修改时间
                    if (oneLine.Contains("new_qty"))
                    {
                        oneRow.Add("new_qty", oneLine.GetAttributeValue<decimal>("new_qty"));//数量
                    }
                    oneRow.Add("productnumber", oneLine.GetAttributeValue<string>("new_productno"));//产品编码
                    oneRow.Add("productname", oneLine.GetAliasAttributeValue<string>("product.name"));//产品名称
                    oneRow.Add("new_brand", oneLine.GetAliasAttributeValue<string>("product.new_brand"));//品牌
                    oneRow.Add("new_oldcode", oneLine.GetAliasAttributeValue<string>("product.new_oldcode"));//旧料号
                    oneRow.Add("new_drawingnumber", oneLine.GetAliasAttributeValue<string>("product.new_drawingnumber"));//图号
                    oneRow.Add("productmodel", oneLine.GetAliasAttributeValue<string>("productmodel.new_name"));//产品型号
                    oneRow.Add("uom", oneLine.GetAliasAttributeValue<string>("uom.name"));//单位
                    detail.Add(oneRow);
                }
                root.Add("detail", detail);
            }
        }

        /// <summary>
        /// 获取请求头中的键值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [NonAction]
        private string GetHeader(string key)
        {
            string sign = HttpContext.Current.Request.Headers[key];
            if (string.IsNullOrEmpty(sign))
            {
                return "";
            }
            else
            {
                return sign;
            }
        }
    }
}