using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using System.Windows.Input;

namespace WinManager
{
    /// <summary>
    /// Utility class
    /// </summary>
    public static class Utils
    {
        public static void SetWindowToolStyle(IntPtr handle)
        {
            var extendedStyle = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(handle, NativeMethods.GWL_EXSTYLE, extendedStyle |
            NativeMethods.WS_EX_TOOLWINDOW);
        }

        [STAThread]
        public static string? GetClipboardText()
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                var clipboardText = Clipboard.GetText(TextDataFormat.Text);
                return clipboardText;
            }
            return null;
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

        /// <summary>
        /// Returns the text selected in the focused control of the foreground window.
        /// If no text is selected, returns the word at the text cursor (caret) position.
        /// Returns null if the text cannot be determined.
        /// </summary>
        public static string? GetSelectedTextOrWordAtCursor()
        {
            var foregroundHandle = NativeMethods.GetForegroundWindow();
            if (foregroundHandle == IntPtr.Zero)
            {
                return null;
            }

            // UI Automation clients should not call UI Automation from a UI thread, because it can hang
            // or deadlock, so do the work on a background thread and give up if the target app is unresponsive
            var task = Task.Run(() => GetSelectedTextOrWordAtCursorCore(foregroundHandle));
            try
            {
                return task.Wait(SelectedTextTimeoutMs) ? task.Result : null;
            }
            catch (AggregateException)
            {
                return null;
            }
        }

        private const int SelectedTextTimeoutMs = 1500;
        private const int MaxSelectedTextLength = 100_000;
        private const int MaxWordSearchLength = 256;
        private const int MaxTextPatternSearchDepth = 10;
        private const int MaxTreeDepth = 100;

        private static string? GetSelectedTextOrWordAtCursorCore(IntPtr foregroundHandle)
        {
            try
            {
                var focusedElement = AutomationElement.FocusedElement;
                if (focusedElement == null || !BelongsToWindow(focusedElement, foregroundHandle))
                {
                    return null;
                }

                var textPattern = FindTextPattern(focusedElement);
                if (textPattern == null)
                {
                    return null;
                }

                var selectionRanges = textPattern.GetSelection();
                if (selectionRanges.Length == 0)
                {
                    return null;
                }

                var selectedText = string.Join(Environment.NewLine, selectionRanges
                    .Select(range => range.GetText(MaxSelectedTextLength))
                    .Where(text => !string.IsNullOrWhiteSpace(text)))
                    .Trim();
                if (selectedText.Length > 0)
                {
                    return selectedText;
                }

                // No selection, so the first range is the degenerate range at the caret position
                var word = GetWordAtCaret(selectionRanges[0]);
                return string.IsNullOrEmpty(word) ? null : word;
            }
            catch (Exception)
            {
                // UI Automation providers may throw a wide range of exceptions, and the text is optional anyway
                return null;
            }
        }

        /// <summary>
        /// Determines whether the element belongs to the given top-level window. Checking the process alone
        /// is not enough, because e.g. UWP apps are hosted in a window owned by a different process.
        /// </summary>
        private static bool BelongsToWindow(AutomationElement element, IntPtr windowHandle)
        {
            NativeMethods.GetWindowThreadProcessId(windowHandle, out uint windowProcessId);
            if (element.Current.ProcessId == windowProcessId)
            {
                return true;
            }

            var windowElement = AutomationElement.FromHandle(windowHandle);
            var walker = TreeWalker.RawViewWalker;
            var current = element;
            for (var depth = 0; current != null && depth < MaxTreeDepth; depth++)
            {
                if (Automation.Compare(current, windowElement))
                {
                    return true;
                }
                current = walker.GetParent(current);
            }
            return false;
        }

        /// <summary>
        /// Returns the text pattern of the element or of its nearest ancestor supporting it, because the focus
        /// is sometimes on a child element (e.g. a link or a paragraph) of the actual text control or document.
        /// </summary>
        private static TextPattern? FindTextPattern(AutomationElement element)
        {
            var walker = TreeWalker.RawViewWalker;
            var processId = element.Current.ProcessId;
            AutomationElement? current = element;
            for (var depth = 0; current != null && depth < MaxTextPatternSearchDepth; depth++)
            {
                if (current.Current.ProcessId != processId)
                {
                    break;
                }
                if (current.TryGetCurrentPattern(TextPattern.Pattern, out object patternObj))
                {
                    return (TextPattern)patternObj;
                }
                current = walker.GetParent(current);
            }
            return null;
        }

        /// <summary>
        /// Returns the word at or immediately before the caret. Word boundaries are determined from the text
        /// around the caret instead of the provider's word unit, whose behavior differs a lot between apps
        /// (e.g. including trailing whitespace or punctuation, or not being supported at all).
        /// </summary>
        private static string? GetWordAtCaret(TextPatternRange caretRange)
        {
            var caret = caretRange.Clone();
            caret.MoveEndpointByRange(TextPatternRangeEndpoint.End, caret, TextPatternRangeEndpoint.Start);

            var beforeRange = caret.Clone();
            var movedBefore = beforeRange.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Character, -MaxWordSearchLength);
            var afterRange = caret.Clone();
            var movedAfter = afterRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, MaxWordSearchLength);

            if (movedBefore != 0 || movedAfter != 0)
            {
                var beforeText = beforeRange.GetText(-1);
                var afterText = afterRange.GetText(-1);
                return ExtractWordAt(beforeText + afterText, beforeText.Length);
            }

            // Fall back to the provider's word unit if moving by characters is not supported
            var wordRange = caret.Clone();
            wordRange.ExpandToEnclosingUnit(TextUnit.Word);
            var wordText = wordRange.GetText(MaxWordSearchLength);
            var firstWordCharIndex = Enumerable.Range(0, wordText.Length).FirstOrDefault(i => IsWordChar(wordText[i]), -1);
            return firstWordCharIndex < 0 ? null : ExtractWordAt(wordText, firstWordCharIndex);
        }

        private static string ExtractWordAt(string text, int index)
        {
            var start = Math.Clamp(index, 0, text.Length);
            var end = start;
            while (start > 0 && IsWordChar(text[start - 1]))
            {
                start--;
            }
            while (end < text.Length && IsWordChar(text[end]))
            {
                end++;
            }
            return text[start..end].Trim(WordConnectors);
        }

        // Characters which are part of a word only when surrounded by other word characters, e.g. "don't" or "well-known"
        private static readonly char[] WordConnectors = ['\'', '\u2019', '-'];

        private static bool IsWordChar(char c)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || WordConnectors.Contains(c))
            {
                return true;
            }
            var category = char.GetUnicodeCategory(c);
            return category == System.Globalization.UnicodeCategory.NonSpacingMark ||
                category == System.Globalization.UnicodeCategory.SpacingCombiningMark ||
                category == System.Globalization.UnicodeCategory.EnclosingMark;
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
