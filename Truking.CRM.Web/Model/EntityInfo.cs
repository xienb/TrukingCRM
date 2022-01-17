using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Truking.CRM.Web.Model
{
    public class EntityInfo
    {
        public string entityName { get; set; }
        public string entityId { get; set; }
    }

    public class BizTmp
    {
        public string url { get; set; }
        public string fileName { get; set; }
        public int fileSize { get; set; }
    }
}