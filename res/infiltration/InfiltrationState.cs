namespace ProjectFR.Infiltration;

public sealed class InfiltrationState
{
    public string CurrentFolderPath { get; set; } = "res://";
    public int TurnCount { get; set; }
    public int Trace { get; set; }
    public int MaxTrace { get; set; } = 100;
    public SecurityAwarenessStage AlertStage { get; set; } = SecurityAwarenessStage.Passive;
    public bool ExitUnlocked { get; set; }
    public RunStatus RunStatus { get; set; } = RunStatus.Active;
    public ObjectiveState ObjectiveState { get; set; } = ObjectiveState.Hidden;
    public CursorAgent CursorAgent { get; } = new();
    public HashSet<string> KnownNodePaths { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ExplorerWindowState> Windows { get; } = new();
    public List<ClipboardEntry> Clipboard { get; } = new();
    public List<FileOperation> ActiveOperations { get; } = new();
    public List<CommandQueueEntry> CommandQueue { get; } = new();
    public List<string> EventLog { get; } = new();

    public void AddLog(string message)
    {
        EventLog.Add(message);
        if (EventLog.Count > 100)
        {
            EventLog.RemoveAt(0);
        }
    }
}
