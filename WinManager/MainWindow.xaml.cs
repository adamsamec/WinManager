using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace WinManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Manager _manager;
        private int _prevItemsListIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            _manager = new Manager(this);

            KeyDown += MainWindow_KeyDown;
            Closing += MainWindow_Closing;
        }

        private void SetWindowToolStyle(IntPtr handle)
        {
            uint extendedStyle = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(handle, NativeMethods.GWL_EXSTYLE, extendedStyle |
            NativeMethods.WS_EX_TOOLWINDOW);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Hide window from task switching
            SetWindowToolStyle(new WindowInteropHelper(this).Handle);

            _manager.HandleMainWindowLoad();
            itemsListBox.KeyDown += new KeyEventHandler((sender, e) => {
                var result = ItemsListBox_KeyDown(sender, e);
                });
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _manager.HideAndSwitchToPrevWindow();
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            _manager.CleanUp();
        }

        private async Task ItemsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    _manager.SwitchToItem(itemsListBox.SelectedIndex);
                    break;
                case Key.Delete:
                    var doForce = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                    var newIndex = await _manager.CloseItem(itemsListBox.SelectedIndex, doForce);
                        FocusItemAfterDelay(newIndex);
                    break; 
                case Key.Back:
                    _manager.ResetFilter();
                    break;
                case Key.Right:
                    _prevItemsListIndex = itemsListBox.SelectedIndex;
                    if (_manager.ShowSelectedAppWindows(itemsListBox.SelectedIndex))
                    {
                        FocusItemAfterDelay(0);
                    }
                    break;
                case Key.Left:
                    if (_manager.ShowApps())
                    {
                        FocusItemAfterDelay(_prevItemsListIndex);
                    }
                    break;
                default:
                    var character = e.Key.ToPrintableCharacter();
                    if (e.Key != Key.Tab && character != "")
                    {
                        _manager.ApplyTypedCharacterToFilter(character);
                        FocusItemAfterDelay(0);
                    }
                    break;
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new SettingsDialog(_manager);
            settingsDialog.Owner = this;
            settingsDialog.ShowDialog();
        }

        public void SetListBoxLabel(string text)
        {
            itemsListBoxLabel.Content = text;
        }

        public void Display()
        {
            Show();
            Activate();
            FocusItemAfterDelay(0);
        }
        public void FocusItemImmediately(int itemIndex)
        {
            itemsListBox.SelectedIndex = itemIndex;
            ((ListBoxItem)itemsListBox.SelectedItem).Focus();
        }

        private void FocusItemAfterDelay(int itemIndex)
        {
            itemsListBox.SelectedIndex = itemIndex;
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler((sender, e) =>
            {
                (sender as DispatcherTimer)?.Stop();
                ((ListBoxItem)itemsListBox.SelectedItem).Focus();
            });
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Start();
        }

        public void SetListBoxItems(List<string> itemsList)
        {
            itemsListBox.Items.Clear();
            if (itemsList.Count == 0)
            {
                var listBoxItem = new ListBoxItem();
                listBoxItem.Content = _manager.View == Manager.ListView.Apps ? WinManager.Resources.noAppsFound : WinManager.Resources.noWindowsFound;
                itemsListBox.Items.Add(listBoxItem);
                return;
            }
            foreach (var item in itemsList)
            {
                var listBoxItem = new ListBoxItem();
                listBoxItem.Content = item;
                itemsListBox.Items.Add(listBoxItem);
            }
        }
    }
}
