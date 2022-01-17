using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;

namespace Truking.CRM.Helper
{
    public class AppConfig
    {
        const string CONFIGNAME = "App.config";
        public static void InitialConfig(Dictionary<string, string> dic = null)
        {
            var filePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\{CONFIGNAME}";
            if (!File.Exists(filePath))
            {
                XmlDocument doc = new XmlDocument();
                XmlElement conf = doc.CreateElement("configuration");
                XmlElement app = doc.CreateElement("appSettings");
                conf.AppendChild(app);
                doc.AppendChild(conf);
                doc.Save(filePath);
                foreach (var kv in dic)
                {
                    UpdateAppConfig(kv.Key, kv.Value);
                }
            }
        }

        public static string Get(string strKey)
        {
            var file = $"{AppDomain.CurrentDomain.BaseDirectory}\\{CONFIGNAME}";
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = file }, ConfigurationUserLevel.None);
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (key == strKey)
                {
                    return config.AppSettings.Settings[strKey].Value.ToString();
                }
            }
            return null;
        }

        public static void UpdateAppConfig(string newKey, string newValue)
        {
            var file = $"{AppDomain.CurrentDomain.BaseDirectory}\\{CONFIGNAME}";
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = file }, ConfigurationUserLevel.None);
            bool exist = false;
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (key == newKey)
                {
                    exist = true;
                }
            }
            if (exist)
            {
                config.AppSettings.Settings.Remove(newKey);
            }
            config.AppSettings.Settings.Add(newKey, newValue);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
