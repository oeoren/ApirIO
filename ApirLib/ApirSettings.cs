using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApirLib
{
    public class ApirSettings
    {
        public string BaseAddress;
        public string OutPath;
        public bool Swagger;
        public bool SwaggerUI;
        public string UserValidator;
        public string MachineValidate;
        public string DomainValidate;
        public bool RelayHost;
        public string ApirKey;
        public string ServiceKey;
        public string FreeResources;
        public string apirApi;
        public string apirTenant;
        public string apirUser;
        public string apirPassword;
        public string ClientSettingsProviderServiceUri;
        public string ConnectionString;
        public string SettingsReadFrom { get; set; } = null;
        public bool? AutoRestart; 

        private static bool hasLocalSettings()
        {
            string path = Directory.GetCurrentDirectory();
        bool hasLocalFile = File.Exists(path + "\\apir.json");
            return hasLocalFile;

        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        public static ApirSettings Read()
        {
            ApirSettings apirSettings;
            var settingsFileName = "apir.json";
            if (!File.Exists(@".\" + settingsFileName))
            {
                settingsFileName = AssemblyDirectory + "\\" + settingsFileName;
            }
            using (StreamReader r = new StreamReader(settingsFileName))
            {
                string json = r.ReadToEnd();
                apirSettings = JsonConvert.DeserializeObject<ApirSettings>(json);
            }
            apirSettings.SettingsReadFrom = settingsFileName;
            return apirSettings;
        }

        public static bool Write(ApirSettings apirSettings)
        {
            using (StreamWriter file = File.CreateText("apir.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, apirSettings);
            }
            return true;
        }

        static ApirSettings settings = null;
        public static ApirSettings Get()
        {
            if (settings == null)
                settings = Read();
            return settings;
        }

        public static string xGet(string key)
        {
            Console.WriteLine("local settings = {0}", hasLocalSettings() ? "true" : "false");
            var val = ConfigurationManager.AppSettings[key];
            return val;
        }

        public static void Put(string key, string val)
        {

        }
    }
}
