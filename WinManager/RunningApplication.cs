using System.Diagnostics;
using System.Reflection.Metadata;

namespace WinManager
{
    /// <summary>
    /// Class for storing running application information
    /// </summary>
    public class RunningApplication
    {
        private int _zIndex;

        public string Name { get; set; }
        public Process AppProcess { get; set; }
        public List<OpenWindow> Windows { get; set; }
        public int ZIndex
        {
            get { return _zIndex; }
        }

        public RunningApplication(string name, Process process)
        {
            Name = name;
            AppProcess = process;
            Windows = new List<OpenWindow>();

            SetZOrder();
        }

        public void SetZOrder()
        {
            IntPtr handle = AppProcess.MainWindowHandle;
            var z = 0;
            // 3 is GetWindowType.GW_HWNDPREV
            for (var h = handle; h != IntPtr.Zero; h = NativeMethods.GetWindow(h, 3))
            {
                z++;
            }
            _zIndex = z;
        }

        public override bool Equals(object other)
        {
            var otherApp = other as RunningApplication;
            if (otherApp == null)
            {
                return false;
            }
            return AppProcess.ProcessName == otherApp.AppProcess.ProcessName;
        }

        public override int GetHashCode()
        {
            return AppProcess.ProcessName.GetHashCode();
        }
    }
}

