namespace ProjectFR.Mission;

public class MissionData
{
    public string Id { get; }
    public string Title { get; }
    public string ClientName { get; }
    public string Briefing { get; }
    public MissionObjectiveType ObjectiveType { get; }
    public string TargetPath { get; }
    public int TurnLimit { get; }
    public int RewardCredits { get; }
    public int RewardReputation { get; }
    public int FailurePenaltyCredits { get; }
    public int FailureHeat { get; }

    public MissionData(
        string id,
        string title,
        string clientName,
        string briefing,
        MissionObjectiveType objectiveType,
        string targetPath,
        int turnLimit,
        int rewardCredits,
        int rewardReputation,
        int failurePenaltyCredits,
        int failureHeat)
    {
        Id = id;
        Title = title;
        ClientName = clientName;
        Briefing = briefing;
        ObjectiveType = objectiveType;
        TargetPath = targetPath;
        TurnLimit = turnLimit;
        RewardCredits = rewardCredits;
        RewardReputation = rewardReputation;
        FailurePenaltyCredits = failurePenaltyCredits;
        FailureHeat = failureHeat;
    }
}
