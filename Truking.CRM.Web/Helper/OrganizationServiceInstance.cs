using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Truking.CRM.Web.Helper
{
    public class OrganizationServiceInstance
    {
        //private CrmServiceClient _crmServiceClient;
        public Guid UserId { get; set; }
        internal IOrganizationService OrgService 
        {
            get 
            {
                //if (_crmServiceClient==null || !_crmServiceClient.IsReady)
                //{
                //    Init();
                //}
                //return (IOrganizationService)_crmServiceClient.OrganizationServiceProxy;
                using (CrmServiceClient conn = new CrmServiceClient(AppConfig.Get("CRMConnect")))
                {
                    UserId = conn.GetMyCrmUserId();
                    // Cast the proxy client to the IOrganizationService interface.
                    IOrganizationService orgService = (IOrganizationService)conn.OrganizationWebProxyClient ??
                                                      conn.OrganizationServiceProxy;
                    return orgService;
                }
            }
        }
        private OrganizationServiceInstance()
        {
            Init();
        }

        private void Init()
        {
            //var crmServiceClient = new CrmServiceClient(AppConfig.Get("CRMConnect"));
            //if (!crmServiceClient.IsReady)
            //{
            //    throw crmServiceClient.LastCrmException;
            //}
            //_crmServiceClient = crmServiceClient;
        }

        public static OrganizationServiceInstance Instance { get { return Nested._instance; } }

        private class Nested
        {
            static Nested()
            {
            }
            internal static readonly OrganizationServiceInstance _instance = new OrganizationServiceInstance();
        }
    }
}