using Microsoft.Xrm.Sdk;
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
    public class MatJob : IJob
    {
        IOrganizationService OrganizationService;
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                //Log.Info("MatJob", "进入物料job：" + DateTime.Now.ToString());
                var conStr = SqliteHelper.GetConfig("CRMConnect");
                CrmServiceClient conn = new CrmServiceClient(conStr);
                OrganizationService = (IOrganizationService)conn.OrganizationWebProxyClient ??
                                                      conn.OrganizationServiceProxy;
                QueryExpression qe_i = new QueryExpression("new_biz_log");
                qe_i.ColumnSet = new ColumnSet("new_biz_logid", "new_biz", "new_lev", "new_msg");
                qe_i.Criteria.AddCondition("new_biz", ConditionOperator.Equal, "SAPSaveProductNew");
                qe_i.Criteria.AddCondition("new_lev", ConditionOperator.Equal, "info");//导入完成以后改成end
                qe_i.PageInfo = new PagingInfo()
                {
                    PageNumber = 1,
                    Count = 2
                };//分页参数
                qe_i.AddOrder("createdon", OrderType.Ascending);//按时间
                var entityCollection_i = OrganizationService.RetrieveMultiple(qe_i);
                if (entityCollection_i.Entities.Count == 0)
                {
                }
                else
                {
                    //Log.Info("MatJob","查询完成："+DateTime.Now.ToString());
                    Page(entityCollection_i);
                }
            }
            catch (Exception e)
            {
                Log.Error("MatJob", e);
            }
        }

        public  void Page(EntityCollection ec)
        {
            foreach (Entity en in ec.Entities)
            {
                Save(en.GetAttributeValue<string>("new_msg"));
                Entity updateLev = new Entity("new_biz_log");
                updateLev.Id = en.Id;
                updateLev["new_lev"] = "end";
                OrganizationService.Update(updateLev);
            }
        }

        private  void Save(string modelStr)
        {
            try
            {
                //var oneSaveReq = new ExecuteMultipleRequest();
                //oneSaveReq.Settings = new ExecuteMultipleSettings()
                //{
                //    ContinueOnError = true,
                //    ReturnResponses = true
                //};
                //var requestCollection = new OrganizationRequestCollection();

                JArray rootJarray = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(modelStr);
                Console.WriteLine($"本条记录总条数：{rootJarray.Count}");
                foreach (JObject rootModel in rootJarray)
                {
                    try
                    {
                        var MATNR = GetStr(rootModel, "MATNR");//物料编码
                        var MTART = GetStr(rootModel, "MTART");//物料类型
                        var MEINS = GetStr(rootModel, "MEINS");//基本单位
                        var ZTUHAO = GetStr(rootModel, "ZTUHAO");//图号
                        var ZPINPAI = GetStr(rootModel, "ZPINPAI");//品牌
                        var ZGUIGE = GetStr(rootModel, "ZGUIGE");//规格
                        var ZTYPE = GetStr(rootModel, "ZTYPE");//自制/外购
                        var ZSFJM = GetStr(rootModel, "ZSFJM");//是否加贸
                        var MAKTX = GetStr(rootModel, "MAKTX");//物料描述
                        var BISMT = GetStr(rootModel, "BISMT");//旧料号

                        var PRODH1 = GetStr(rootModel, "PRODH1");//物料大类编码
                        var PRODH1_TEXT = GetStr(rootModel, "PRODH1_TEXT");//物料大类名称
                        var PRODH2 = GetStr(rootModel, "PRODH2");//物料中类编码
                        var PRODH2_TEXT = GetStr(rootModel, "PRODH2_TEXT");//物料中类名称
                        var PRODH3 = GetStr(rootModel, "PRODH3");//物料小类编码
                        var PRODH3_TEXT = GetStr(rootModel, "PRODH3_TEXT");//物料小类名称
                        var SPART = GetStr(rootModel, "SPART");//产品组编码

                        var LONGTEXT = GetStr(rootModel, "LONGTEXT");//长名称，里面包含所有参数

                        //if (MTART == "TK08" || MTART == "TK09" || (MTART != "TK08" && SPART == "99"))
                        //{
                        //创建产品系列（大中小），并返回产品小类的guid
                        //创建物料组（小），并返回物料组guid
                        //产品组根据编码获得产品组guid
                        //创建产品型号，通过sap图号匹配crm型号名称
                        Guid xlGuid = CreateOrGetProductSeries(PRODH1, PRODH1_TEXT, PRODH2, PRODH2_TEXT, PRODH3, PRODH3_TEXT);
                        //Guid wlGuid = CreateOrGetMatGroup(PRODH3, PRODH3_TEXT);
                        Guid cpzGuid = GetProductGroupGuid(SPART);
                        //Guid xhGuid = CreateOrGetProductModel(cpzGuid, ZGUIGE);//不再创建产品型号

                        Entity entity = null;
                        bool isCreate = true;
                        QueryExpression query = new QueryExpression();
                        query.EntityName = "product";
                        query.Criteria.AddCondition("productnumber", ConditionOperator.Equal, MATNR);
                        query.ColumnSet = new ColumnSet("productid");
                        EntityCollection entitylist = OrganizationService.RetrieveMultiple(query);
                        if (entitylist != null && entitylist.Entities.Count > 0)
                        {
                            //修改
                            isCreate = false;
                            entity = entitylist.Entities[0];
                        }
                        else
                        {
                            //新增
                            entity = new Entity("product");
                            //entity.Id = Guid.NewGuid();
                        }
                        entity["name"] = MAKTX;//名称
                        entity["productnumber"] = MATNR;//物料编码
                        entity["productstructure"] = new OptionSetValue(1);//产品
                        entity["statecode"] = new OptionSetValue(0);
                        entity["statuscode"] = new OptionSetValue(1);

                        entity["parentproductid"] = new EntityReference("product", xlGuid);//系列层次结构
                                                                                           // entity["new_productmodule_id"] = new EntityReference("new_materialgroup", wlGuid);//物料组. 关联有问题
                        if (cpzGuid != Guid.Empty)
                        {
                            entity["new_productgroupid"] = new EntityReference("new_productgroup", cpzGuid);//产品组
                        }
                        //if (xhGuid != Guid.Empty)
                        //{
                        //    entity["new_productmodel_id"] = new EntityReference("new_productmodel", xhGuid);//产品型号，对应sap规格
                        //}

                        int? new_itemtype = Getnew_itemtype(MTART);
                        if (new_itemtype != null)
                        {
                            entity["new_itemtype"] = new OptionSetValue(new_itemtype.Value);//物料类型
                        }
                        Guid defaultuomscheduleid = GetLookUpGuidByStringCode(OrganizationService, "uomschedule", "name", MEINS);
                        if (defaultuomscheduleid != Guid.Empty)
                        {
                            entity["defaultuomscheduleid"] = new EntityReference("uomschedule", defaultuomscheduleid);//计价单位组

                            Guid defaultuomid = GetLookUpGuidByStringCode(OrganizationService, "uom", "name", MEINS);
                            if (defaultuomid != Guid.Empty)
                            {
                                entity["defaultuomid"] = new EntityReference("uom", defaultuomid);//计价单位
                            }
                            else
                            {
                                Entity jj = new Entity("uom");
                                //Guid jjid = Guid.NewGuid();
                                //jj.Id = jjid;
                                jj["name"] = MEINS;
                                jj["uomscheduleid"] = new EntityReference("uomschedule", defaultuomscheduleid);
                                Guid jjid = OrganizationService.Create(jj);
                                entity["defaultuomid"] = new EntityReference("uom", jjid);
                            }
                        }
                        else
                        {
                            //创建计价单位组及计价单位
                            Entity jjz = new Entity("uomschedule");
                            //Guid jjzid = Guid.NewGuid();
                            //jjz.Id = jjzid;
                            jjz["name"] = MEINS;
                            jjz["baseuomname"] = MEINS;
                            Guid jjzid = OrganizationService.Create(jjz);

                            Entity jj = new Entity("uom");
                            //Guid jjid = Guid.NewGuid();
                            //jj.Id = jjid;
                            jj["name"] = MEINS;
                            jj["uomscheduleid"] = new EntityReference("uomschedule", jjzid);
                            Guid jjid = OrganizationService.Create(jj);

                            entity["defaultuomscheduleid"] = new EntityReference("uomschedule", jjzid);
                            entity["defaultuomid"] = new EntityReference("uom", jjid);
                        }



                        entity["new_drawingnumber"] = ZTUHAO;//配件图号
                        entity["new_brand"] = ZPINPAI;//品牌
                        entity["description"] = LONGTEXT;//对接说明
                        entity["new_oldcode"] = BISMT;//旧料号

                        //int? new_selfmadeorpurchased = GetPicklistIntValue(OrganizationService, "product", "new_selfmadeorpurchased", ZTYPE);
                        //if (new_selfmadeorpurchased != null)
                        //{
                        //    entity["new_selfmadeorpurchased"] = new OptionSetValue(new_selfmadeorpurchased.Value);//自制 外购
                        //}
                        if (ZTYPE == "E")
                        {
                            entity["new_selfmadeorpurchased"] = new OptionSetValue(100000000);
                        }
                        else if (ZTYPE == "F")
                        {
                            entity["new_selfmadeorpurchased"] = new OptionSetValue(100000001);
                        }

                        if (ZSFJM == "X")
                        {
                            entity["new_addtrade"] = true;
                        }
                        if (isCreate)
                        {
                            entity.Id = OrganizationService.Create(entity);
                            Guid newGuid = entity.Id;
                            Task.Factory.StartNew(() =>
                            {
                                Entity updateState = new Entity("product");
                                updateState.Id = newGuid;
                                updateState["statecode"] = new OptionSetValue(0);
                                updateState["statuscode"] = new OptionSetValue(-1);
                                OrganizationService.Update(entity);
                            });
                            Console.WriteLine($"物料：{MATNR}保存成功");
                            //requestCollection.Add(new CreateRequest() { Target = entity });
                        }
                        else
                        {
                            entity["statecode"] = new OptionSetValue(0);
                            entity["statuscode"] = new OptionSetValue(-1);
                            OrganizationService.Update(entity);
                            Console.WriteLine($"物料：{MATNR}更新成功");
                            //requestCollection.Add(new UpdateRequest() { Target = entity });
                        }
                        //}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }

                //oneSaveReq.Requests = requestCollection;
                //var resp = OrganizationService.Execute(oneSaveReq);
                //Log.InfoMsg(Newtonsoft.Json.JsonConvert.SerializeObject(resp.Results));
            }
            catch (Exception ex)
            {
                //Log.LogException(ex);
                throw ex;
            }
        }

        private  string GetStr(JObject jo, string key)
        {
            if (jo.Property(key) != null) return jo[key].ToString();
            else return "";
        }


        /// <summary>
        /// 根据产品组编码获得产品组guid  new_productgroup
        /// </summary>
        /// <param name="SPART"></param>
        /// <returns></returns>
        public  Guid GetProductGroupGuid(string SPART)
        {
            if (string.IsNullOrEmpty(SPART))
            {
                return Guid.Empty;
            }
            else
            {
                QueryExpression qe1 = new QueryExpression("new_productgroup");
                qe1.Criteria.AddCondition("new_code", ConditionOperator.Equal, SPART);
                qe1.ColumnSet = new ColumnSet("new_productgroupid");
                var ec1 = OrganizationService.RetrieveMultiple(qe1);
                if (ec1 != null && ec1.Entities.Count > 0)
                {
                    return ec1.Entities[0].Id;
                }
                else
                {
                    Entity e1 = new Entity("new_productgroup");
                    //e1.Id = Guid.NewGuid();
                    e1["new_code"] = SPART;
                    e1.Id = OrganizationService.Create(e1);
                    return e1.Id;
                }
            }
        }

        public Guid CreateOrGetProductSeries(string PRODH1, string PRODH1_TEXT, string PRODH2, string PRODH2_TEXT, string PRODH3, string PRODH3_TEXT)
        {
            if (string.IsNullOrEmpty(PRODH1) || string.IsNullOrEmpty(PRODH1_TEXT) || string.IsNullOrEmpty(PRODH2) ||
                string.IsNullOrEmpty(PRODH2_TEXT) || string.IsNullOrEmpty(PRODH3) || string.IsNullOrEmpty(PRODH3_TEXT))
            {
                throw new Exception("大中小类不能为空");
            }
            //大类
            QueryExpression qe1 = new QueryExpression("product");
            Entity e1 = null;
            qe1.Criteria.AddCondition("productnumber", ConditionOperator.Equal, PRODH1);
            qe1.ColumnSet = new ColumnSet("productid");
            var ec1 = OrganizationService.RetrieveMultiple(qe1);
            if (ec1 != null && ec1.Entities.Count > 0)
            {
                e1 = ec1.Entities[0];
                //e1["statecode"] = new OptionSetValue(0);
                //e1["statuscode"] = new OptionSetValue(-1);
                //OrganizationService.Update(e1);
            }
            else
            {
                e1 = new Entity("product");
                //e1.Id = Guid.NewGuid();
                e1["productnumber"] = PRODH1;
                e1["name"] = PRODH1_TEXT;
                e1["productstructure"] = new OptionSetValue(2);//系列
                e1["statecode"] = new OptionSetValue(0);
                e1["statuscode"] = new OptionSetValue(1);
                e1.Id = OrganizationService.Create(e1);

                e1["statecode"] = new OptionSetValue(0);
                e1["statuscode"] = new OptionSetValue(-1);
                OrganizationService.Update(e1);
            }
            //中类
            QueryExpression qe2 = new QueryExpression("product");
            Entity e2 = null;
            qe2.Criteria.AddCondition("productnumber", ConditionOperator.Equal, PRODH2);
            qe2.ColumnSet = new ColumnSet("productid");
            var ec2 = OrganizationService.RetrieveMultiple(qe2);
            if (ec2 != null && ec2.Entities.Count > 0)
            {
                e2 = ec2.Entities[0];
                //e2["statecode"] = new OptionSetValue(0);
                //e2["statuscode"] = new OptionSetValue(-1);
                //OrganizationService.Update(e2);
            }
            else
            {
                e2 = new Entity("product");
                //e2.Id = Guid.NewGuid();
                e2["productnumber"] = PRODH2;
                e2["name"] = PRODH2_TEXT;
                e2["productstructure"] = new OptionSetValue(2);//系列
                e2["statecode"] = new OptionSetValue(0);
                e2["statuscode"] = new OptionSetValue(1);
                e2["parentproductid"] = new EntityReference("product", e1.Id);
                e2.Id = OrganizationService.Create(e2);

                e2["statecode"] = new OptionSetValue(0);
                e2["statuscode"] = new OptionSetValue(-1);
                OrganizationService.Update(e2);
            }
            //小类
            QueryExpression qe3 = new QueryExpression("product");
            Entity e3 = null;
            qe3.Criteria.AddCondition("productnumber", ConditionOperator.Equal, PRODH3);
            qe3.ColumnSet = new ColumnSet("productid");
            var ec3 = OrganizationService.RetrieveMultiple(qe3);
            if (ec3 != null && ec3.Entities.Count > 0)
            {
                e3 = ec3.Entities[0];
                //e3["statecode"] = new OptionSetValue(0);
                //e3["statuscode"] = new OptionSetValue(-1);
                //OrganizationService.Update(e3);
            }
            else
            {
                e3 = new Entity("product");
                //e3.Id = Guid.NewGuid();
                e3["productnumber"] = PRODH3;
                e3["name"] = PRODH3_TEXT;
                e3["productstructure"] = new OptionSetValue(2);//系列
                e3["statecode"] = new OptionSetValue(0);
                e3["statuscode"] = new OptionSetValue(1);
                e3["parentproductid"] = new EntityReference("product", e2.Id);
                e3.Id = OrganizationService.Create(e3);

                e3["statecode"] = new OptionSetValue(0);
                e3["statuscode"] = new OptionSetValue(-1);
                OrganizationService.Update(e3);
            }
            return e3.Id;
        }



        /// <summary>
        /// 创建产品型号，通过sap图号匹配crm型号名称   new_productmodel
        /// </summary>
        /// <param name="cpzGuid"></param>
        /// <param name="zTUHAO"></param>
        /// <returns></returns>
        private  Guid CreateOrGetProductModel(Guid cpzGuid, string zTUHAO)
        {
            if (string.IsNullOrEmpty(zTUHAO))
            {
                return Guid.Empty;
            }
            else
            {
                QueryExpression qe1 = new QueryExpression("new_productmodel");
                qe1.Criteria.AddCondition("new_name", ConditionOperator.Equal, zTUHAO);
                qe1.ColumnSet = new ColumnSet("new_productmodelid");
                var ec1 = OrganizationService.RetrieveMultiple(qe1);
                if (ec1 != null && ec1.Entities.Count > 0)
                {
                    return ec1.Entities[0].Id;
                }
                else
                {
                    Entity e1 = new Entity("new_productmodel");
                    //e1.Id = Guid.NewGuid();
                    e1["new_name"] = zTUHAO;
                    if (cpzGuid != Guid.Empty)
                    {
                        e1["new_productgroup_id"] = new EntityReference("new_productgroup", cpzGuid);
                    }
                    e1.Id = OrganizationService.Create(e1);
                    return e1.Id;
                }
            }
        }


        /// <summary>
        /// 查询实体的某一个字段，获得该实体的guid
        /// </summary>
        /// <param name="organizationService"></param>
        /// <param name="entityName"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        public  Guid GetLookUpGuidByStringCode(IOrganizationService organizationService, string entityName, string attributeName, string attributeValue)
        {
            QueryExpression isExistMst = new QueryExpression();
            isExistMst.ColumnSet = new ColumnSet(entityName + "id");
            isExistMst.EntityName = entityName;
            isExistMst.Criteria.AddCondition(attributeName, ConditionOperator.Equal, attributeValue);
            EntityCollection isExistList = organizationService.RetrieveMultiple(isExistMst);
            if (isExistList != null && isExistList.Entities.Count == 1)
            {
                return isExistList.Entities[0].Id;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public  int? Getnew_itemtype(string val)
        {
            if (val == "TK01")
            {
                return 1;
            }
            else if (val == "TK02")
            {
                return 2;
            }
            else if (val == "TK03")
            {
                return 3;
            }
            else if (val == "TK06")
            {
                return 6;
            }
            else if (val == "TK08")
            {
                return 8;
            }
            else if (val == "TK09")
            {
                return 9;
            }
            else
            {
                return null;
            }
        }
    }
}
