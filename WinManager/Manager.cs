using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Media;
using System.Reflection;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace WinManager
{
    /// <summary>
    /// Main application logic
    /// </summary>
    public class Manager
    {
        private int _processId = Process.GetCurrentProcess().Id;
        private IntPtr _handle = NativeMethods.GetForegroundWindow();
        private IntPtr _prevWindowHandle = NativeMethods.GetForegroundWindow();
        private Config _config = new Config();
        private MainWindow _mainWindow;
        private SpeechSynthesizer _speech = new SpeechSynthesizer();
        private Updater _appUpdater = new Updater();
        private bool _hasCheckedForUpdateOnFirstShow = false;

        private List<RunningApplication> _appsList = new List<RunningApplication>();
        private List<RunningApplication> _filteredAppsList = new List<RunningApplication>();
        private List<OpenWindow> _filteredWindowsList = new List<OpenWindow>();
        private string _appsFilterText = "";
        private string _windowsFilterText = "";
        private int _currentAppIndex = 0;
        private ListView _view = ListView.Hidden;

        private const int WaitForAppKillTimeLimit = 1500;
        private const int RefreshAfterAppQuitDelay = 1500;
        private const int RefreshAfterWindowCloseDelay = 500;

        public Settings AppSettings
        {
            get { return _config.AppSettings; }
        }
        public List<TriggerShortcut> TriggerShortcuts
        {
            get { return _config.TriggerShortcuts; }
        }
        public int CurrentAppIndex
        { 
        get { return _currentAppIndex; }
}
        public ListView View
        {
            get { return _view; }
        }
        public Updater AppUpdater
        {
            get { return _appUpdater; }
        }
        public UpdateData? AppUpdateData { get; set; }

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
            InitSettingsFromConfig();
            InitSpeech();

            // Announce WinManager start
            Speak(Resources.startAnnouncement);
        }

        private void InitTriggerShortcuts()
        {
            foreach (var shortcut in _config.TriggerShortcuts)
            {
                // Determine enabled state for shortcuts from settings
                var settingAction = _config.GetActionSetting(shortcut.Action);
                shortcut.IsEnabled = Config.StringToBool((string)Utils.GetPropValue(settingAction, shortcut.Id));

                // Create keyboard hook if shortcut is enabled
                if (shortcut.IsEnabled)
                {
                    UpdateHook(shortcut);
                }
            }
        }

        private void InitSettingsFromConfig()
        {
            // Update WinManager launch on startup seting from config
            var isLaunchOnStartupEnabled = Config.StringToBool(AppSettings.launchOnStartup);
            ChangeLaunchOnStartupSetting(isLaunchOnStartupEnabled);
        }

        private void InitSpeech()
        {
            try
            {
                _speech.InjectOneCoreVoices();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception during OneCore voices injection: " + ex.ToString());
            }
            _speech.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet, 0, WinManager.Resources.Culture);
        }

        private void UpdateHook(TriggerShortcut shortcut)
        {
            if (shortcut.IsEnabled && shortcut.Hook == null)
            {
                shortcut.Hook = new KeyboardHook(_mainWindow, shortcut.KeyCode, shortcut.Modifiers);
                shortcut.Hook.Triggered += () =>
                {
                    Show(shortcut.Action);
                };
            }
            else if (shortcut.Hook != null)
            {
                shortcut.Hook.Dispose();
                shortcut.Hook = null;
            }
        }

        public void Speak(string message)
        {
            _speech.Speak(message);
        }

        private async void CheckForUpdateOnFirstShow()
        {
            if (_hasCheckedForUpdateOnFirstShow || !Config.StringToBool(AppSettings.checkUpdateOnFirstShow))
            {
                return;
            }
            _hasCheckedForUpdateOnFirstShow = true;
            try
            {
                AppUpdateData = await AppUpdater.CheckForUpdate();
                if (AppUpdateData != null)
                {
                    var updateAvailableDialog = new UpdateAvailableDialog(this, AppUpdateData);
                    var doUpdate = updateAvailableDialog.ShowDialog() == true;
                    if (doUpdate)
                    {
                        var downloadingUpdateDialog = new DownloadingUpdateDialog(this, AppUpdateData);
                        downloadingUpdateDialog.DownloadUpdate();
                        downloadingUpdateDialog.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                // Ignore if check for update fails
                Debug.WriteLine("Exception during first launch update check: " + ex.ToString());
            }
        }

        private void CheckOngoingUpdateDownload()
        {
            if (AppUpdater.State == Updater.UpdateState.Downloading || AppUpdater.State == Updater.UpdateState.Deleting)
            {
                var downloadingUpdateDialog = new DownloadingUpdateDialog(this, AppUpdateData);
                downloadingUpdateDialog.Owner = _mainWindow;
                downloadingUpdateDialog.ShowDialog();
            }
            else if (AppUpdater.DownloadingDialog != null)
            {
                // Make sure possibly existing previous launch update installer dialog is closed before opening it again
                if (AppUpdater.LaunchInstallerDialog != null && AppUpdater.LaunchInstallerDialog.IsVisible)
                {
                    AppUpdater.LaunchInstallerDialog.DialogResult = false;
                }
                AppUpdater.DownloadingDialog.ShowLaunchUpdateInstallerDialog();
            }
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
            var newPrevHandle = NativeMethods.GetForegroundWindow();

            // Fix for situation when trying to show WinManager if already shown
            if (newPrevHandle != _handle)
            {
                _prevWindowHandle = newPrevHandle;
            }

            // Don't show when WinManager is already shown
            if (IsShown())
            {
                return;
            }

            SystemSounds.Hand.Play();
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

            CheckForUpdateOnFirstShow();
            CheckOngoingUpdateDownload();
        }

        private bool IsShown()
        {
            // WinManager is not activated if main window is not visible or if no window is in foregroud 
            if (!_mainWindow.IsVisible || _prevWindowHandle == IntPtr.Zero)
            {
                return false;
            }
            uint prevProcessId;
            NativeMethods.GetWindowThreadProcessId(_prevWindowHandle, out prevProcessId);
            return prevProcessId == _processId;
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
                // Override certain app names
                if (Consts.AppNamesOverrides.ContainsKey(appName))
                {
                    appName = Consts.AppNamesOverrides[appName];
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
                            var window = new OpenWindow(process.MainWindowTitle, processApp.Windows[0].Handle, process);
                            app.Windows.Add(window);
                            app.HasWindowsWithOwnProcesses = true;
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

        public async Task<int> CloseItem(int itemIndex, bool doForce)
        {
            int newIndex = 0;

            // Ignore closing if there are no items after applying filter
            if (View == ListView.Apps && _filteredAppsList.Count == 0)
            {
                return newIndex;
            }
            if ((View == ListView.ForegroundAppWindows || View == ListView.SelectedAppWindows) && _filteredWindowsList.Count == 0)
            {
                return newIndex;
            }

            // Determine app or window to close
            switch (View)
            {
                case ListView.Apps:
                    var appToQuit = _filteredAppsList[itemIndex];
                    var isAppQuitted = false;
                    if (doForce)
                    {
                        Speak(Resources.forceQuittingApp);
                        var processesKillTasks = new List<Task>();

                        // If app has windows with own process, kill each such window asynchronously, with a time limit for each kill task
                        if (appToQuit.HasWindowsWithOwnProcesses)
                        {
                            foreach (var window in appToQuit.Windows)
                            {
                                if (window.WindowProcess != null)
                                {
                                    var windowKillTask = KillAsync(window.WindowProcess);
                                    processesKillTasks.Add(windowKillTask);
                                }
                            }
                        }
                        var appKillTask = KillAsync(appToQuit.AppProcess);
                        processesKillTasks.Add(appKillTask);

                        // Wait until all processes kill tasks are done
                        await Task.WhenAll(processesKillTasks);

                        isAppQuitted = true;
                    }
                    else // We are not force quitting
                    {
                        Speak(Resources.quittingApp);

                        // Close all app windows individually
                        foreach (var window in appToQuit.Windows)
                        {
                            // Run window closing message in a new thread to prevent WinManager window blocking if closing fails
                            new Thread(() =>
                            {
                                Thread.CurrentThread.IsBackground = true;
                                NativeMethods.SendMessage(window.Handle, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                            }).Start();
                        }

                        // Give some time to close all app windows
                        Thread.Sleep(RefreshAfterAppQuitDelay);
                    }

                    // Refresh apps list, then check if closing succeeded
                    RefreshApps();
                    if (_appsList.Contains(appToQuit))
                    {
                        Speak(Resources.quittingAppFailed);
                    }
                    else
                    {
                        ApplyFilter();
                        isAppQuitted = true;
                    }
                    newIndex = isAppQuitted ? Math.Max(Math.Min(itemIndex, _filteredAppsList.Count - 1), 0) : itemIndex;
                    break;
                case ListView.ForegroundAppWindows:
                case ListView.SelectedAppWindows:
                    Speak(Resources.closingWindow);
                    var handle = _filteredWindowsList[itemIndex].Handle;

                    // Run window closing message in a new thread to prevent WinManager window blocking if closing fails
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        NativeMethods.SendMessage(handle, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    }).Start();

                    // Give some time for closing, refresh apps and windows list, then check if closing succeeded
                    Thread.Sleep(RefreshAfterWindowCloseDelay);
                    var appWithWindowToClose = _filteredAppsList[_currentAppIndex];
                    var windowToClose = _filteredWindowsList[itemIndex];
                    RefreshApps();
                    var appIndex = -1;
                    var refreshedAppWithWindowToClose = _filteredAppsList.Find(app => {
                        appIndex += 1;
                        var doFilter = app.Equals(appWithWindowToClose);
                        return doFilter;
                        });
                    if (refreshedAppWithWindowToClose != null && _appsList.Contains(refreshedAppWithWindowToClose))
                    {
                    _currentAppIndex = appIndex;
                        var closingFailed = false;
                        foreach (var window in refreshedAppWithWindowToClose.Windows)
                        {
                            if (window.Equals(windowToClose))
                            {
                                closingFailed = true;
                            }
                        }
                        if (closingFailed)
                        {
                            Speak(Resources.closingWindowFailed);
                        }
                        else
                        {
                            ApplyFilter();
                            newIndex = Math.Max(Math.Min(itemIndex, _filteredWindowsList.Count - 1), 0);
                        }
                    }
                    else
                    {
                        _view = ListView.Apps;
                        _mainWindow.SetListBoxLabel(Resources.runningAppsLabel);
                        ApplyFilter();
                        newIndex = Math.Max(Math.Min(_currentAppIndex, _filteredAppsList.Count - 1), 0);
                    }
                    break;
            }
            return newIndex;
        }

        private async Task KillAsync(Process process)
        {
            process.Kill();
            process.WaitForExit(WaitForAppKillTimeLimit);
        }

        public void RefreshAppsAndApplyFilter()
        {
            RefreshApps();
            ApplyFilter();
        }

        public void ApplyFilter()
        {
            ApplyTypedCharacterToFilter("");
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

        public bool ChangeEnabledStateForTriggerShortcut(TriggerShortcut shortcut, bool newState)
        {
            // Ensure at least one another shortcugt with the same action is enabled before disabling this one
            if (!newState)
            {
                var isAnotherEnabled = _config.TriggerShortcuts.Find(anotherShortcut =>
                {
                    return anotherShortcut.Id != shortcut.Id && anotherShortcut.Action == shortcut.Action && anotherShortcut.IsEnabled;
                }) != null;
                if (!isAnotherEnabled)
                {
                    return true;
                }
            }

            // Set new state to the local state and update hook accordingly
            shortcut.IsEnabled = newState;
            UpdateHook(shortcut);

            // Update settings accordinglyAAQ
            var actionSetting = _config.GetActionSetting(shortcut.Action);
            var newStateString = Config.BoolToString(newState);
            Utils.SetPropValue(actionSetting, shortcut.Id, newStateString);
            SaveSettings();
            return newState;
        }

        public void ChangeLaunchOnStartupSetting(bool value)
        {
            if (Consts.InstallFolder == null)
            {
                return;
            }
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
            catch (Exception ex)
            {
                Debug.WriteLine("Exception during updating launch on startup registry: " + ex.ToString());
            }
            // Update settings
            AppSettings.launchOnStartup = Config.BoolToString(value);
            SaveSettings();
        }

        public void ChangeCheckUpdateOnFirstShowSetting(bool value)
        {
            AppSettings.checkUpdateOnFirstShow = Config.BoolToString(value);
            SaveSettings();
        }

        private void SaveSettings()
        {
            _config.Save();
        }

        public void CleanUp()
        {
            foreach (var shortcut in TriggerShortcuts)
            {
                if (shortcut.Hook != null)
                {
                    shortcut.Hook.Dispose();
                }
            }
            Speak(Resources.exitAnnouncement);

        }
    }
}
