using System.Diagnostics;

namespace WinManager
{
    /// <summary>
    /// Class for storing running application information
    /// </summary>
    public class RunningApplication
    {
        private int _zIndex;

        public string Name { get; set; }
        public Process? AppProcess { get; set; }
        public bool HasWindowsWithOwnProcesses { get; set; }
        public List<OpenWindow> Windows { get; set; }
        public IntPtr Handle
        {
            // In case of modern app and explorer.exe, use the first window handle
            get { return AppProcess == null || AppProcess.ProcessName == "explorer" ? Windows[0].Handle : AppProcess.MainWindowHandle; }
        }
        public int ZIndex
        {
            get { return _zIndex; }
        }

        public RunningApplication(string name, Process? process, bool hasWindowsWithOwnProcesses = false)
        {
            Name = name;
            AppProcess = process;
            HasWindowsWithOwnProcesses = hasWindowsWithOwnProcesses;
            Windows = new List<OpenWindow>();
        }

        public RunningApplication(string name) : this(name, null) { }

        public void SetZOrder()
        {
            IntPtr handle;
            if (AppProcess == null || Windows.Count >= 1)
            {
                handle = Windows[0].Handle;
            }
            else
            {
                handle = AppProcess.MainWindowHandle;
            }
            var z = 0;
            // 3 is GetWindowType.GW_HWNDPREV
            for (var h = handle; h != IntPtr.Zero; h = NativeMethods.GetWindow(h, 3))
            {
                z++;
            }
            _zIndex = z;
        }

        public override bool Equals(object? other)
        {
            var otherApp = other as RunningApplication;
            if (otherApp == null)
            {
                return false;
            }
            if (AppProcess != null && otherApp.AppProcess != null)
            {
                return AppProcess.ProcessName == otherApp.AppProcess.ProcessName;
            }
            else
            {
                return Name == otherApp.Name;
            }
        }

        public override int GetHashCode()
        {
            if (AppProcess != null)
            {
                return AppProcess.ProcessName.GetHashCode();
            }
            else
            {
                return Name.GetHashCode();
            }
        }
    }
}

