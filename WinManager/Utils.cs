using Accessibility;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace WinManager
{
    /// <summary>
    /// Utility class
    /// </summary>
    public static class Utils
    {
        private static string? _installFolder;

        public static string? GetInstallFolder()
        {
            if (_installFolder== null)
            {
            var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            _installFolder = System.IO.Path.GetDirectoryName(assemblyPath);
            }
            return _installFolder;
        }

        public static void SetWindowToolStyle(IntPtr handle)
        {
            var extendedStyle = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(handle, NativeMethods.GWL_EXSTYLE, extendedStyle |
            NativeMethods.WS_EX_TOOLWINDOW);
        }

        public static bool IsFileInUse(string filePath)
        {
            try
            {
                using (Stream stream = new FileStream(filePath, FileMode.Open))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }

        public static object? GetPropValue(object obj, string propName)
        {
            return obj.GetType().GetProperty(propName)?.GetValue(obj, null);
        }

        public static void SetPropValue(object obj, string propName, object? propValue)
        {
            obj.GetType().GetProperty(propName)?.SetValue(obj, propValue, null);
        }

        public static void SetYesOrNo(object obj, object defaultObj, string[] propNames)
        {
            foreach (var propName in propNames)
            {
                var propValue = (string?)GetPropValue(obj, propName);
                var defaultPropValue = (string?)GetPropValue(defaultObj, propName);
                var newPropValue = (propValue == "yes" || propValue == "no") ? propValue : defaultPropValue;
                SetPropValue(obj, propName, newPropValue);
            }
        }

        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        public static string? GetProcessFilePath(IntPtr handle, int buffer = 1024)
        {
            var filePathBuilder = new StringBuilder(buffer);
            var bufferLength = (uint)filePathBuilder.Capacity + 1;
            var path = NativeMethods.QueryFullProcessImageName(handle, 0, filePathBuilder, ref bufferLength) ?
                filePathBuilder.ToString() :
                null;
            return path;
        }

        public static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }

    internal static class Extensions
    {
        public static string? GetFilePath(this Process process)
        {
            return Utils.GetProcessFilePath(process.Handle);
        }

        public static string ToPrintableCharacter(this Key key)
        {
            // Consider Backspace and Escape as non-printable characters returning empty string
            if (key == Key.Back || key == Key.Escape)
            {
                return "";
            }

            var character = "";
            var virtualKey = KeyInterop.VirtualKeyFromKey(key);
            var keyboardState = new byte[256];
            NativeMethods.GetKeyboardState(keyboardState);

            var scanCode = NativeMethods.MapVirtualKey((uint)virtualKey, Utils.MapType.MAPVK_VK_TO_VSC);
            var stringBuilder = new StringBuilder(2);

            var result = NativeMethods.ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                default:
                    {
                        character = stringBuilder[0].ToString();
                        break;
                    }
            }
            return character;
        }

    }
}
