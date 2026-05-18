namespace ProjectFR.Infiltration;

public enum SecurityAgentType
{
    GuardScanner,
    AntivirusHeavy,
    IndexerScout,
    BackupRepairer,
    FirewallSentinel,
    AiMonitor
}

public enum SecurityAwarenessStage
{
    Passive,
    Suspicious,
    ActiveScan,
    Quarantine,
    Purge
}

public sealed class SecurityAgent
{
    public string Id { get; }
    public SecurityAgentType AgentType { get; }
    public string DisplayName { get; }
    public string CurrentNodePath { get; set; }
    public List<string> PatrolRoute { get; } = new();
    public SecurityAwarenessStage AwarenessStage { get; set; }
    public int DisabledTurns { get; set; }
    public bool IsAlerted { get; set; }
    public int PatrolIndex { get; set; }
    public int SightRange { get; set; } = 1;

    public SecurityAgent(
        SecurityAgentType agentType,
        string displayName,
        string currentNodePath,
        IEnumerable<string>? patrolRoute = null,
        string? id = null)
    {
        Id = id ?? Guid.NewGuid().ToString("N");
        AgentType = agentType;
        DisplayName = displayName;
        CurrentNodePath = currentNodePath;
        AwarenessStage = SecurityAwarenessStage.Passive;

        if (patrolRoute != null)
        {
            PatrolRoute.AddRange(patrolRoute);
        }
    }
}
