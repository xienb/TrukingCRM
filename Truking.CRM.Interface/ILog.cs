using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.Interface
{
    public interface ILog
    {
        void InfoMsg(string msg);
        void ErrorMsg(string msg);
    }
}
