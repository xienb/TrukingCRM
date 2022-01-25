using System;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Xrm.Sdk;

namespace Truking.CRM.Web.Helper
{
    public static class CommonHelper
    {
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (Convert.ToInt64(ts.TotalMilliseconds)).ToString();
        }

        /// <summary>
        /// 判断时间戳与服务器时间是否大于5分钟
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static bool IsTimeStampRight(string timestamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(timestamp));
            TimeSpan ts = dt - DateTime.UtcNow;
            var abs = Math.Abs(ts.TotalSeconds);
            return !(abs > 300);
        }

        /// <summary>
        /// 获取选项值的中文名
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string GetPicklistName(this Entity entity, string attribute)
        {
            if (entity.Contains(attribute))
            {
                return entity.FormattedValues[attribute];
            }
            return "";
        }

        //加密
        public static string AesEncrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        //解密
        public static string AesDecrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(str);

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
        
    }
}