namespace ProjectFR.Infiltration;

public sealed class CursorAgent
{
    public string CurrentNodePath { get; set; } = "res://";
    public int ActionPoints { get; set; } = 3;
    public int MaxActionPoints { get; set; } = 3;
    public int ClipboardCapacity { get; set; } = 1;
    public bool IsDetected { get; set; }

    public void RestoreActionPoints()
    {
        ActionPoints = MaxActionPoints;
    }
}
