using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.Model
{
    public enum ResponseCode
    {
        success = 0,
        failure = 1,
        loginError = 2,
        unauthorized = 3,
        signError = 4,
        parsError = 5,
        error = 6
    }
}
