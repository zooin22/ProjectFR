namespace ProjectFR.Infiltration;

public sealed class InfiltrationState
{
    public string CurrentFolderPath { get; set; } = "res://";
    public int TurnCount { get; set; }
    public int Trace { get; set; }
    public int MaxTrace { get; set; } = 100;
    public int OperatorMaxHp { get; set; } = InfiltrationTuning.OperatorMaxHp;
    public int OperatorHp { get; set; } = InfiltrationTuning.OperatorMaxHp;
    public bool IsOperatorAlive => OperatorHp > 0;
    public int LastTurnContactDamage { get; set; }
    public SecurityAwarenessStage AlertStage { get; set; } = SecurityAwarenessStage.Passive;
    public bool ExitUnlocked { get; set; }
    public RunStatus RunStatus { get; set; } = RunStatus.Active;
    public ObjectiveState ObjectiveState { get; set; } = ObjectiveState.Hidden;
    public CursorAgent CursorAgent { get; } = new();
    public HashSet<string> KnownNodePaths { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ExplorerWindowState> Windows { get; } = new();
    public List<ClipboardEntry> Clipboard { get; } = new();
    public List<ClipboardEntry> PouchCache { get; } = new();
    public HashSet<string> ExposedPouchPaths { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> PermissionOverrideTurns { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> TrackedPathTurns { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> ForcedLockTurns { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> ScanPressureTurns { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<FileOperation> ActiveOperations { get; } = new();
    public List<CommandQueueEntry> CommandQueue { get; } = new();
    public List<string> EventLog { get; } = new();

    public void TakeOperatorDamage(int amount)
    {
        OperatorHp = Math.Max(0, OperatorHp - amount);
        AddLog($"Operator took {amount} contact damage (HP: {OperatorHp}/{OperatorMaxHp})");
    }

    public void AddLog(string message)
    {
        EventLog.Add(message);
        if (EventLog.Count > 100)
        {
            EventLog.RemoveAt(0);
        }
    }
}
