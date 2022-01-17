using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Truking.CRM.Interface;

namespace Truking.CRM.WinSrv
{
    public class WinSrvApi : MarshalByRefObject, IWinSrvApi
    {
        public bool AddNewJob(string qstr, string dll, string ass, string name)
        {
            throw new NotImplementedException();
        }
    }
}
