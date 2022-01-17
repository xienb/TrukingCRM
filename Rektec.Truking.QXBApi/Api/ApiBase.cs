using Newtonsoft.Json.Linq;
using Rektec.Truking.Http;
using Rektec.Truking.Http.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rektec.Truking.QXBApi.Api
{
    /// <summary>
    /// api基类
    /// </summary>
    public class ApiBase
    {
        private string _appkey;
        private string _secretkey;

        public string Appkey
        {
            get
            {
                return _appkey;
            }
        }
        public string Secretkey
        {
            get
            {
                return _secretkey;
            }
        }
        public ApiBase(string appkey, string secretkey)
        {
            _appkey = appkey;
            _secretkey = secretkey;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (Convert.ToInt64(ts.TotalMilliseconds)).ToString();
        }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public string GetSign(string timestamp)
        {
            string myString = _appkey + timestamp + _secretkey;
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(myString);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;
            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");
            }
            return byte2String;
        }

        /// <summary>
        /// URL编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }
            return (sb.ToString());
        }

        /// <summary>
        /// 设置公共请求头
        /// </summary>
        /// <param name="rb"></param>
        /// <returns></returns>
        public RequestBuilder SetHeader(RequestBuilder rb)
        {
            IDictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Auth-Version", "2.0");
            header.Add("appkey", _appkey);
            var timestamp = GetTimeStamp();
            header.Add("timestamp", timestamp);
            var sign = GetSign(timestamp);
            header.Add("sign", sign);
            return rb.Headers(header);
        }
    }
}
