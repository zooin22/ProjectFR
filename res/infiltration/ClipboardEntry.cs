namespace ProjectFR.Infiltration;

public sealed class ClipboardEntry
{
    public string NodePath { get; set; } = string.Empty;
    public ExplorerNodeKind NodeKind { get; set; } = ExplorerNodeKind.File;
    public bool IsGhosted { get; set; }
}
