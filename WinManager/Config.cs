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

        public List<TriggerShortcut> TriggerShortcuts = new List<TriggerShortcut>
        {
            // Show apps
            new TriggerShortcut("Win_F12", ModifierKeyCodes.Windows, 0x77, TriggerShortcut.TriggerAction.ShowApps),
            new TriggerShortcut("Win_Shift_A", ModifierKeyCodes.Windows | ModifierKeyCodes.Shift, 0x41, TriggerShortcut.TriggerAction.ShowApps),

            // Show windows
            new TriggerShortcut("Win_F11", ModifierKeyCodes.Windows, 0x76, TriggerShortcut.TriggerAction.ShowWindows),
            new TriggerShortcut("Win_Shift_Q", ModifierKeyCodes.Windows | ModifierKeyCodes.Shift, 0x51, TriggerShortcut.TriggerAction.ShowWindows),
        };
        public Settings AppSettings
        {
            get { return _config.settings; }
        }

        private const string TrueString = "yes";
        private const string FalseString = "no";

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
            Utils.SetYesOrNo(settings.enabledShortcuts.showWindows, defaultSettings.enabledShortcuts.showWindows, ["Win_F11", "Win_Shift_Q"]);
            Save();
        }

        public void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var configString = JsonSerializer.Serialize(_config, options);
            File.WriteAllText(_path, configString, Encoding.UTF8);
        }

        public static bool StringToBool(string value)
        {
            return value == TrueString;
        }

        public static string BoolToString(bool value)
        {
            return value ? TrueString : FalseString;
        }

        public object GetActionSetting(TriggerShortcut.TriggerAction action)
        {
            var actionMapping = new List<Object> {
AppSettings.enabledShortcuts.showApps,
AppSettings.enabledShortcuts.showWindows,
            };
            return actionMapping[(int) action];
        }
    }
}

