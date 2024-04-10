using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetLanguageDictionary();
            var mainWindow = new MainWindow();
        }

        private void SetLanguageDictionary()
        {
            string lang;
            if (Consts.ForceCzechLanguage)
            {
                lang = "cs-CZ";
            }
            else
            {
                var currentCulture = Thread.CurrentThread.CurrentCulture.ToString();
                switch (currentCulture)
                {
                    case "cs-CZ":
                        lang = "cs-CZ";
                        break;
                    case "sk-SK":
                        lang = "cs-CZ"; // Use Czech dictionary for Slovak environment
                        break;
                    default:
                        lang = "en-US";
                        break;
                }
            }
            WinManager.Resources.Culture = new System.Globalization.CultureInfo(lang);
        }
    }
}
