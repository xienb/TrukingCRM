using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.WinSrv.Models
{
    [Serializable]
    [Table("bizlog")]
    public class bizlog
    {
        [Key]
        public int phid { get; set; }

        public string biz { get; set; }

        public string lev { get; set; }
        public string msg { get; set; }
        public string trace { get; set; }

        public DateTime ct { get; set; }
    }
}
