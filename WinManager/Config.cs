using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace WinManager
{
    /// <summary>
    /// Application configuration
    /// </summary>
    public class Config
    {
        private ConfigJson _config;

        public List<TriggerShortcut> TriggerShortcuts = new List<TriggerShortcut>
        {
            // Show apps
            new TriggerShortcut("Win_F12", "Windows + F12", ModifierKeyCodes.Windows, 0x7B, TriggerShortcut.TriggerAction.ShowApps),
            new TriggerShortcut("Win_Shift_E", "Windows + Shift + E", ModifierKeyCodes.Windows | ModifierKeyCodes.Shift, 0x45, TriggerShortcut.TriggerAction.ShowApps),

            // Show windows
            new TriggerShortcut("Win_Shift_F12", "Windows + Shift + F12", ModifierKeyCodes.Windows | ModifierKeyCodes.Shift, 0x7B, TriggerShortcut.TriggerAction.ShowWindows),
            new TriggerShortcut("Win_Shift_Q", "Windows + Shift + Q", ModifierKeyCodes.Windows | ModifierKeyCodes.Shift, 0x51, TriggerShortcut.TriggerAction.ShowWindows),

            // Show translator
            new TriggerShortcut("Win_F10", "Windows + F10", ModifierKeyCodes.Windows, 0x79, TriggerShortcut.TriggerAction.ShowTranslator),
            new TriggerShortcut("Win_Shift_X", "Windows + Shift + X", ModifierKeyCodes.Windows | ModifierKeyCodes.Shift, 0x58, TriggerShortcut.TriggerAction.ShowTranslator),
        };
        public Settings AppSettings
        {
            get { return _config.settings; }
        }

        private const string TrueString = "yes";
        private const string FalseString = "no";

        public Config()
        {
            Directory.CreateDirectory(Consts.LocalUserFolder);

            // Create the config if it not yet exists
            if (!File.Exists(Consts.ConfigFilePath))
            {
                File.Copy(Consts.DefaultConfigFilePath, Consts.ConfigFilePath);
            }

            // Load the config
            var configString = File.ReadAllText(Consts.ConfigFilePath, Encoding.UTF8);
            var config = JsonSerializer.Deserialize<ConfigJson>(configString);
            if (config == null)
            {
                throw new SerializationException("Unable to deserialize config file");
            }
            _config = config as ConfigJson;
            var settings = _config.settings;

            var defaultConfigString = File.ReadAllText(Consts.DefaultConfigFilePath, Encoding.UTF8);
            var defaultConfig = JsonSerializer.Deserialize<ConfigJson>(defaultConfigString);
            if (defaultConfig == null)
            {
                throw new SerializationException("Unable to deserialize default config file");
            }
            var defaultSettings = defaultConfig.settings;

            // Set missing JSON properties to defaults
            Utils.SetYesOrNo(settings, defaultSettings, ["launchOnStartup", "checkUpdateOnFirstShow"]);
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
                if (settings.enabledShortcuts.showTranslator == null)
                {
                    settings.enabledShortcuts.showTranslator = defaultSettings.enabledShortcuts.showTranslator;
                }
            }
            Utils.SetYesOrNo(settings.enabledShortcuts.showTranslator, defaultSettings.enabledShortcuts.showTranslator, ["Win_F10", "Win_Shift_F"]);
            Save();
        }

        public void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var configString = JsonSerializer.Serialize(_config, options);
            File.WriteAllText(Consts.ConfigFilePath, configString, Encoding.UTF8);
        }

        public static bool StringToBool(string? value)
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
AppSettings.enabledShortcuts.showTranslator,
            };
            return actionMapping[(int)action];
        }
    }

    [Serializable]
    public class InstallPathUnknownException : Exception
    {
        public InstallPathUnknownException() { }

        public InstallPathUnknownException(string message)
            : base(message) { }

        public InstallPathUnknownException(string message, Exception inner)
            : base(message, inner) { }
    }
}

