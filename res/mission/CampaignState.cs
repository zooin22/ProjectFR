namespace ProjectFR.Mission;

public static class CampaignState
{
    private static readonly List<MissionData> _missionBoard = new();
    private static readonly Dictionary<FactionId, int> _factionReputation = new();
    private static readonly HashSet<string> _completedMissionIds = new();
    private static int _selectedMissionIndex;

    public static int Credits { get; private set; } = 100;
    public static int Reputation { get; private set; }
    public static int Heat { get; private set; }
    public static IReadOnlyList<MissionData> MissionBoard => _missionBoard;
    public static MissionData? CurrentMission { get; private set; }
    public static MissionResult? LastMissionResult { get; private set; }

    public static int GetFactionReputation(FactionId factionId) =>
        _factionReputation.GetValueOrDefault(factionId, 0);

    public static bool IsMissionAvailable(MissionData mission) =>
        mission.PrerequisiteMissionId == null || _completedMissionIds.Contains(mission.PrerequisiteMissionId);

    private static List<MissionData> GetAvailableMissions() =>
        _missionBoard.Where(IsMissionAvailable).ToList();

    public static void EnsureInitialized()
    {
        if (_missionBoard.Count > 0)
            return;

        _missionBoard.AddRange(MissionBoardFactory.CreateDefaultBoard());
        _selectedMissionIndex = 0;
        CurrentMission = GetAvailableMissions()[0];
    }

    public static MissionData GetSelectedMission()
    {
        EnsureInitialized();
        var available = GetAvailableMissions();
        _selectedMissionIndex = Math.Clamp(_selectedMissionIndex, 0, available.Count - 1);
        return available[_selectedMissionIndex];
    }

    public static MissionData SelectNextMission()
    {
        EnsureInitialized();
        var available = GetAvailableMissions();
        _selectedMissionIndex = (_selectedMissionIndex + 1) % available.Count;
        CurrentMission = available[_selectedMissionIndex];
        return CurrentMission;
    }

    public static MissionData SelectPreviousMission()
    {
        EnsureInitialized();
        var available = GetAvailableMissions();
        _selectedMissionIndex = (_selectedMissionIndex - 1 + available.Count) % available.Count;
        CurrentMission = available[_selectedMissionIndex];
        return CurrentMission;
    }

    public static void BeginSelectedMission()
    {
        CurrentMission = GetSelectedMission();
    }

    public static CampaignModifiers GetModifiers()
    {
        return Heat switch
        {
            >= 6 => new CampaignModifiers(heatTurnPenalty: 2, enemyAttackBonus: 2, enemyApBonus: 1, enemyHpBonus: 3, summary: "TRACE CRITICAL · security actively hardens the node"),
            >= 3 => new CampaignModifiers(heatTurnPenalty: 1, enemyAttackBonus: 1, enemyApBonus: 0, enemyHpBonus: 1, summary: "TRACE ELEVATED · patrols are alert"),
            _ => new CampaignModifiers(heatTurnPenalty: 0, enemyAttackBonus: 0, enemyApBonus: 0, enemyHpBonus: 0, summary: "TRACE LOW · standard intrusion posture")
        };
    }

    public static void ApplyMissionResult(MissionResult result)
    {
        Credits = Math.Max(0, Credits + result.CreditsDelta);
        Reputation = Math.Max(-10, Reputation + result.ReputationDelta);
        Heat = Math.Max(0, Heat + result.HeatDelta);
        if (result.Success)
        {
            Heat = Math.Max(0, Heat - 1);
            _completedMissionIds.Add(result.Mission.Id);
        }

        var faction = result.Mission.Client.FactionId;
        _factionReputation[faction] = Math.Max(-10, GetFactionReputation(faction) + result.ReputationDelta);

        LastMissionResult = result;
    }
}
