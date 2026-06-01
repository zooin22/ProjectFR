namespace ProjectFR.Infiltration;

public sealed class SecurityBehaviorContext
{
    public string PrimaryPath { get; init; } = string.Empty;
    public SecurityAgent? Agent { get; init; }
    public IReadOnlyList<SecurityAgent> Agents { get; init; } = Array.Empty<SecurityAgent>();
    public string CurrentFolderPath { get; init; } = string.Empty;
    public string CursorPath { get; init; } = string.Empty;
    public string ObjectivePath { get; init; } = string.Empty;
    public bool IsObjectivePath { get; init; }
    public bool IsObjectiveRoute { get; init; }
    public bool AgentOnObjectiveRoute { get; init; }
    public bool DirectJump { get; init; }
    public int TraceAmount { get; init; }
    public string TraceReason { get; init; } = string.Empty;
    public SecurityAwarenessStage AwarenessStage { get; init; } = SecurityAwarenessStage.Suspicious;

    public required Action<int, string> AddTrace { get; init; }
    public required Action<string> AddLog { get; init; }
    public required Action<SecurityAgent, SecurityAwarenessStage> AlertAgent { get; init; }
    public required Action<string, int, string> MarkTrackedPath { get; init; }
    public required Action<string, int, string> ApplyForcedLock { get; init; }
    public required Action<string, int, string> ApplyScanPressure { get; init; }
}
