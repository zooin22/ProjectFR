namespace ProjectFR.Mission;

public class MissionProgress
{
    private bool _objectiveCompleted;

    public MissionData Mission { get; }
    public bool ObjectiveCompleted => _objectiveCompleted;

    public MissionProgress(MissionData mission)
    {
        Mission = mission;
    }

    public string? RegisterAction(string actionId, string? targetPath)
    {
        if (_objectiveCompleted || string.IsNullOrWhiteSpace(targetPath) || !string.Equals(targetPath, Mission.TargetPath, StringComparison.OrdinalIgnoreCase))
            return null;

        bool matched = Mission.ObjectiveType switch
        {
            MissionObjectiveType.Extract => actionId == "copy",
            MissionObjectiveType.Delete => actionId == "delete",
            MissionObjectiveType.Scan => actionId == "inspect",
            _ => false
        };

        if (!matched)
            return null;

        _objectiveCompleted = true;
        return $"Mission objective complete: {Mission.ObjectiveType} @ {Mission.TargetPath}";
    }

    public bool HasExceededTurnLimit(int turnCount)
    {
        return turnCount > Mission.TurnLimit;
    }

    public MissionResult Resolve(bool playerSurvived, bool dungeonCleared, int turnsUsed)
    {
        bool success = playerSurvived && dungeonCleared && _objectiveCompleted && !HasExceededTurnLimit(turnsUsed);
        string summary = success
            ? $"{Mission.Title} complete. Client package delivered cleanly."
            : BuildFailureSummary(playerSurvived, dungeonCleared, turnsUsed);

        return new MissionResult(
            mission: Mission,
            success: success,
            summary: summary,
            creditsDelta: success ? Mission.RewardCredits : -Mission.FailurePenaltyCredits,
            reputationDelta: success ? Mission.RewardReputation : -1,
            heatDelta: success ? 0 : Mission.FailureHeat,
            objectiveCompleted: _objectiveCompleted,
            playerSurvived: playerSurvived,
            dungeonCleared: dungeonCleared,
            turnsUsed: turnsUsed
        );
    }

    private string BuildFailureSummary(bool playerSurvived, bool dungeonCleared, int turnsUsed)
    {
        if (!playerSurvived)
            return $"{Mission.Title} failed. Operator trace collapsed before extraction.";

        if (HasExceededTurnLimit(turnsUsed))
            return $"{Mission.Title} failed. Trace level spiked past turn limit {Mission.TurnLimit}.";

        if (!_objectiveCompleted)
            return $"{Mission.Title} failed. Objective at {Mission.TargetPath} was not secured.";

        if (!dungeonCleared)
            return $"{Mission.Title} failed. Exit route stayed hot and the package could not be delivered.";

        return $"{Mission.Title} failed.";
    }
}
