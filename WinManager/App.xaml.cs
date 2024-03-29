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
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "cs-CZ":
                    WinManager.Resources.Culture = new System.Globalization.CultureInfo("cs-CZ");
                    break;
                default:
                    var lang = Consts.ForceCzechLanguage ? "cs-CZ" : "en-US";
                    WinManager.Resources.Culture = new System.Globalization.CultureInfo(lang);
                    break;
            }


        }

    }

}
