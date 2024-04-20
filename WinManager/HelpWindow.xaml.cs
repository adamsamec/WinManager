using System.Diagnostics;
using System.Windows;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        private Manager _manager;

        public HelpWindow(Manager manager)
        {
            InitializeComponent();

            _manager = manager;
        }

        private void HelpWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var html = _manager.GetHelpHTML();
            html = @"
<html>
 <body>
<div id='page' tabindex='-1'>
<h1>WinManager " + Consts.AppVersion + @"</h1>
" + html + @"
</div>
<script>
function focusBegining() {
var page = document.getElementById('page');
page.focus();
}
</script>
</body>
</html>
";
            Debug.WriteLine(html);
            webBrowser.LoadCompleted += (sendr, e) =>
            {
                webBrowser.InvokeScript("focusBegining");
            };
            webBrowser.NavigateToString(html);
            webBrowser.Focus();
        }
    }
}