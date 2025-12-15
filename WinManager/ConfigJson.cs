namespace WinManager
{
    public class ConfigJson
    {
        public Settings settings { get; set; }
    }

    public class Settings
    {
        public string launchOnStartup { get; set; }
        public string checkUpdateOnFirstShow { get; set; }
        public Enabledshortcuts enabledShortcuts { get; set; }
    }

    public class Enabledshortcuts
    {
        public Showapps showApps { get; set; }
        public Showwindows showWindows { get; set; }
        public Showtranslator showTranslator { get; set; }
    }

    public class Showapps
    {
        public string Win_F12 { get; set; }
        public string Win_Shift_E { get; set; }
    }

    public class Showwindows
    {
        public string Win_Shift_F12 { get; set; }
        public string Win_Shift_Q { get; set; }
    }

    public class Showtranslator
    {
        public string Win_F10 { get; set; }
        public string Win_Shift_X { get; set; }
    }

}