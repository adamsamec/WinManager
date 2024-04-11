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
        public uint Pid { get; set; }

        public OpenWindow(string title, IntPtr handle)
        {
            Title = title;
            Handle = handle;
            uint pid;
            NativeMethods.GetWindowThreadProcessId(handle, out pid);
            Pid = pid;
        }

        public override bool Equals(object other)
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

