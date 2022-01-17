using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.Interface
{
    public interface IWinSrvApi
    {
        bool AddNewJob(string qstr,string dll,string ass,string name);
    }
}
