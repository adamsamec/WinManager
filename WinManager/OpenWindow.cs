using System.Diagnostics;

namespace WinManager
{
    /// <summary>
    /// Class for storing open window information
    /// </summary>
    public class OpenWindow
    {
        public string Title { get; set; }
        public IntPtr Handle { get; set; }
        public Process? WindowProcess { get; set; }
        public uint Pid { get; }

        public OpenWindow(string title, IntPtr handle, Process? windowProcess = null)
        {
            Title = title;
            Handle = handle;
            WindowProcess = windowProcess;

            if (windowProcess == null)
            {
                uint pid;
                NativeMethods.GetWindowThreadProcessId(handle, out pid);
                Pid = pid;
            }
            else
            {
                Pid = (uint)windowProcess.Id;
            }
        }

        public override bool Equals(object? other)
        {
            var otherWindow = other as OpenWindow;
            if (otherWindow == null)
            {
                return false;
            }
            return Handle == otherWindow.Handle;
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

    }
}

