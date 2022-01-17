using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rektec.Truking.QXBApi.Api
{
    /// <summary>
    /// 接口类型
    /// </summary>
    public enum QXB
    {
        api_1_31,
        api_1_41,
        api_1_16,
        api_1_51,
        api_1_47,
        api_1_43,
        api_44_2,
        api_1_55,
        api_6_6,
        api_1_49,
        api_28_6,
        api_51_1,
        api_32_1,
        api_21_4,
        api_21_6,
        api_50_5,
        api_51_4,
        api_20_1,
        api_20_3,
        api_37_3,
        api_25_1,
        api_26_1,
        api_34_1
    }

   /// <summary>
   /// 接口地址
   /// </summary>
    public static class ApiUrl
    {
        public static Dictionary<QXB, string> Dic = new Dictionary<QXB, string>
        {
            { QXB.api_1_31,"https://api.qixin.com/APIService/v2/search/advSearch?keyword={0}&region={1}&skip={2}"},
            { QXB.api_1_41,"https://api.qixin.com/APIService/enterprise/getBasicInfo?keyword={0}"},
            { QXB.api_1_16,"https://api.qixin.com/APIService/enterprise/getEntBriefByName?name={0}"},
            { QXB.api_1_51,"https://api.qixin.com/APIService/enterprise/getContactInfo?keyword={0}"},
            { QXB.api_1_47,"https://api.qixin.com/APIService/enterprise/getChangeRecords?keyword={0}&skip={1}"},
            { QXB.api_1_43,"https://api.qixin.com/APIService/enterprise/getPartners?keyword={0}&skip={1}"},
            { QXB.api_44_2,"https://api.qixin.com/APIService/relation/getEquityStructureByName?name={0}"},
            { QXB.api_1_55,"https://api.qixin.com/APIService/enterprise/getAbnormals?keyword={0}&skip={1}"},
            { QXB.api_6_6,"https://api.qixin.com/APIService/sumLawsuit/sumLawsuit?name={0}"},
            { QXB.api_1_49,"https://api.qixin.com/APIService/enterprise/getBranchs?keyword={0}&skip={1}"},
            { QXB.api_28_6,"https://api.qixin.com/APIService/enterprise/getInvestmentByName?name={0}"},
            { QXB.api_51_1,"https://api.qixin.com/APIService/envp/getEPByName?name={0}&skip={1}"},
            { QXB.api_32_1,"https://api.qixin.com/APIService/v2/adminPunish/getAdminPunishByName?keyword={0}&skip={1}"},
            { QXB.api_21_4,"https://api.qixin.com/APIService/operation/getBiddingListByName?name={0}&skip={1}"},
            { QXB.api_21_6,"https://api.qixin.com/APIService/operation/getBiddingDetail?id={0}"},
            { QXB.api_50_5,"https://api.qixin.com/APIService/baseInfo/getCoreMember?name={0}&skip={1}"},
            { QXB.api_51_4,"https://api.qixin.com/APIService/bankruptcy/getBankruptcyListByName?name={0}&skip={1}"},
            { QXB.api_20_1,"http://api.qixin.com/APIService/overduetax/getOverDueTaxByName?name={0}&skip={1}"},
            { QXB.api_20_3,"https://api.qixin.com/APIService/overduetax/getTaxCaseByName?name={0}&skip={1}"},
            { QXB.api_37_3,"https://api.qixin.com/APIService/v2/financing/getFinancingByName?name={0}"},
            { QXB.api_25_1,"https://api.qixin.com/APIService/v2/mortgage/getMortgagesByName?name={0}&skip={1}"},
            { QXB.api_26_1,"https://api.qixin.com/APIService/v2/equityPledge/getEquityQualitiesByName?name={0}&skip={1}"},
            { QXB.api_34_1,"https://api.qixin.com/APIService/v2/judicialFreeze/getJudicialFreezeByName?name={0}&skip={1}"}
        };
    }
}
