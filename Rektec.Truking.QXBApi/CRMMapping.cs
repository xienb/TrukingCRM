using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json.Linq;
using Rektec.Truking.QXBApi.Api;
using Truking.CRM.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rektec.Truking.QXBApi
{
    /// <summary>
    /// 实现CRM数据保存（提供给HiddenApi调用）
    /// </summary>
    public class CRMMapping
    {
        IOrganizationService org;
        ILog log;
        string _appkey;
        string _secretkey;

        public CRMMapping(IOrganizationService organizationService, ILog Log, string appkey, string secretkey)
        {
            org = organizationService;
            log = Log;
            _appkey = appkey;
            _secretkey = secretkey;
        }

        /// <summary>
        /// 查询实体的某一个字段，获得该实体的guid
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        private Guid GetLookUpGuidByStringCode(string entityName, string attributeName, string attributeValue)
        {
            QueryExpression isExistMst = new QueryExpression();
            isExistMst.ColumnSet = new ColumnSet(entityName + "id");
            isExistMst.EntityName = entityName;
            isExistMst.Criteria.AddCondition(attributeName, ConditionOperator.Equal, attributeValue);
            EntityCollection isExistList = org.RetrieveMultiple(isExistMst);
            if (isExistList != null && isExistList.Entities.Count > 0)
            {
                return isExistList.Entities[0].Id;
            }
            else
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// 保存工商照面信息到客户单位，如果是更新则只保存主档，明细
        /// </summary>
        /// <param name="root">工商照面数据</param>
        /// <param name="saveDetail">当客户已存在，是否强制刷新明细</param>
        /// <returns></returns>
        public bool SaveQXBAccount(string new_taxno)
        {
            bool rv = true;
            try
            {
                QueryExpression qe = new QueryExpression("account");
                qe.Criteria.AddCondition("new_taxno", ConditionOperator.Equal, new_taxno);
                qe.ColumnSet = new ColumnSet("accountid", "name");
                var ec = org.RetrieveMultiple(qe);
                Entity entity = null;
                if (ec != null && ec.Entities.Count > 0)
                {
                    //修改
                    entity = ec.Entities[0];


                    NOPageApi api = new NOPageApi(_appkey, _secretkey, QXB.api_1_41);
                    var resp = api.ExcuteRequest(new_taxno);
                    JObject data = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(resp);
                    //调用1.16
                    var json_1_16 = new NOPageApi(_appkey, _secretkey, QXB.api_1_16).ExcuteRequest(data["data"]["creditNo"].ToString()).ToQxbRv();
                    if (json_1_16.status == "200")
                    {
                        data["data"]["description"] = (json_1_16.data as JObject)["brief"].ToString();
                    }
                    else
                    {
                        data["data"]["description"] = "";
                    }
                    //调用1.51获取联系方式
                    var json_1_51 = new NOPageApi(_appkey, _secretkey, QXB.api_1_51).ExcuteRequest(data["data"]["creditNo"].ToString()).ToQxbRv();
                    if (json_1_51.status == "200")
                    {
                        data["data"]["emailaddress1"] = (json_1_51.data as JObject)["email"].ToString();
                        data["new_telephone"] = (json_1_51.data as JObject)["telephone"].ToString();
                    }
                    else
                    {
                        data["data"]["emailaddress1"] = "";
                        data["data"]["new_telephone"] = "";
                    }
                    JObject root = data["data"] as JObject;
                    string id = root.GetStr("id");
                    //string name = root.GetStr("name");
                    //string creditNo = root.GetStr("creditNo");

                    string districtCode = root.GetStr("districtCode");
                    var provinceId = GetLookUpGuidByStringCode("new_province", "new_code", districtCode.Substring(0, 2) + "0000");
                    if (provinceId != Guid.Empty)
                    {
                        entity["new_province_id"] = new EntityReference("new_province", provinceId);
                    }
                    var cityId = GetLookUpGuidByStringCode("new_city", "new_code", districtCode.Substring(0, 4) + "00");
                    if (cityId != Guid.Empty)
                    {
                        entity["new_city_id"] = new EntityReference("new_city", cityId);
                    }
                    var countyID = GetLookUpGuidByStringCode("new_county", "new_code", districtCode);
                    if (countyID != Guid.Empty)
                    {
                        entity["new_county_id"] = new EntityReference("new_county", countyID);
                    }
                    entity["new_enterpriseid"] = id;
                    entity["new_synchronizeinformation"] = true;

                    entity["new_enterprisekind"] = root.GetStr("econKind");//企业类型
                    entity["new_registeredcapital"] = root.GetStr("registCapi");//注册资本

                    var hArray = root["historyNames"] as JArray;
                    string historyNames = "";
                    if (hArray != null && hArray.Count > 0)
                    {
                        List<string> hList = new List<string>();
                        foreach (var vv in hArray)
                        {
                            hList.Add(vv.ToString());
                        }
                        historyNames = string.Join(",", hList.ToArray());
                    }
                    entity["new_usedname"] = historyNames;//历史名称
                    entity["new_address"] = root.GetStr("address");//地址
                    entity["new_registercode"] = root.GetStr("regNo");//企业注册号
                    entity["new_businessscope"] = root.GetStr("scope");//经营范围
                    entity["new_startdate"] = root.GetStr("termStart");//执照生效日期
                    entity["new_enddate"] = root.GetStr("termEnd");//执照到期日期
                    entity["new_commercialbureau"] = root.GetStr("belongOrg");//所属工商局
                    entity["new_corporaterepresentative"] = root.GetStr("operName");//法人
                    string startDate = root.GetStr("startDate");
                    if (startDate != "-" && startDate != "")
                    {
                        entity["new_startdate"] = startDate;
                    }
                    string endDate = root.GetStr("endDate");
                    if (endDate != "-" && endDate != "")
                    {
                        entity["new_enddate"] = startDate;
                    }

                    entity["new_operatingstate"] = root.GetStr("new_status");//状态
                    entity["new_organizationcode"] = root.GetStr("orgNo");//组织号
                    entity["new_businesslicense"] = root.GetStr("creditNo");//统一社会信用代码

                    org.Update(entity);

                    AccountInfo accountInfo = new AccountInfo();
                    accountInfo.id = entity.Id;
                    accountInfo.creditNo = new_taxno;
                    accountInfo.name = root.GetStr("name");
                    UpdateAccountDetail(accountInfo);
                    rv = true;
                }
                else
                {
                    rv = false;
                    log.InfoMsg("QXB税号不存在");
                }
                return rv;
            }
            catch (Exception ex)
            {
                log.ErrorMsg("QXB更新报错：" + ex.Message + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 更新客户所有子表
        /// </summary>
        /// <param name="accountId"></param>
        public void UpdateAccountDetail(AccountInfo account)
        {
            new Api_1_47(_appkey, _secretkey, QXB.api_1_47, "new_acc_changerecord", org, log).Update(account);//1.47 变更记录
            UpdateEquityStructureByName(account);//44.2 股权穿透（三层）
            new Api_1_55(_appkey, _secretkey, QXB.api_1_55, "new_acc_abnormaloperation", org, log).Update(account);//1.55 经营异常
            new Api_1_43(_appkey, _secretkey, QXB.api_1_43, "new_acc_industrialshareholders", org, log).Update(account);//1.43 工商股东
            UpdateSumLawsuit(account);//6.6 整体诉讼
            new Api_1_49(_appkey, _secretkey, QXB.api_1_49, "new_acc_branch", org, log).Update(account);//1.49 分支机构
            UpdateInvestmentByName(account);//28.6 对外投资

            new Api_51_1(_appkey, _secretkey, QXB.api_51_1, "new_acc_protectionpenalties", org, log).Update(account);//51.1 环保处罚列表
            new Api_32_1(_appkey, _secretkey, QXB.api_32_1, "new_acc_administrativesanction", org, log).Update(account); //32.1 行政处罚
            new Api_21_4(_appkey, _secretkey, QXB.api_21_4, "new_acc_biddingdetails", org, log).Update(account);//21.6招投标详情
            new Api_50_5(_appkey, _secretkey, QXB.api_50_5, "new_acc_coreteam", org, log).Update(account);//50.5 核心团队
            new Api_51_4(_appkey, _secretkey, QXB.api_51_4, "new_acc_bankruptcyinformation", org, log).Update(account);//51.4 破产信息

            new Api_20_1(_appkey, _secretkey, QXB.api_20_1, "new_acc_arrearsinformation", org, log).Update(account);//20.1 欠税信息 new_acc_arrearsinformation
            new Api_20_3(_appkey, _secretkey, QXB.api_20_3, "new_acc_taxviolations", org, log).Update(account);//20.3 重大税收违法
            UpdateFinancingByName(account);//37.3 融资信息 new_acc_financinginformation
            new Api_25_1(_appkey, _secretkey, QXB.api_25_1, "new_acc_chattelmortgage", org, log).Update(account);//25.1 动产抵押  new_acc_chattelmortgage
            new Api_26_1(_appkey, _secretkey, QXB.api_26_1, "new_acc_equitypledge", org, log).Update(account);//26.1 股权出质  new_acc_equitypledge
            new Api_34_1(_appkey, _secretkey, QXB.api_34_1, "new_acc_equityfreeze", org, log).Update(account);//34.1 股权冻结  new_acc_equityfreeze
        }

        /// <summary>
        /// 44.2 股权穿透（三层）
        /// 该接口没有体现父子结构
        /// </summary>
        /// <param name="account"></param>
        public void UpdateEquityStructureByName(AccountInfo account)
        {
            var api = new NOPageApi(_appkey, _secretkey, QXB.api_44_2, "new_acc_equitypenetration");
            log.InfoMsg("44.2 股权穿透");
            api.DeleteAll(account);
            var resp = api.ExcuteRequest(account.creditNo).ToQxbRv();
            if (resp.status == "200")
            {
                JObject root = resp.data as JObject;
                JObject actual_controller = root["actual_controller"] as JObject;
                string name = actual_controller.GetStr("name");//实际控股人
                string percent = actual_controller.GetStr("percent");//实际控股人比例
                JArray items = root["equity_structure"]["children"] as JArray;
                foreach (JObject it in items)
                {
                    var new_level = it.GetStr("level");//层级
                    var new_childreneid = it.GetStr("eid");//股东ID
                    var new_childrenname = it.GetStr("name");//股东名称
                    var new_childrentype = it.GetStr("type");//股东类型
                    var new_childrenshortname = it.GetStr("short_name");//股东简称
                    var new_childamount = it.GetStr("amount");//股东出资金额
                    var new_childrenpercent = it.GetStr("percent");//股东出资比例
                    var new_childrenshtype = it.GetStr("sh_type");//类型

                    QueryExpression qe = new QueryExpression("new_acc_equitypenetration");
                    qe.Criteria.AddCondition("new_account_id", ConditionOperator.Equal, account.id);
                    qe.Criteria.AddCondition("new_childreneid", ConditionOperator.Equal, new_childreneid);
                    qe.ColumnSet.AddColumn("new_acc_equitypenetrationid");
                    var ec = org.RetrieveMultiple(qe);
                    Entity entity = null;
                    bool isNew = true;
                    if (ec != null && ec.Entities.Count > 0)
                    {
                        isNew = false;
                        entity = ec.Entities[0];
                    }
                    else
                    {
                        entity = new Entity("new_acc_equitypenetration");
                    }
                    entity["new_account_id"] = new EntityReference("account", account.id);
                    entity["new_name"] = name;
                    entity["new_percent"] = percent;
                    entity["new_level"] = new_level;
                    entity["new_childreneid"] = new_childreneid;
                    entity["new_childrenname"] = new_childrenname;
                    entity["new_childrentype"] = new_childrentype;
                    entity["new_childrenshortname"] = new_childrenshortname;
                    entity["new_childamount"] = new_childamount;
                    entity["new_childrenpercent"] = new_childrenpercent;
                    entity["new_childrenshtype"] = new_childrenshtype;
                    if (isNew)
                    {
                        org.Create(entity);
                    }
                    else
                    {
                        org.Update(entity);
                    }
                }
            }
        }

        /// <summary>
        /// 6.6 整体诉讼
        /// </summary>
        /// <param name="account"></param>
        public void UpdateSumLawsuit(AccountInfo account)
        {
            var api6_6 = new NOPageApi(_appkey, _secretkey, QXB.api_6_6, "new_acc_overalllitigation", org);
            api6_6.DeleteAll(account);//删除所有
            var resp = api6_6.ExcuteRequest(account.creditNo).ToQxbRv();
            JObject root = resp.data as JObject;
            var fygg = root.GetStr("fygg");
            var shixin = root.GetStr("shixin");
            var ktgg = root.GetStr("ktgg");
            var consumerResult = root.GetStr("consumerResult");
            var total = root.GetStr("total");
            var cpws = root.GetStr("cpws");
            var terminationCaseResult = root.GetStr("terminationCaseResult");
            var lian = root.GetStr("lian");
            var zxgg = root.GetStr("zxgg");
            Entity entity = new Entity("new_acc_overalllitigation");
            entity["new_account_id"] = new EntityReference("account", account.id);

            entity["new_fygg"] = fygg;
            entity["new_shixin"] = shixin;
            entity["new_ktgg"] = ktgg;
            entity["new_consumerresult"] = consumerResult;
            entity["new_total"] = total;
            entity["new_cpws"] = cpws;
            entity["new_terminationcaseresult"] = terminationCaseResult;
            entity["new_lian"] = lian;
            entity["new_zxgg"] = zxgg;
            org.Create(entity);
        }

        /// <summary>
        /// 28.6 对外投资
        /// </summary>
        /// <param name="account"></param>
        public void UpdateInvestmentByName(AccountInfo account)
        {
            var api_28_6 = new NOPageApi(_appkey, _secretkey, QXB.api_28_6, "new_acc_outboundinvestment", org);
            api_28_6.DeleteAll(account);
            var resp = api_28_6.ExcuteRequest(account.creditNo).ToQxbRv();
            JArray root = resp.data as JArray;
            foreach (JObject oneRow in root)
            {
                var enterprise = oneRow.GetStr("enterprise");
                var short_name = oneRow.GetStr("short_name");
                var amount = oneRow.GetStr("amount");
                var sh_type = oneRow.GetStr("sh_type");
                var percent = oneRow.GetStr("percent");
                Entity entity = new Entity("new_acc_outboundinvestment");
                entity["new_account_id"] = new EntityReference("account", account.id);
                entity["new_enterprise"] = enterprise;
                entity["new_shortname"] = short_name;
                entity["new_amount2"] = amount;
                entity["new_percent"] = percent;
                entity["new_shtype"] = sh_type;
                org.Create(entity);
            }
        }

        /// <summary>
        /// 37.3 融资信息
        /// </summary>
        /// <param name="account"></param>
        public void UpdateFinancingByName(AccountInfo account)
        {
            var api_37_3 = new NOPageApi(_appkey, _secretkey, QXB.api_37_3, "new_acc_financinginformation", org);
            api_37_3.DeleteAll(account);
            var resp = api_37_3.ExcuteRequest(account.creditNo).ToQxbRv();
            if (resp.status == "200")
            {
                JObject root = resp.data as JObject;
                JArray financing_list = root["financing_list"] as JArray;
                if (financing_list != null && financing_list.Count > 0)
                {
                    foreach (JObject oneRow in financing_list)
                    {
                        var financing_amount = oneRow.GetStr("financing_amount");
                        var finance_date = oneRow.GetStr("finance_date");
                        var financing_round = oneRow.GetStr("financing_round");
                        var investors = oneRow["investors"] as JArray;
                        var all_investors = "";
                        foreach (JObject oneInv in investors)
                        {
                            if (string.IsNullOrEmpty(all_investors))
                            {
                                all_investors += oneInv.GetStr("org_name");
                            }
                            else
                            {
                                all_investors += ("," + oneInv.GetStr("org_name"));
                            }
                        }
                        Entity entity = new Entity("new_acc_financinginformation");
                        entity["new_account_id"] = new EntityReference("account", account.id);
                        if (finance_date != "" && finance_date != "-")
                        {
                            entity["new_date"] = Convert.ToDateTime(finance_date);
                        }
                        entity["new_financinground"] = financing_round;
                        entity["new_financingamount2"] = financing_amount;
                        entity["new_investorsorgname"] = all_investors;
                        org.Create(entity);
                    }
                }
            }
        }
    }
}
