using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.Plugin
{
    public class Gzap
    {
        public int new_name { get; set; }
        public string new_content { get; set; }
        public string new_ownerid_id { get; set; }
        public string new_place { get; set; }
        public string new_preparatorywork { get; set; }
    }

    public class AccountinspectCreatedPlugin : IPlugin
    {
        /// <summary>
        /// 获取指定表的所有字段说明信息
        /// </summary>
        /// <param name="organizationService"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetEntities(IOrganizationService organizationService, string entityName)
        {
            Dictionary<string, string> attributesData = new Dictionary<string, string>();

            //RetrieveAllEntitiesRequest metaDataRequest = new RetrieveAllEntitiesRequest();
            RetrieveEntityRequest metaDataRequest1 = new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.Attributes,//控制查询元数据的过滤条件
                LogicalName = entityName
            };
            RetrieveEntityResponse metaDataResponse = new RetrieveEntityResponse();

            // Execute the request.
            metaDataResponse = (RetrieveEntityResponse)organizationService.Execute(metaDataRequest1);
            var entities = metaDataResponse.EntityMetadata;

            foreach (var attr in entities.Attributes)
            {
                if (attr.Description.LocalizedLabels.Count > 0)
                {
                    attributesData.Add(attr.LogicalName, attr.Description.LocalizedLabels[0].Label);
                }
            }
            return attributesData;
        }


        public void Execute(IServiceProvider serviceProvider)
        {
            //获取执行上下文
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            //获取服务工厂
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            //获取组织服务
            IOrganizationService service = serviceFactory.CreateOrganizationService(null); // 创建组织服务，null表示System这个用户
            //获取跟踪服务                                                                          // 获取跟踪服务
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));


            try
            {
                //获取创建消息
                //if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity && (context.MessageName.ToLower() == "create"))//创建时触发，create、update、delete三种状态
                //{
                //Update Create插件 获取上下文实体
                Entity entity1 = (Entity)context.InputParameters["Target"];
                //创建客户考察接待安排明细，客户考察的id 设置成entity1的id
                //当客户考察类型为商务考察时，客户考察接待明细安排的关联工作1-12 设置成说明字段

                //获取entity1的考察类型， 通过考察类型查询工作模板中的数据
                //查询到的数据创建  客户考察接待安排明细  对应的子表（工作安排），注意要按序号

                //更新客户考察接待明细  对应的参观路线字段=  子表的地点相加
                //entity1["new_inspecttype"]
                var new_inspecttype = entity1.GetAttributeValue<OptionSetValue>("new_inspecttype").Value;//0 商务考察  1 样板点
                var kykcjdap_entity = new Entity("new_sales_accountreceptionarrangementline");
                kykcjdap_entity["new_sales_accountinspect_id"] = new EntityReference("new_sales_accountinspect", entity1.Id);
                kykcjdap_entity["new_name"] = "路线";
                if (new_inspecttype == 0)//商务考察
                {
                    SetGlgz(service, kykcjdap_entity);//设置关联工作
                }
                var pk_kykcjdap_entity = service.Create(kykcjdap_entity);

                //查询模板
                var fetcXml = "<fetch mapping=\"logical\" version=\"1.0\"><entity name=\"new_accountreceptionarrangement\"><attribute name=\"new_name\" /><attribute name=\"new_templatetype\" /><attribute name=\"new_place\" /><attribute name=\"new_content\" /><attribute name=\"new_ownerid_id\" /><attribute name=\"new_preparatorywork\" /><filter><condition attribute=\"new_templatetype\" operator=\"eq\" value=\"" + new_inspecttype + "\" /></filter></entity></fetch>";
                FetchExpression fetch1 = new FetchExpression(fetcXml);
                EntityCollection ec1 = service.RetrieveMultiple(fetch1);
                if (ec1?.Entities?.Count > 0)
                {
                    //var requestWithResults = new ExecuteMultipleRequest()
                    //{
                    //    Settings = new ExecuteMultipleSettings()
                    //    {
                    //        ContinueOnError = false,
                    //        ReturnResponses = true
                    //    },
                    //    Requests = new OrganizationRequestCollection()
                    //};

                    List<Gzap> gzapList = new List<Gzap>();
                    SortGzap(ec1, gzapList);

                    var luxian = "";
                    foreach (Gzap gzap in gzapList)
                    {
                        Entity oneGzap = new Entity("new_salesaccountreceptionarrangementline");
                        oneGzap["new_master_id"] = new EntityReference("new_sales_accountreceptionarrangementline", pk_kykcjdap_entity);
                        oneGzap["new_name"] = gzap.new_name.ToString();//序号  new_name 100
                        oneGzap["new_content"] = gzap.new_content;//内容  new_content 111
                        oneGzap["new_ownerid_id"] = gzap.new_ownerid_id;//责任人  new_ownerid_id  100
                        oneGzap["new_place"] = gzap.new_place;//地点   new_place  100
                        oneGzap["new_preparatorywork"] = gzap.new_preparatorywork;//准备工作   new_preparatorywork  2000
                        service.Create(oneGzap);
                        //CreateRequest createRequest = new CreateRequest { Target = oneGzap };
                        //requestWithResults.Requests.Add(createRequest);
                        if (String.IsNullOrEmpty(luxian))
                        {
                            luxian += gzap.new_place;
                        }
                        else
                        {
                            luxian += ("-->" + gzap.new_place);
                        }
                    }
                    //service.Execute(requestWithResults);//保存所有工作安排
                    //更新路线
                    kykcjdap_entity.Id = pk_kykcjdap_entity;
                    kykcjdap_entity["new_name"] = luxian;
                    service.Update(kykcjdap_entity);
                }
                //}
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                // 异常处理代码
                tracingService.Trace("Message:{0},StackTrace:{1}", ex.Message, ex.StackTrace);
                throw ex;
            }

        }

        /// <summary>
        /// 设置关联工作
        /// </summary>
        /// <param name="service"></param>
        /// <param name="kykcjdap_entity"></param>
        private void SetGlgz(IOrganizationService service, Entity kykcjdap_entity)
        {
            var allmeta = GetEntities(service, "new_sales_accountreceptionarrangementline");//所有说明
            kykcjdap_entity["new_elsework1"] = allmeta["new_elsework1"];
            kykcjdap_entity["new_elsework2"] = allmeta["new_elsework2"];
            kykcjdap_entity["new_elsework3"] = allmeta["new_elsework3"];
            kykcjdap_entity["new_elsework4"] = allmeta["new_elsework4"];
            kykcjdap_entity["new_elsework5"] = allmeta["new_elsework5"];
            kykcjdap_entity["new_elsework6"] = allmeta["new_elsework6"];
            kykcjdap_entity["new_elsework7"] = allmeta["new_elsework7"];
            kykcjdap_entity["new_elsework8"] = allmeta["new_elsework8"];
            kykcjdap_entity["new_elsework9"] = allmeta["new_elsework9"];
            kykcjdap_entity["new_elsework10"] = allmeta["new_elsework10"];
            kykcjdap_entity["new_elsework11"] = allmeta["new_elsework11"];
            kykcjdap_entity["new_elsework12"] = allmeta["new_elsework12"];
        }

        /// <summary>
        /// 工作安排按序号排序
        /// </summary>
        /// <param name="ec1"></param>
        /// <param name="gzapList"></param>
        private void SortGzap(EntityCollection ec1, List<Gzap> gzapList)
        {
            foreach (Entity oneGzapmb in ec1.Entities)
            {
                Gzap oneGzap = new Gzap();
                oneGzap.new_name = Convert.ToInt32(oneGzapmb["new_name"]);
                oneGzap.new_content = (oneGzapmb.Contains("new_content")? oneGzapmb["new_content"].ToString():"");
                oneGzap.new_ownerid_id = (oneGzapmb.Contains("new_ownerid_id") ?  oneGzapmb["new_ownerid_id"].ToString() : "");
                oneGzap.new_place = (oneGzapmb.Contains("new_place") ?  oneGzapmb["new_place"].ToString():"");
                oneGzap.new_preparatorywork = (oneGzapmb.Contains("new_preparatorywork") ? oneGzapmb["new_preparatorywork"].ToString() : "");
                gzapList.Add(oneGzap);
            }
            gzapList.Sort((x, y) => x.new_name.CompareTo(y.new_name));//排序
        }
    }
}
