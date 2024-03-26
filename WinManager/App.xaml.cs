using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const bool _useCzechByDefault = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetLanguageDictionary();
            var mainWindow = new MainWindow();
        }

        private void SetLanguageDictionary()
        {
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "cs-CZ":
                    WinManager.Resources.Culture = new System.Globalization.CultureInfo("cs-CZ");
                    break;
                default:
                    var lang = _useCzechByDefault ? "cs-CZ" : "en-US";
                    WinManager.Resources.Culture = new System.Globalization.CultureInfo(lang);
                    break;
            }


        }

    }

}
