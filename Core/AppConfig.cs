using System.IO;
using System.Text.Json;

namespace PassManaAlpha.Core
{
    public class AppConfig
    {
        public string MasterKey { get; set; } = string.Empty;

        private static readonly string ConfigPath = "config.json";

        public static AppConfig Load()
        {
            if (!File.Exists(ConfigPath)) return new AppConfig();
            try
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch { return new AppConfig(); }
        }

        public void Save()
        {
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this));
        }
    }
}