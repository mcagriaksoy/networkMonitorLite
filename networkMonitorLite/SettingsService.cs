// Author: mcagriaksoy - github.com/mcagriaksoy

using System;
using System.IO;
using System.Text.Json;

namespace NetworkMonitor
{
    public static class SettingsService
    {
        private static readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NetworkMonitorLite");
        private static readonly string SettingsFile = Path.Combine(AppFolder, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                        return settings;
                }
            }
            catch
            {
                // ignore and return defaults
            }
            return AppSettings.Default();
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                if (!Directory.Exists(AppFolder))
                    Directory.CreateDirectory(AppFolder);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFile, json);
            }
            catch
            {
                // swallow errors for now
            }
        }
    }
}
