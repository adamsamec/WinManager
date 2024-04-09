using AccessibleOutput;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Media;

namespace WinManager
{
    /// <summary>
    /// Main application logic
    /// </summary>
    public class Manager
    {
        private IntPtr _prevWindowHandle = NativeMethods.GetForegroundWindow();
        private Config _config = new Config();
        private List<KeyboardHook> _keyboardHooks = new List<KeyboardHook>();
        private MainWindow _mainWindow;
        private AutoOutput _srOutput = new AutoOutput();
        private Updater _appUpdater = new Updater();

        private List<RunningApplication> _appsList = new List<RunningApplication>();
        private List<RunningApplication> _filteredAppsList = new List<RunningApplication>();
        private List<OpenWindow> _filteredWindowsList = new List<OpenWindow>();
        private string _appsFilterText = "";
        private string _windowsFilterText = "";
        private int _currentAppIndex = 0;
        private ListView _view = ListView.Hidden;

        public Settings AppSettings
        {
            get { return _config.AppSettings; }
        }
        public ListView View
        {
            get { return _view; }
        }
        public Updater AppUpdater
        {
            get { return _appUpdater; }
        }

        public enum ListView
        {
            Hidden,
            Apps,
            SelectedAppWindows,
            ForegroundAppWindows
        }

        public Manager(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            InitTriggerShortcuts();

            // Update WinManager launch on startup seting from config
            var isLaunchOnStartupEnabled = AppSettings.launchOnStartup == Config.True;
            ChangeLaunchOnStartupSetting(isLaunchOnStartupEnabled);

            // Announce WinManager start
            Speak(Resources.startAnnouncement);
        }

        private void InitTriggerShortcuts()
        {
            var actionMapping = new List<Object> {
AppSettings.enabledShortcuts.showApps,
AppSettings.enabledShortcuts.showWindows,
            };
            foreach (var shortcut in _config.TriggerShortcuts)
            {
                // Determine enabled state for shortcuts from settings
                var settingAction = actionMapping[(int)shortcut.Action];
                shortcut.IsEnabled = ((string) Utils.GetPropValue(settingAction, shortcut.Id)) == Config.True;

                // Create keyboard hook if shortcut is enabled
                if (shortcut.IsEnabled)
                {
                    var hook = new KeyboardHook(_mainWindow, shortcut.KeyCode, shortcut.Modifiers);
                    hook.Triggered += () =>
                    {
                        Show(shortcut.Action);
                    };
                    _keyboardHooks.Add(hook);
                }
            }
        }

        public void Speak(string message)
        {
            _srOutput.Speak(message);
        }

        public void HandleMainWindowLoad()
        {
            Hide();
        }

        public void HideAndSwitchToPrevWindow()
        {
            Hide();
            NativeMethods.SetForegroundWindow(_prevWindowHandle);
            NativeMethods.SetActiveWindow(_prevWindowHandle);
        }

        private void Hide()
        {
            _mainWindow.Hide();
        }

        private void Show(TriggerShortcut.TriggerAction type)
        {
            SystemSounds.Hand.Play();
            _prevWindowHandle = NativeMethods.GetForegroundWindow();
            _view = ListView.Hidden;

            RefreshApps();
            switch (type)
            {
                case TriggerShortcut.TriggerAction.ShowApps:
                    ShowApps();
                    break;
                case TriggerShortcut.TriggerAction.ShowWindows:
                    ShowForeGroundAppWindows();
                    break;
            }

            // Display main window
            _mainWindow.Show(); // This extra Show() fixes the initial display
            _mainWindow.Display();
            if (AppUpdater.State == Updater.UpdateState.Downloading || AppUpdater.State == Updater.UpdateState.Deleting)
            {
                _mainWindow.ShowSettingsDialog();
            }
        }

        public bool ShowApps()
        {
            if (View != ListView.Hidden && View != ListView.SelectedAppWindows)
            {
                return false;
            }

            _view = ListView.Apps;

            // Determine and update ListBox items
            var appsItemsList = _filteredAppsList.Select(app => GetAppItemText(app)).ToList();
            _mainWindow.SetListBoxLabel(Resources.runningAppsLabel);
            _mainWindow.SetListBoxItems(appsItemsList);

            return true;
        }

