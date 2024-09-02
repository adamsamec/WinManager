using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for TranslatorWindow.xaml
    /// </summary>
    public partial class TranslatorWindow : Window
    {
        private Manager _manager;
        private string? _keyword;

        public TranslatorWindow(Manager manager, string? keyword)
        {
            InitializeComponent();

            _manager = manager;
            _keyword = keyword;

            KeyDown += TranslatorWindow_KeyDown;
        }

        private void TranslatorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Title = WinManager.Resources.translatorWindowTitle + Consts.WindowTitleSeparator + Consts.AppName;

            // Hide window from task switching
            Utils.SetWindowToolStyle(new WindowInteropHelper(this).Handle);

            if (_keyword != null)
            {
                keywordTextBox.Text = _keyword;
                keywordTextBox.SelectAll();
            }
            keywordTextBox.KeyDown += new KeyEventHandler((sender, e) =>
            {
                keywordTextBox_KeyDown(sender, e);
            });
            keywordTextBox.Focus();
        }

        private void TranslatorWindow_KeyDown(object sender, KeyEventArgs e)
        {
            var isControlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            var isShiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (e.Key == Key.Escape)
            {
                _manager.HideTranslatorAndSwitchToPrevWindow();
            }
            else if (isControlDown && isShiftDown && e.Key == Key.F)
            {
                keywordTextBox.Focus();
            }
        }

        private void keywordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TranslateKeyword();
            }
        }

        private void translateButton_Click(object sender, RoutedEventArgs e)
        {
            TranslateKeyword();
        }

        private async Task TranslateKeyword()
        {
            if (!String.IsNullOrEmpty(keywordTextBox.Text))
            {
                webBrowser.IsEnabled = true;
                string pageContent;
                try
                {
                    pageContent = await Page.GetTranslationPageContent(keywordTextBox.Text);
                }
                catch (PageRetrieveFailedException)
                {
                    pageContent = WinManager.Resources.translationRetrievalFailedMessage;
                }
                Page.SetupWebBrowser(webBrowser, pageContent, "cs", false);
            }
        }

        public void Display()
        {
            Show();
            Activate();
            keywordTextBox.Focus();
        }

    }
}