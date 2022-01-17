using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rektec.Truking.Http
{
    public class SyncRespEntity
    {
        public WebException WebEx { get; set; }
        public string Resp { get; set; }
    }
}
