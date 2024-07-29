using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LIM.Helpers
{
    public class LimSettings
    {
        public string SharePointUrl { get;set; }

        public string ListName { get;set; }

        public string BarcodeScannerComPort { get; set; } = "COM1";

        public int BarcodeScannerBaud { get; set; } = 9600;


        private string settingsPath;

        private LimSettings(string settingsPath)
        {
            this.settingsPath = settingsPath;
        }

        [JsonConstructor]
        private LimSettings()
        {
            
        }

        public void Save()
        {
            if (settingsPath != "")
            {
                var jsonObject = JsonSerializer.Serialize(this);
                File.WriteAllText(settingsPath, jsonObject);
            }
        }

        public static LimSettings TryLoadFromFileOrNew()
        {
            var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lim-settings.json");
            if (File.Exists(settingsPath))
            {
                var jsonObject = File.ReadAllText(settingsPath);
                var settings = JsonSerializer.Deserialize<LimSettings>(jsonObject);
                settings.settingsPath = settingsPath;
                return settings;
            }
            else
            {
                return new LimSettings(settingsPath);
            }
        }
    }
}
