namespace ProjectFR.Infiltration;

public enum RunStatus
{
    Active,
    ObjectiveCompleted,
    Escaped,
    Failed,
    TimedOut
}

public enum ObjectiveState
{
    Hidden,
    Revealed,
    InProgress,
    Completed
}
