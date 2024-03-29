using System.IO;

namespace WinManager
{
    /// <summary>
    /// Constants class
    /// </summary>
    public static class Consts
    {
        public const string AppVersion = "0.1.0";
        public const bool ForceCzechLanguage = false;

        // Paths and filenames
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
                return _installFolder;
            }
        }
        public const string ApiUrl = "http://api.adamsamec.cz/WinManager/Update.json";
        public static string SetupDownloadFolder = Path.Combine(localUserFolder, "setup");

        // Other
        public const string StartupRegistryKeyName = "WinManager";

        // Private fields
        private static string _installFolder;
    }
}
