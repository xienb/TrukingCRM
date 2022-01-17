using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.Model
{
    public class WebRv
    {
        public ResponseCode code { get; set; }
        public string msg { get; set; }
        public object data { get; set; }
    }
}
