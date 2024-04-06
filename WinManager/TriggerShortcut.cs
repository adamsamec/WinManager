namespace WinManager
{
    /// <summary>
    /// Triggering keys definition class
    /// </summary>
    public class TriggerShortcut
    {
        public ModifierKeyCodes Modifiers { get; set; }
        public int KeyCode { get; set; }
        public TriggerType Type { get; set; }

        public enum TriggerType
        {
            Apps,
            Windows
        }

        public TriggerShortcut(ModifierKeyCodes modifiers, int keyCode, TriggerType type)
        {
            Modifiers = modifiers;
            KeyCode = keyCode;
            Type = type;
        }
    }
}

