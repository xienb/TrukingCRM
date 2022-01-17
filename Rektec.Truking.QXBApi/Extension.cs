using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rektec.Truking.QXBApi
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jo"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetStr(this JObject jo, string key)
        {
            if (jo == null)
            {
                return "";
            }
            else
            {
                return (jo[key] ?? "").ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static QXBReturn ToQxbRv(this string str)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<QXBReturn>(str);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static QXBReturnPage ToQxbRvPage(this string str)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<QXBReturnPage>(str);
        }
    }
}
