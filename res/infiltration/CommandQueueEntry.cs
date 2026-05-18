namespace ProjectFR.Infiltration;

public sealed class CommandQueueEntry
{
    public int Order { get; set; }
    public OperationType OperationType { get; set; }
    public string PrimaryTargetPath { get; set; } = string.Empty;
    public string? SecondaryTargetPath { get; set; }
    public string Summary { get; set; } = string.Empty;
}
