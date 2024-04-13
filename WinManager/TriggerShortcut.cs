namespace WinManager
{
    /// <summary>
    /// Triggering keys definition class
    /// </summary>
    public class TriggerShortcut
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsEnabled { get; set; }
        public ModifierKeyCodes Modifiers { get; set; }
        public int KeyCode { get; set; }
        public TriggerAction Action { get; set; }
        public KeyboardHook? Hook { get; set; }

        public enum TriggerAction
        {
            ShowApps,
            ShowWindows
        }

        public TriggerShortcut(string id, string text, ModifierKeyCodes modifiers, int keyCode, TriggerAction action)
        {
            Id = id;
            Text = text;
            Modifiers = modifiers;
            KeyCode = keyCode;
            Action = action;
        }
    }
}

