namespace ProjectFR.Mission;

public class MissionResult
{
    public MissionData Mission { get; }
    public bool Success { get; }
    public string Summary { get; }
    public int CreditsDelta { get; }
    public int ReputationDelta { get; }
    public int HeatDelta { get; }
    public bool ObjectiveCompleted { get; }
    public bool PlayerSurvived { get; }
    public bool DungeonCleared { get; }
    public int TurnsUsed { get; }

    public MissionResult(
        MissionData mission,
        bool success,
        string summary,
        int creditsDelta,
        int reputationDelta,
        int heatDelta,
        bool objectiveCompleted,
        bool playerSurvived,
        bool dungeonCleared,
        int turnsUsed)
    {
        Mission = mission;
        Success = success;
        Summary = summary;
        CreditsDelta = creditsDelta;
        ReputationDelta = reputationDelta;
        HeatDelta = heatDelta;
        ObjectiveCompleted = objectiveCompleted;
        PlayerSurvived = playerSurvived;
        DungeonCleared = dungeonCleared;
        TurnsUsed = turnsUsed;
    }
}
