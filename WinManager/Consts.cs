using System.IO;

namespace WinManager
{
    /// <summary>
    /// Constants class
    /// </summary>
    public static class Consts
    {
        public const string AppVersion = "1.0.3";
        public static readonly bool ForceCzechLanguage = false;

        // Paths and filenames
        public const string PagesFolder = "Pages";
        public static string HelpFileRelativePath = Path.Combine(PagesFolder, "Help.{0}.md");
        public static string ChangeLogFileRelativePath = Path.Combine(PagesFolder, "ChangeLog.{0}.md");
        public static string localUserFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinManager");
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
        public const string ApiUrl = "http://api.adamsamec.cz/WinManager/Update.json";
        public static string InstallerDownloadFolder = Path.Combine(localUserFolder, "installer");

        // App names overrides
        public static Dictionary<string, string> AppNamesOverrides = new Dictionary<string, string>
        {
            {"Notepad.exe", WinManager.Resources.notepadOverride},
            {"WindowsTerminal.exe", "Windows Terminal"}
        };

        // Other
        public const string StartupRegistryKeyName = "WinManager";

        // Private fields
        private static string? _installFolder;
    }
}
