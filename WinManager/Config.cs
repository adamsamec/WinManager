using System.IO;
using System.Text;
using System.Text.Json;

namespace WinManager
{
    /// <summary>
    /// Application configuration
    /// </summary>
    public class Config
    {
        private string _path;
        private ConfigJson _config;

        public const string True = "yes";
        public const string False = "no";

        public Settings AppSettings
        {
            get { return _config.settings; }
        }

        public Config()
        {
            Directory.CreateDirectory(Consts.localUserFolder);
            var defaultPath = Path.Combine(Consts.InstallFolder, Consts.ConfigDefaultFilename);
            _path = Path.Combine(Consts.localUserFolder, Consts.ConfigFilename);

            // Create the config if it not yet exists
            if (!File.Exists(_path))
            {
                File.Copy(defaultPath, _path);
            }

            // Load the config
            var configString = File.ReadAllText(_path, Encoding.UTF8);
            _config = JsonSerializer.Deserialize<ConfigJson>(configString);
            var settings = _config.settings;

            var defaultConfigString = File.ReadAllText(defaultPath, Encoding.UTF8);
            var defaultConfig = JsonSerializer.Deserialize<ConfigJson>(defaultConfigString);
            var defaultSettings = defaultConfig.settings;

            // Set missing JSON properties to defaults
            Utils.SetYesOrNo(settings, defaultSettings, ["launchOnStartup"]);
            if (settings.enabledShortcuts == null)
            {
                settings.enabledShortcuts = defaultSettings.enabledShortcuts;
            }
            else
            {
                if (settings.enabledShortcuts.showApps == null)
                {
                    settings.enabledShortcuts.showApps = defaultSettings.enabledShortcuts.showApps;
                }
                if (settings.enabledShortcuts.showWindows == null)
                {
                    settings.enabledShortcuts.showWindows = defaultSettings.enabledShortcuts.showWindows;
                }
            }
            Utils.SetYesOrNo(settings.enabledShortcuts.showApps, defaultSettings.enabledShortcuts.showApps, ["Win_F12", "Win_Shift_A"]);
            Utils.SetYesOrNo(settings.enabledShortcuts.showWindows, defaultSettings.enabledShortcuts.showWindows, ["Win_F11", "Win_Shift_W"]);
            Save();
        }

        public void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var configString = JsonSerializer.Serialize(_config, options);
            File.WriteAllText(_path, configString, Encoding.UTF8);
        }
    }
}

