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
        private MainWindow _mainWindow;
        private KeyboardHook _hook;
        private AutoOutput _srOutput = new AutoOutput();
        private Config _config = new Config();
        private Updater _appUpdater = new Updater();

        private List<RunningApplication> _appsList = new List<RunningApplication>();
        private List<RunningApplication> _filteredAppsList = new List<RunningApplication>();
        private List<OpenWindow> _filteredWindowsList = new List<OpenWindow>();
        private string _appsOrWindowsFilterText = "";
        private string _SelectedAppWindowsFilterText = "";
        private int _selectedAppIndex = 0;
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
            FrontAppWindows
        }

        public Manager(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _hook = new KeyboardHook(_mainWindow, 0x77, ModifierKeyCodes.Windows); // Windows + F8
            _hook.Triggered += Show;

            // Update WinManager launch on startup seting from config
            var isLaunchOnStartupEnabled = AppSettings.launchOnStartup == Config.TRUE;
            ChangeLaunchOnStartupSetting(isLaunchOnStartupEnabled);

            // Announce WinManager start
            Speak(Resources.startAnnouncement);
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

        private void Show()
        {
            SystemSounds.Hand.Play();
            _prevWindowHandle = NativeMethods.GetForegroundWindow();
            _view = ListView.Hidden;

            UpdateApps();
            ShowApps();

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

        public bool ShowSelectedAppWindows(int appIndex)
        {
            if (View != ListView.Apps || _filteredAppsList.Count == 0)
            {
                return false;
            }

            // Determine selected app and its windows
            _SelectedAppWindowsFilterText = "";
            _selectedAppIndex = appIndex;
            _filteredWindowsList = new List<OpenWindow>(_filteredAppsList[appIndex].Windows);
            _view = ListView.SelectedAppWindows;

            // Determine and update ListBox items
            var windowsTitlesList = _filteredWindowsList.Select(window => window.Title).ToList();
            _mainWindow.SetListBoxLabel(Resources.openWindowsLabel);
            _mainWindow.SetListBoxItems(windowsTitlesList);

            return true;
        }

        private void UpdateApps()
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
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(process.GetMainModuleFilePath());
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

            // Turn apps with the same process name into windows
            _appsList.Clear();
            foreach (var processApp in processesAppsList)
            {
                var appExists = false;
                foreach (var app in _appsList)
                {
                    var process = processApp.AppProcess;
                    if (app.AppProcess.ProcessName == process.ProcessName)
                    {
                        appExists = true;
                        var handle = process.Handle;
                        uint pid;
                        NativeMethods.GetWindowThreadProcessId(handle, out pid);
                        var window = new OpenWindow(process.MainWindowTitle, handle, pid);
                        app.Windows.Add(window);
                    }
                }
                if (!appExists)
                {
                    _appsList.Add(processApp);
                }
            }
            UpdateWindows();

            // Reset filtred apps list
            _filteredAppsList = new List<RunningApplication>(_appsList);
            _appsOrWindowsFilterText = "";
        }

        private void UpdateWindows()
        {
            var windows = WindowsFinder.GetWindows();

            // Categorize windows into apps
            foreach (var window in windows)
            {
                foreach (var app in _appsList)
                {
                    if (window.Pid == app.AppProcess.Id)
                    {
                        app.Windows.Add(window);
                    }
                }
            }

            // Remove apps that have no windows
            _appsList = _appsList.Where(app => app.Windows.Count > 0).ToList();
        }

        public void SwitchToItem(int itemIndex)
        {
            // Ignore switching if there are no items after applying filter
            if (View == ListView.Apps && _filteredAppsList.Count == 0)
            {
                return;
            }
            if ((View == ListView.FrontAppWindows || View == ListView.SelectedAppWindows) && _filteredWindowsList.Count == 0)
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
            character = character.ToLower();
            var itemsTextsList = new List<string>();
            switch (View)
            {
                case ListView.Apps:
                    _appsOrWindowsFilterText += character;
                    _filteredAppsList = _appsList.Where(app =>
                    {
                        var doesMatch = app.Name.ToLower().Contains(_appsOrWindowsFilterText);
                        if (doesMatch)
                        {
                            itemsTextsList.Add(GetAppItemText(app));
                        }
                        return doesMatch;
                    }).ToList();
                    break;
                case ListView.SelectedAppWindows:
                    _SelectedAppWindowsFilterText += character;
                    _filteredWindowsList = _filteredAppsList[_selectedAppIndex].Windows.Where(window =>
                    {
                        var doesMatch = window.Title.ToLower().Contains(_SelectedAppWindowsFilterText);
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
            var itemsTextsList = new List<string>();
            switch (View)
            {
                case ListView.Apps:
                    _appsOrWindowsFilterText = "";
                    _filteredAppsList = _appsList.Select(app =>
                    {
                        itemsTextsList.Add(GetAppItemText(app));
                        return app;
                    }).ToList();
                    break;
                case ListView.SelectedAppWindows:
                    _SelectedAppWindowsFilterText = "";
                    _filteredWindowsList = _filteredAppsList[_selectedAppIndex].Windows.Select(window =>
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
            AppSettings.launchOnStartup = value ? Config.TRUE : Config.FALSE; ;
            SaveSettings();
        }

        private void SaveSettings()
        {
            _config.Save();
        }

        public void CleanUp()
        {
            _hook.Dispose();
        }
    }
}
