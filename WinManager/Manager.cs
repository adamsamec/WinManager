﻿using AccessibleOutput;
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
        private string _appsOrWindowsFilterText;
        private string _SelectedAppWindowsFilterText;
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
            UpdateApps();
            UpdateWindows();
            ShowApps();

            // Display main window
            _mainWindow.Show(); // This extra Show() fixes the initial display
            _mainWindow.Display();
            if (AppUpdater.IsDownloading)
            {
                _mainWindow.OpenSettings();
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
            var appsItemsList = new List<string>();
            foreach (var app in _filteredAppsList)
            {
                appsItemsList.Add(GetAppItemText(app));
            }
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
            _filteredWindowsList.Clear();
            _SelectedAppWindowsFilterText = "";
            _selectedAppIndex = appIndex;
            var windows = _filteredAppsList[appIndex].Windows;
            foreach (var window in windows)
            {
                _filteredWindowsList.Add(window);
            }
            _view = ListView.SelectedAppWindows;

            // Determine and update ListBox items
            var windowsTitlesList = new List<string>();
            foreach (var window in _filteredWindowsList)
            {
                windowsTitlesList.Add(window.Title);
            }
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
                if (String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    continue;
                }
                string appName;
                try
                {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(process.GetMainModuleFilePath());
                    appName = fileVersionInfo.FileDescription;
                    if (String.IsNullOrEmpty(appName))
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    appName = process.MainWindowTitle;
                }

                var app = new RunningApplication(appName, process);
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

            // Update filtred apps list
            _filteredAppsList.Clear();
            _appsOrWindowsFilterText = "";
            foreach (var app in _appsList)
            {
                _filteredAppsList.Add(app);
            }
        }

        private void UpdateWindows()
        {
            var windows = WindowsFinder.GetWindows();
            foreach (var window in windows)
            {
                foreach (var app in _filteredAppsList)
                {
                    if (window.Pid == app.AppProcess.Id)
                    {
                        app.Windows.Add(window);
                    }
                }
            }
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
            if (_config.AppSettings.filterByTyping != Config.TRUE)
            {
                return;
            }

            character = character.ToLower();
            var itemsTextsList = new List<string>();
            switch (View)
            {
                case ListView.Apps:
                    _appsOrWindowsFilterText += character;
                    _filteredAppsList.Clear();
                    foreach (var app in _appsList)
                    {
                        if (app.Name.ToLower().Contains(_appsOrWindowsFilterText))
                        {
                            _filteredAppsList.Add(app);
                            itemsTextsList.Add(GetAppItemText(app));
                        }
                    }
                    break;
                case ListView.SelectedAppWindows:
                    _SelectedAppWindowsFilterText += character;
                    _filteredWindowsList.Clear();
                    var windows = _filteredAppsList[_selectedAppIndex].Windows;
                    foreach (var window in windows)
                    {
                        if (window.Title.ToLower().Contains(_SelectedAppWindowsFilterText))
                        {
                            _filteredWindowsList.Add(window);
                            itemsTextsList.Add(window.Title);
                        }
                    }
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
                    _filteredAppsList.Clear();
                    foreach (var app in _appsList)
                    {
                        _filteredAppsList.Add(app);
                        itemsTextsList.Add(GetAppItemText(app));
                    }
                    break;
                case ListView.SelectedAppWindows:
                    _filteredWindowsList.Clear();
                    var windows = _filteredAppsList[_selectedAppIndex].Windows;
                    foreach (var window in windows)
                    {
                        _filteredWindowsList.Add(window);
                        itemsTextsList.Add(window.Title);
                    }
                    break;
            }

            _mainWindow.SetListBoxItems(itemsTextsList);
        }

        public void ChangeLaunchOnStartupSetting(bool value)
        {
            // The path to the key where Windows looks for startup applications
            RegistryKey startupRegistryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            //Path to the WinManager launch executable
            var executablePath = Path.Combine(Consts.InstallFolder, Consts.ExecutableFilename);

            // Modify the registry
            try
            {
                if (value)
                {
                    startupRegistryKey.SetValue(Consts.StartupRegistryKeyName, executablePath);
                }
                else if (startupRegistryKey.GetValue(Consts.StartupRegistryKeyName, "none") != "none")
                {
                    startupRegistryKey.DeleteValue(Consts.StartupRegistryKeyName);
                }
            }
            catch (Exception ex)
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