        public static string GetAppItemText(RunningApplication app)
        {
            var itemText = $"{app.Name} ({app.Windows.Count} {Resources.windowsCount})";
            return itemText;
        }

        public bool ShowForeGroundAppWindows()
        {
            if (View != ListView.Hidden)
            {
                return false;
            }

            // Determine foreground app, its index and its windows
            var appIndex = -1;
            var foregroundApp = _appsList.Find(app =>
            {
                appIndex++;
                return app.Windows[0].Handle == _prevWindowHandle;
            });
            if (foregroundApp == null)
            {
                _currentAppIndex = -1;
                _filteredWindowsList.Clear();
            }
            else
            {
                _currentAppIndex = appIndex;
                _filteredWindowsList = new List<OpenWindow>(foregroundApp.Windows);
            }

            _windowsFilterText = "";
            _view = ListView.ForegroundAppWindows;

            // Determine and update ListBox items
            var windowsTitlesList = _filteredWindowsList.Select(window => window.Title).ToList();
            _mainWindow.SetListBoxLabel(Resources.openWindowsLabel);
            _mainWindow.SetListBoxItems(windowsTitlesList);

            return true;
        }

        public bool ShowSelectedAppWindows(int appIndex)
        {
            if (View != ListView.Apps || _filteredAppsList.Count == 0)
            {
                return false;
            }

            // Determine selected app, its index and its windows
            _currentAppIndex = appIndex;
            _filteredWindowsList = new List<OpenWindow>(_filteredAppsList[appIndex].Windows);

            _windowsFilterText = "";
            _view = ListView.SelectedAppWindows;

            // Determine and update ListBox items
            var windowsTitlesList = _filteredWindowsList.Select(window => window.Title).ToList();
            _mainWindow.SetListBoxLabel(Resources.openWindowsLabel);
            _mainWindow.SetListBoxItems(windowsTitlesList);

            return true;
        }

        private void RefreshApps()
        {
            var processes = Process.GetProcesses();
            var processesAppsList = new List<RunningApplication>();
            foreach (var process in processes)
            {
                RunningApplication app;
                if (process.ProcessName == "explorer")
                {
                    app = new RunningApplication(Resources.fileExplorer, process);
                    processesAppsList.Add(app);
                    continue;
                }
                if (String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    continue;
                }
                string? appName;
                try
                {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(process.GetFilePath());
                    appName = fileVersionInfo.FileDescription;
                    if (String.IsNullOrEmpty(appName))
                    {
                        continue;
                    }
                }
                catch (Exception)
                {
                    appName = process.MainWindowTitle;
                }

                app = new RunningApplication(appName, process);
                processesAppsList.Add(app);
            }
            processesAppsList = processesAppsList.OrderBy(app => app.ZIndex).ToList();
            RefreshAndCategorizeWindows(ref processesAppsList);
            _appsList.Clear();

            // Turn apps with the same process name into windows
            foreach (var processApp in processesAppsList)
            {
                var appExists = false;
                foreach (var app in _appsList)
                {
                    var process = processApp.AppProcess;
                    if (app.AppProcess.ProcessName == process.ProcessName)
                    {
                        appExists = true;
                        if (processApp.Windows.Count > 0)
                        {
                            var window = new OpenWindow(process.MainWindowTitle, processApp.Windows[0].Handle);
                            app.Windows.Add(window);
                        }
                    }
                }
                if (!appExists)
                {
                    _appsList.Add(processApp);
                }
            }

            // Reset filtred apps list
            _filteredAppsList = new List<RunningApplication>(_appsList);
            _appsFilterText = "";
        }

        private void RefreshAndCategorizeWindows(ref List<RunningApplication> appsList)
        {
            var windows = WindowsFinder.GetWindows();

            // Categorize windows into apps
            foreach (var window in windows)
            {
                foreach (var app in appsList)
                {
                    if (window.Pid == app.AppProcess.Id)
                    {
                        app.Windows.Add(window);
                    }
                }
            }

            // Remove apps that have no windows
            appsList = appsList.Where(app => app.Windows.Count > 0).ToList();
        }

