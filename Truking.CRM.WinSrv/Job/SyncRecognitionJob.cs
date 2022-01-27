using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
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
    public class SyncRecognitionJob : IJob
    {
        IOrganizationService OrganizationServiceAdmin;
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                //Log.Info("SyncRecognitionJob", "jobin");
                var conStr = SqliteHelper.GetConfig("CRMConnect");
                CrmServiceClient conn = new CrmServiceClient(conStr);
                OrganizationServiceAdmin = (IOrganizationService)conn.OrganizationWebProxyClient ??
                                                      conn.OrganizationServiceProxy;
                string current = CommonHelper.GetSysPar(OrganizationServiceAdmin, "SAP_API_RecognitionSync_StartDT");
                DateTime currentDt = CommonHelper.ToDateTime2(current);
                DateTime newTime = currentDt.AddDays(1);
                if (newTime > DateTime.Now)
                {
                    newTime = DateTime.Now;
                }

                JObject reqJo = new JObject();
                reqJo.Add("BUKRS", "1000");
                reqJo.Add("BBUDAT", current);
                reqJo.Add("EBUDAT", newTime.ToString("yyyyMMdd"));
                string reqStr = reqJo.ToString();
                string resp = CommonHelper.GetRecognition(OrganizationServiceAdmin, "[" + reqStr + "]");
                Log.Info("SyncRecognitionJob", "请求参数:" + reqStr + "SAP返回数据:" + resp);
                JArray arrayList = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(resp);
                if (arrayList != null && arrayList.Count > 0)
                {
                    //批量提交
                    var oneSaveReq = new ExecuteMultipleRequest();
                    oneSaveReq.Settings = new ExecuteMultipleSettings()
                    {
                        ContinueOnError = true,
                        ReturnResponses = true
                    };
                    //var requestCollection = new OrganizationRequestCollection();

                    //按认款凭证编码（BELNR）更新或者新增
                    foreach (JObject oneOrd in arrayList)
                    {
                        try
                        {
                            var BELNR = oneOrd.GetStr("BELNR");//认款凭证编码
                            var BUKRS = oneOrd.GetStr("BUKRS");//公司代码
                            var KUNNR = oneOrd.GetStr("KUNNR");//客户
                            var BLART = oneOrd.GetStr("BLART");//凭证类型
                            var BUDAT = oneOrd.GetStr("BUDAT");//认款日期
                            var VBELN = oneOrd.GetStr("VBELN");//销售订单
                            var AUFNR = oneOrd.GetStr("AUFNR");//内部订单
                            var BSTNK = oneOrd.GetStr("BSTNK");//销售合同
                            var WRBTR = Convert.ToDecimal(oneOrd.GetStr("WRBTR"));//认款金额 业务货币
                            var DMBTR = Convert.ToDecimal(oneOrd.GetStr("DMBTR"));//认款金额 本币
                            var ZSDFJDMC = oneOrd.GetStr("ZSDFJDMC");//款项阶段
                            var WAERS = oneOrd.GetStr("WAERS");//币种
                            var BKTXT = oneOrd.GetStr("BKTXT");//摘要

                            if (String.IsNullOrEmpty(KUNNR)) continue;//临时加上，应该让sap修改

                            Entity mstEntity = null;
                            QueryExpression isExistMst = new QueryExpression("new_sales_testapplyline");
                            isExistMst.Criteria.AddCondition("new_name", ConditionOperator.Equal, BELNR);
                            EntityCollection isExistList = OrganizationServiceAdmin.RetrieveMultiple(isExistMst);
                            bool isCreate = true;
                            if (isExistList != null && isExistList.Entities.Count > 0)
                            {
                                isCreate = false;
                                mstEntity = isExistList.Entities[0];
                            }
                            else
                            {
                                mstEntity = new Entity("new_sales_testapplyline");
                                mstEntity.Id = Guid.NewGuid();
                            }
                            mstEntity["new_name"] = BELNR;
                            mstEntity["new_companycode"] = BUKRS;
                            Guid accountid = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "account", "new_sapcode", KUNNR);
                            if (accountid != Guid.Empty)
                            {
                                mstEntity["new_account_id"] = new EntityReference("account", accountid);
                            }
                            int? pzlx = CommonHelper.GetPicklistIntValue(OrganizationServiceAdmin, "new_sales_testapplyline", "new_documenttype", BLART);
                            if (pzlx != null)
                            {
                                mstEntity["new_documenttype"] = new OptionSetValue(pzlx.Value);
                            }
                            mstEntity["new_date"] = CommonHelper.GetSapTime(BUDAT);
                            mstEntity["new_erporderno"] = VBELN;
                            Guid saleOrderId = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "new_ord_saleorder", "new_erpcode", VBELN);
                            if (saleOrderId != Guid.Empty)
                            {
                                mstEntity["new_ord_saleorder_id"] = new EntityReference("new_ord_saleorder", saleOrderId);
                            }
                            mstEntity["new_ordernumber"] = AUFNR;


                            //查询销售合同
                            QueryExpression contactQE = new QueryExpression();
                            contactQE.ColumnSet = new ColumnSet("new_contractid", "ownerid");
                            contactQE.EntityName = "new_contract";
                            contactQE.Criteria.AddCondition("new_customercontractno", ConditionOperator.Equal, BSTNK);
                            EntityCollection contactEC = OrganizationServiceAdmin.RetrieveMultiple(contactQE);
                            if (contactEC != null && contactEC.Entities.Count == 1)
                            {
                                mstEntity["new_contract_id"] = new EntityReference("new_contract", contactEC.Entities[0].Id);
                                mstEntity["ownerid"] = contactEC.Entities[0].GetAttributeValue<EntityReference>("ownerid");
                            }
                            //Guid contractId = GetLookUpGuidByStringCode(OrganizationServiceAdmin, "new_contract", "new_customercontractno", BSTNK);


                            mstEntity["new_amount_base"] = new Money(WRBTR);
                            int? kxjd = CommonHelper.GetPicklistIntValue(OrganizationServiceAdmin, "new_sales_testapplyline", "new_paymentstage", ZSDFJDMC);
                            if (kxjd != null)
                            {
                                mstEntity["new_paymentstage"] = new OptionSetValue(kxjd.Value);
                            }
                            Guid transId = CommonHelper.GetLookUpGuidByStringCode(OrganizationServiceAdmin, "transactioncurrency", "isocurrencycode", WAERS);
                            if (transId != Guid.Empty)
                            {
                                mstEntity["transactioncurrencyid"] = new EntityReference("transactioncurrency", transId);
                            }
                            mstEntity["new_summary"] = BKTXT;
                            if (isCreate)
                            {
                                //requestCollection.Add(new CreateRequest() { Target = mstEntity });
                                OrganizationServiceAdmin.Create(mstEntity);
                            }
                            else
                            {
                                //requestCollection.Add(new UpdateRequest() { Target = mstEntity });
                                OrganizationServiceAdmin.Update(mstEntity);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("SyncRecognitionJob", ex);
                            continue;
                        }
                    }

                    //oneSaveReq.Requests = requestCollection;
                    //var respExecute = OrganizationServiceAdmin.Execute(oneSaveReq);//请求结果
                    //Log.InfoMsg("同步认款数据保存返回："+Newtonsoft.Json.JsonConvert.SerializeObject(respExecute.Results));
                }

                //更新时间戳
                QueryExpression query = new QueryExpression("new_systemparameter");
                query.Criteria.AddCondition("new_name", ConditionOperator.Equal, "SAP_API_RecognitionSync_StartDT");
                EntityCollection entitylist = OrganizationServiceAdmin.RetrieveMultiple(query);

                if (entitylist != null && entitylist.Entities.Count > 0)
                {
                    Entity updateDt = entitylist.Entities[0];
                    updateDt["new_value"] = newTime.ToString("yyyyMMdd");
                    OrganizationServiceAdmin.Update(updateDt);
                }


            }
            catch (Exception ex)
            {
                Log.Error("SyncRecognitionJob", ex);
            }
        }
    }
}
