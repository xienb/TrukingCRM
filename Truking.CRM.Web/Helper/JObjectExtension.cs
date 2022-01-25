using Newtonsoft.Json.Linq;

namespace Truking.CRM.Web.Helper
{
    public static class JObjectExtension
    {
        public static string GetStr(this JObject jo, string key)
        {
            if (jo.Property(key) != null) return jo[key].ToString();
            else return "";
        }
    }
}