        public void SwitchToItem(int itemIndex)
        {
            // Ignore switching if there are no items after applying filter
            if (View == ListView.Apps && _filteredAppsList.Count == 0)
            {
                return;
            }
            if ((View == ListView.ForegroundAppWindows || View == ListView.SelectedAppWindows) && _filteredWindowsList.Count == 0)
            {
                return;
            }

            // Determine window handle to switch to
            IntPtr handle = -1;
            switch (View)
            {
                case ListView.Apps:
                    var process = _filteredAppsList[itemIndex].AppProcess;
                    handle = process.MainWindowHandle;
                    break;
                case ListView.ForegroundAppWindows:
                case ListView.SelectedAppWindows:
                    handle = _filteredWindowsList[itemIndex].Handle;
                    break;
            }
            if (handle == -1)
            {
                return;
            }

            // Hide WinManager window and switch to app or window using their handle
            Hide();
            NativeMethods.ShowWindow(handle, 5);
            NativeMethods.SetForegroundWindow(handle);
            NativeMethods.SetActiveWindow(handle);
        }

        public void ApplyTypedCharacterToFilter(string character)
        {
            if (View == ListView.ForegroundAppWindows && _currentAppIndex == -1)
            {
                return;
            }

            character = character.ToLower();
            var itemsTextsList = new List<string>();
            switch (View)
            {
                case ListView.Apps:
                    _appsFilterText += character;
                    _filteredAppsList = _appsList.Where(app =>
                    {
                        var doesMatch = app.Name.ToLower().Contains(_appsFilterText);
                        if (doesMatch)
                        {
                            itemsTextsList.Add(GetAppItemText(app));
                        }
                        return doesMatch;
                    }).ToList();
                    break;
                case ListView.ForegroundAppWindows:
                case ListView.SelectedAppWindows:
                    _windowsFilterText += character;
                    _filteredWindowsList = _filteredAppsList[_currentAppIndex].Windows.Where(window =>
                    {
                        var doesMatch = window.Title.ToLower().Contains(_windowsFilterText);
                        if (doesMatch)
                        {
                            itemsTextsList.Add(window.Title);
                        }
                        return doesMatch;
                    }).ToList();
                    break;
            }

            _mainWindow.SetListBoxItems(itemsTextsList);
        }

        public void ResetFilter()
        {
            if (View == ListView.ForegroundAppWindows && _currentAppIndex == -1)
            {
                return;
            }

            var itemsTextsList = new List<string>();
            switch (View)
            {
                case ListView.Apps:
                    _appsFilterText = "";
                    _filteredAppsList = _appsList.Select(app =>
                    {
                        itemsTextsList.Add(GetAppItemText(app));
                        return app;
                    }).ToList();
                    break;
                case ListView.ForegroundAppWindows:
                case ListView.SelectedAppWindows:
                    _windowsFilterText = "";
                    _filteredWindowsList = _filteredAppsList[_currentAppIndex].Windows.Select(window =>
                    {
                        itemsTextsList.Add(window.Title);
                        return window;
                    }).ToList();
                    break;
            }
            _mainWindow.SetListBoxItems(itemsTextsList);
        }

        public void ChangeLaunchOnStartupSetting(bool value)
        {
            // The path to the key where Windows looks for startup applications
            var startupRegistryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            //Path to the WinManager launch executable
            var executablePath = Path.Combine(Consts.InstallFolder, Consts.ExecutableFilename);

            // Modify the registry
            try
            {
                if (value)
                {
                    startupRegistryKey?.SetValue(Consts.StartupRegistryKeyName, executablePath);
                }
                else if (startupRegistryKey != null && (string)startupRegistryKey.GetValue(Consts.StartupRegistryKeyName, "none") != "none")
                {
                    startupRegistryKey.DeleteValue(Consts.StartupRegistryKeyName);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to update launch on startup registry");
            }
            // Update settings
            AppSettings.launchOnStartup = value ? Config.True : Config.False; ;
            SaveSettings();
        }

        private void SaveSettings()
        {
            _config.Save();
        }

        public void CleanUp()
        {
            foreach (var hook in _keyboardHooks)
            {
                hook.Dispose();
            }
            Speak(Resources.exitAnnouncement);

        }
    }
}
