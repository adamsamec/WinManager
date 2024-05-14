using System.IO;

namespace WinManager
{
    /// <summary>
    /// Constants class
    /// </summary>
    public static class Consts
    {
        public const string AppVersion = "1.0.6";
        public static readonly bool ForceCzechLanguage = false;

        // URLs
        public const string ApiUrl = "http://api.adamsamec.cz/WinManager/Update.json";
        public const string ChangeLogUrl = "https://raw.githubusercontent.com/adamsamec/WinManager/main/ChangeLog/ChangeLog.{0}.md";
        public const string translatorUrl = "https://slovnik.seznam.cz/preklad/anglicky_cesky/";

        // Paths and filenames
        public const string PagesFolder = "Pages";
        public static string HelpFileRelativePath = Path.Combine(PagesFolder, "Help.{0}.md");
        public static string localUserFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinManager");
        public static string InstallerDownloadFolder = Path.Combine(localUserFolder, "installer");
        public const string ExecutableFilename = "WinManager.exe";
        public const string ConfigDefaultFilename = "App.config.default.json";
        public const string ConfigFilename = "App.config.json";
        public static string InstallFolder
        {
            get
            {
                if (_installFolder == null)
                {
                    var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    _installFolder = System.IO.Path.GetDirectoryName(assemblyPath);
                }
                if (_installFolder == null)
                {
                    throw new InstallPathUnknownException("Unable to determine WinManager install path");
                }
                return _installFolder;
            }
        }

        // Running apps filtering and names overrides
        public static string[] IgnoredProcesses = {
      "SystemSettings",
      "CalculatorApp"
        };
        public static Dictionary<string, string> AppNamesOverrides = new Dictionary<string, string>
        {
            //{"ApplicationFrameHost", WinManager.Resources.modernAppsOverride},
            {"Notepad", WinManager.Resources.notepadOverride},
            {"WindowsTerminal", "Windows Terminal"},
        };

        // Other
        public const string StartupRegistryKeyName = "WinManager";

        // Private fields
        private static string? _installFolder;
    }
}
