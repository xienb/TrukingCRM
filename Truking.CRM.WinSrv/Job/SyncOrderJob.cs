using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json.Linq;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.WinSrv.Job
{
    [DisallowConcurrentExecution]
    public class SyncOrderJob : IJob
    {
        IOrganizationService OrganizationServiceAdmin;
        public void Execute(IJobExecutionContext context)
        {
            //Log.Info("SyncOrderJob", "进入订单job：" + DateTime.Now.ToString());
            try
            {
                JObject reqJo = new JObject();
                var conStr = SqliteHelper.GetConfig("CRMConnect");
                CrmServiceClient conn = new CrmServiceClient(conStr);
                OrganizationServiceAdmin = (IOrganizationService)conn.OrganizationWebProxyClient ??
                                                      conn.OrganizationServiceProxy;
                string current = CommonHelper.GetSysPar(OrganizationServiceAdmin, "SAP_API_OrdSync_StartDT");
                DateTime currentDt = CommonHelper.ToDateTime(current);
                DateTime newTime = currentDt.AddMinutes(60);
                if (newTime > DateTime.Now)
                {
                    newTime = DateTime.Now;
                }
                reqJo.Add("FDATETIME", current);
                reqJo.Add("TDATETIME", newTime.ToString("yyyyMMddHHmmss"));
                string reqStr = reqJo.ToString();
                Log.Info("SyncOrderJob", "sap请求参数：" + reqStr);
                string resp = CommonHelper.GetSapOrder(OrganizationServiceAdmin, reqStr);
                Log.Info("SyncOrderJob", "sap返回：" + resp);
                //遍历每一条订单，全部保存成功以后，更新时间戳 
                //select new_name,new_value from new_systemparameter
                //SAP_API_OrdSync_StartDT  格式 yyyyMMddHHmmss
                JArray ordList = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(resp);
                if (ordList != null && ordList.Count > 0)
                {
                    int sucessCount = 0;//保存成功的条数
                    foreach (JObject oneOrd in ordList)
                    {
                        try
                        {
                            var oneSaveReq = new ExecuteTransactionRequest();
                            oneSaveReq.ReturnResponses = true;
                            var requestCollection = new OrganizationRequestCollection();

                            SaveData(oneOrd, requestCollection);

                            oneSaveReq.Requests = requestCollection;

                            OrganizationServiceAdmin.Execute(oneSaveReq);
                            sucessCount++;
                        }
                        catch (Exception ex)//有一条出错，继续执行
                        {
                            Log.Info("SyncOrderJob", "订单号" + oneOrd.GetStr("VBELN") + "出错");
                            Log.Error("SyncOrderJob", ex);
                            sucessCount++;
                            continue;
                        }
                    }
                    //更新时间戳
                    QueryExpression query = new QueryExpression("new_systemparameter");
                    query.Criteria.AddCondition("new_name", ConditionOperator.Equal, "SAP_API_OrdSync_StartDT");
                    EntityCollection entitylist = OrganizationServiceAdmin.RetrieveMultiple(query);
                    if (entitylist != null && entitylist.Entities.Count > 0)
                    {
                        Entity updateDt = entitylist.Entities[0];
                        updateDt["new_value"] = newTime.ToString("yyyyMMddHHmmss");
                        OrganizationServiceAdmin.Update(updateDt);
                    }
                }
                else
                {
                    //更新时间戳
                    QueryExpression query = new QueryExpression("new_systemparameter");
                    query.Criteria.AddCondition("new_name", ConditionOperator.Equal, "SAP_API_OrdSync_StartDT");
                    EntityCollection entitylist = OrganizationServiceAdmin.RetrieveMultiple(query);
                    if (entitylist != null && entitylist.Entities.Count > 0)
                    {
                        Entity updateDt = entitylist.Entities[0];
                        updateDt["new_value"] = newTime.ToString("yyyyMMddHHmmss");
                        OrganizationServiceAdmin.Update(updateDt);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("SyncOrderJob", ex);
            }
        }

        private void SaveData(JObject oneOrd, OrganizationRequestCollection orc)
        {
            string VBELN = oneOrd.GetStr("VBELN");
            if (!string.IsNullOrEmpty(VBELN))
            {
                //构建表头保存请求，根据VBELN 判断表头是修改还是新增，标体直接清空全部后逐条新建
                Entity mstEntity = null;
                QueryExpression isExistMst = new QueryExpression();
                isExistMst.EntityName = "new_ord_saleorder";
                isExistMst.Criteria.AddCondition("new_erpcode", ConditionOperator.Equal, VBELN);
                EntityCollection isExistList = OrganizationServiceAdmin.RetrieveMultiple(isExistMst);
                bool isCreate = true;
                if (isExistList != null && isExistList.Entities.Count > 0)
                {
                    isCreate = false;
                    mstEntity = isExistList.Entities[0];
                }
                else
                {
                    mstEntity = new Entity("new_ord_saleorder");
                    mstEntity.Id = Guid.NewGuid();
                }
                //订单号
                mstEntity["new_erpcode"] = VBELN;
                mstEntity["new_name"] = VBELN;
                //订单类型
                string AUART = oneOrd.GetStr("AUART");
                if (!string.IsNullOrEmpty(AUART))
                {
                    int? ii = CommonHelper.GetPicklistIntValue(OrganizationServiceAdmin, "new_ord_saleorder", "new_ordertype", AUART);
                    if (ii == null) ii = 999;
                    mstEntity["new_ordertype"] = new OptionSetValue(ii.Value);
                }
                //销售组织
                string VKORG = oneOrd.GetStr("VKORG");
                if (!string.IsNullOrEmpty(VKORG))
                {
                    Guid deptGuid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "businessunit", "new_department_code", VKORG);
                    if (deptGuid != Guid.Empty)
                    {
                        mstEntity["new_depart_id"] = new EntityReference("businessunit", deptGuid);
                    }
                }
                //分销渠道
                string VTWEG = oneOrd.GetStr("VTWEG");
                if (!string.IsNullOrEmpty(VTWEG))
                {
                    Guid deptGuid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "new_channel", "new_code", VTWEG);
                    if (deptGuid != Guid.Empty)
                    {
                        mstEntity["new_channel_id"] = new EntityReference("new_channel", deptGuid);
                    }
                }
                //产品组
                string SPART = oneOrd.GetStr("SPART");
                if (!string.IsNullOrEmpty(SPART))
                {
                    Guid deptGuid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "new_productgroup", "new_code", SPART);
                    if (deptGuid != Guid.Empty)
                    {
                        mstEntity["new_productgroup_id"] = new EntityReference("new_productgroup", deptGuid);
                    }
                }
                //售达方
                string KUNNR = oneOrd.GetStr("KUNNR");
                if (!string.IsNullOrEmpty(KUNNR))
                {
                    Guid guid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "account", "new_sapcode", KUNNR);
                    if (guid != Guid.Empty)
                    {
                        mstEntity["new_account_id"] = new EntityReference("account", guid);
                    }
                }
                //公司代码
                mstEntity["new_corpcode"] = oneOrd.GetStr("BUKRS");
                //合同号
                mstEntity["new_contractno"] = oneOrd.GetStr("BSTNK");

                //查询销售合同
                QueryExpression contactQE = new QueryExpression();
                contactQE.ColumnSet = new ColumnSet("new_contractid", "ownerid");
                contactQE.EntityName = "new_contract";
                contactQE.Criteria.AddCondition("new_customercontractno", ConditionOperator.Equal, oneOrd.GetStr("BSTNK"));
                EntityCollection contactEC = OrganizationServiceAdmin.RetrieveMultiple(contactQE);
                if (contactEC != null && contactEC.Entities.Count > 0)
                {
                    mstEntity["new_contract_id"] = new EntityReference("new_contract", contactEC.Entities[0].Id);
                    mstEntity["ownerid"] = contactEC.Entities[0].GetAttributeValue<EntityReference>("ownerid");
                }
                else
                {
                    Log.Info("SyncOrderJob", "未找到合同：" + oneOrd.GetStr("BSTNK"));
                }

                //创建日期
                string AUDAT = oneOrd.GetStr("AUDAT");
                if (!string.IsNullOrEmpty(AUDAT))
                {
                    mstEntity["new_orderdate"] = Convert.ToDateTime(AUDAT.Substring(0, 4) + "-" + AUDAT.Substring(4, 2) + "-" + AUDAT.Substring(6, 2));
                }
                //订单总额
                decimal NETWR = (decimal)oneOrd["NETWR"];
                mstEntity["new_totalamount"] = new Money(NETWR);
                //币种
                string WAERS = oneOrd.GetStr("WAERS");
                if (!string.IsNullOrEmpty(WAERS))
                {
                    Guid guid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "transactioncurrency", "isocurrencycode", WAERS);
                    if (guid != Guid.Empty)
                    {
                        mstEntity["transactioncurrencyid"] = new EntityReference("transactioncurrency", guid);
                    }
                }

                //出口报关类型
                string ZSDECKBG = oneOrd.GetStr("ZSDECKBG");
                if (!string.IsNullOrEmpty(ZSDECKBG))
                {
                    mstEntity["new_exportdeclaration"] = new OptionSetValue(CommonHelper.GetPicklistIntValue(OrganizationServiceAdmin, "new_ord_saleorder", "new_exportdeclaration", ZSDECKBG).Value);
                }

                //项目号
                mstEntity["new_itemno"] = oneOrd.GetStr("KUNNR");

                //送达方
                string KUNNR_WE = oneOrd.GetStr("KUNNR_WE");
                if (!string.IsNullOrEmpty(KUNNR))
                {
                    Guid guid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "account", "new_sapcode", KUNNR_WE);
                    if (guid != Guid.Empty)
                    {
                        mstEntity["new_shipto_account_id"] = new EntityReference("account", guid);
                    }
                }
                //开票方
                string KUNRE = oneOrd.GetStr("KUNRE");
                if (!string.IsNullOrEmpty(KUNRE))
                {
                    Guid guid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "account", "new_sapcode", KUNRE);
                    if (guid != Guid.Empty)
                    {
                        mstEntity["new_invoiceaccount_id"] = new EntityReference("account", guid);
                    }
                }
                //付款方
                string KUNRG = oneOrd.GetStr("KUNRG");
                if (!string.IsNullOrEmpty(KUNRG))
                {
                    Guid guid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "account", "new_sapcode", KUNRG);
                    if (guid != Guid.Empty)
                    {
                        mstEntity["new_payaccount_id"] = new EntityReference("account", guid);
                    }
                }


                if (isCreate)
                {
                    orc.Add(new CreateRequest() { Target = mstEntity });
                }
                else
                {
                    orc.Add(new UpdateRequest() { Target = mstEntity });
                }

                //构建明细保存请求
                SaveDetail(oneOrd, orc, mstEntity);
            }
        }
        /// <summary>
        /// 组装明细
        /// </summary>
        /// <param name="oneOrd"></param>
        /// <param name="orc"></param>
        /// <param name="mstEntity"></param>
        private void SaveDetail(JObject oneOrd, OrganizationRequestCollection orc, Entity mstEntity)
        {
            if (oneOrd.Property("SALEITEMS") != null)
            {
                JArray detailData = oneOrd["SALEITEMS"] as JArray;
                //先删除所有明细
                QueryExpression delQuery = new QueryExpression("new_ord_saleorderline");
                delQuery.Criteria.AddCondition("new_ord_salesorder_id", ConditionOperator.Equal, mstEntity.Id);
                EntityCollection delList = OrganizationServiceAdmin.RetrieveMultiple(delQuery);
                if (delList.Entities != null && delList.Entities.Count > 0)
                {
                    return;//无法对明细修改，因为有可能单据为非制单状态
                    foreach (var delOne in delList.Entities)
                    {
                        orc.Add(new DeleteRequest() { Target = new EntityReference("new_ord_saleorderline", delOne.Id) });
                    }
                }
                foreach (JObject line in detailData)
                {
                    Entity entityDetail = new Entity("new_ord_saleorderline");
                    entityDetail["new_ord_salesorder_id"] = new EntityReference("new_ord_saleorder", mstEntity.Id);
                    entityDetail["new_erp_seq"] = line.GetStr("POSNR");
                    entityDetail["new_productno"] = line.GetStr("MATNR");//产品编码
                    //new_product_id，根据产品编码找产品
                    if (!string.IsNullOrEmpty(line.GetStr("MATNR")))
                    {
                        Guid guid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "product", "productnumber", line.GetStr("MATNR"));
                        if (guid != Guid.Empty)
                        {
                            entityDetail["new_product_id"] = new EntityReference("product", guid);
                        }
                        else
                        {

                        }
                    }
                    if (mstEntity.Contains("ownerid"))
                    {
                        entityDetail["ownerid"] = mstEntity["ownerid"];
                    }

                    entityDetail["new_qty"] = (decimal)line["KWMENG"];

                    entityDetail["new_amount_base"] = new Money((decimal)line["KZWI1"]);
                    entityDetail["new_taxamount"] = new Money((decimal)line["MWSBP"]);
                    entityDetail["new_price"] = new Money((decimal)line["ZSDFXSDJ"]);
                    //单位
                    string VRKME_detail = line.GetStr("VRKME");
                    if (!string.IsNullOrEmpty(VRKME_detail))
                    {
                        Guid guid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "uom", "name", VRKME_detail);
                        if (guid != Guid.Empty)
                        {
                            entityDetail["new_uom_id"] = new EntityReference("uom", guid);
                        }
                    }

                    //货币
                    string WAERS_detail = line.GetStr("WAERS");
                    if (!string.IsNullOrEmpty(WAERS_detail))
                    {
                        Guid guid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "transactioncurrency", "isocurrencycode", WAERS_detail);
                        if (guid != Guid.Empty)
                        {
                            entityDetail["transactioncurrencyid"] = new EntityReference("transactioncurrency", guid);
                        }
                    }

                    orc.Add(new CreateRequest() { Target = entityDetail });
                }
            }
        }

    }
}
