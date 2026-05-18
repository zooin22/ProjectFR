namespace ProjectFR.Infiltration;

public enum ExplorerWindowType
{
    Main,
    Clipboard,
    Temp,
    LogViewer,
    Backup,
    Archive
}

public sealed class ExplorerWindowState
{
    public string WindowId { get; set; } = Guid.NewGuid().ToString("N");
    public ExplorerWindowType WindowType { get; set; } = ExplorerWindowType.Main;
    public string Title { get; set; } = "Window";
    public string BoundPath { get; set; } = string.Empty;
    public bool IsOpen { get; set; } = true;
    public bool IsFocused { get; set; }
    public int SlotIndex { get; set; }
    public int TraceModifier { get; set; }
}
