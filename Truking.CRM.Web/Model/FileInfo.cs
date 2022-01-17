using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Truking.CRM.Web.Model
{
    public class MyFileInfo
    {
        public string entityName { get; set; }
        public string entityId { get; set; }

        public string fileId { get; set; }

        public string fileName { get; set; }

        public long fileSize { get; set; }
    }
}