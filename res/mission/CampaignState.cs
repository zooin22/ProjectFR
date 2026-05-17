using ProjectFR.Battle;

namespace ProjectFR.Mission;

public static class CampaignState
{
    private static readonly List<MissionData> _missionBoard = new();
    private static int _selectedMissionIndex;

    public static int Credits { get; private set; } = 100;
    public static int Reputation { get; private set; }
    public static int Heat { get; private set; }
    public static IReadOnlyList<MissionData> MissionBoard => _missionBoard;
    public static MissionData? CurrentMission { get; private set; }
    public static MissionResult? LastMissionResult { get; private set; }

    public static void EnsureInitialized()
    {
        if (_missionBoard.Count > 0)
            return;

        _missionBoard.AddRange(new[]
        {
            new MissionData(
                id: "mission_extract_boss",
                title: "Archive Lift",
                clientName: "Morrow Proxy",
                briefing: "Dive into the asset vault, copy the Boss.zip package, then clear the route before the trace closes.",
                objectiveType: MissionObjectiveType.Extract,
                targetPath: BattleConstants.BossZipPath,
                turnLimit: 10,
                rewardCredits: 90,
                rewardReputation: 2,
                failurePenaltyCredits: 25,
                failureHeat: 2),
            new MissionData(
                id: "mission_delete_readme",
                title: "Loose End Cleanup",
                clientName: "Northline Legal",
                briefing: "Erase the root Readme before it gets mirrored into a compliance snapshot.",
                objectiveType: MissionObjectiveType.Delete,
                targetPath: BattleConstants.RootReadmePath,
                turnLimit: 10,
                rewardCredits: 65,
                rewardReputation: 1,
                failurePenaltyCredits: 20,
                failureHeat: 1),
            new MissionData(
                id: "mission_scan_cache",
                title: "Cache Recon",
                clientName: "Helix Ops",
                briefing: "Inspect the BuildCache folder, map the live defenses, and exfiltrate before security tightens.",
                objectiveType: MissionObjectiveType.Scan,
                targetPath: BattleConstants.RootBuildCachePath,
                turnLimit: 10,
                rewardCredits: 55,
                rewardReputation: 1,
                failurePenaltyCredits: 15,
                failureHeat: 1)
        });

        _selectedMissionIndex = 0;
        CurrentMission = _missionBoard[0];
    }

    public static MissionData GetSelectedMission()
    {
        EnsureInitialized();
        return _missionBoard[_selectedMissionIndex];
    }

    public static MissionData SelectNextMission()
    {
        EnsureInitialized();
        _selectedMissionIndex = (_selectedMissionIndex + 1) % _missionBoard.Count;
        CurrentMission = _missionBoard[_selectedMissionIndex];
        return CurrentMission;
    }

    public static MissionData SelectPreviousMission()
    {
        EnsureInitialized();
        _selectedMissionIndex = (_selectedMissionIndex - 1 + _missionBoard.Count) % _missionBoard.Count;
        CurrentMission = _missionBoard[_selectedMissionIndex];
        return CurrentMission;
    }

    public static void BeginSelectedMission()
    {
        EnsureInitialized();
        CurrentMission = _missionBoard[_selectedMissionIndex];
    }

    public static void ApplyMissionResult(MissionResult result)
    {
        Credits = Math.Max(0, Credits + result.CreditsDelta);
        Reputation = Math.Max(-10, Reputation + result.ReputationDelta);
        Heat = Math.Max(0, Heat + result.HeatDelta);
        LastMissionResult = result;
    }
}
