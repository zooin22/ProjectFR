namespace ProjectFR.Infiltration;

public sealed class FileOperation
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public OperationType Type { get; }
    public string TargetNodePath { get; }
    public string? SecondaryTargetPath { get; }
    public float Progress { get; private set; }
    public int RequiredTicks { get; }
    public int ElapsedTicks { get; private set; }
    public OperationStatus Status { get; private set; } = OperationStatus.Queued;
    public bool CompletionHandled { get; private set; }
    public ExplorerNodeKind NodeKind { get; set; } = ExplorerNodeKind.File;
    public long NodeSize { get; set; }
    public List<string> CompletionNotes { get; } = new();

    public FileOperation(OperationType type, string targetNodePath, int requiredTicks = 1, string? secondaryTargetPath = null)
    {
        Type = type;
        TargetNodePath = targetNodePath;
        SecondaryTargetPath = secondaryTargetPath;
        RequiredTicks = Math.Max(1, requiredTicks);
    }

    public void Start()
    {
        if (Status == OperationStatus.Queued)
        {
            Status = OperationStatus.Running;
        }
    }

    public void Tick()
    {
        if (Status != OperationStatus.Running)
            return;

        ElapsedTicks = Math.Min(RequiredTicks, ElapsedTicks + 1);
        Progress = (float)ElapsedTicks / RequiredTicks;

        if (ElapsedTicks >= RequiredTicks)
        {
            Status = OperationStatus.Completed;
            Progress = 1f;
        }
    }

    public void Interrupt()
    {
        if (Status == OperationStatus.Running)
        {
            Status = OperationStatus.Interrupted;
        }
    }

    public void Fail()
    {
        Status = OperationStatus.Failed;
    }

    public void MarkCompletionHandled()
    {
        CompletionHandled = true;
    }
}
