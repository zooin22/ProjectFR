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

        var morrowProxy = new MissionClientProfile(
            name: "Morrow Proxy",
            faction: "Corporate Espionage",
            agenda: "Steal sealed research packages before the original owner notices.",
            riskNote: "Pays well, but leaves ugly traces when jobs go loud.");

        var northlineLegal = new MissionClientProfile(
            name: "Northline Legal",
            faction: "Legal Fixers",
            agenda: "Erase or bury documents that can become liabilities.",
            riskNote: "Low noise preferred. Failures spread quickly through compliance networks.");

        var helixOps = new MissionClientProfile(
            name: "Helix Ops",
            faction: "Security Contractors",
            agenda: "Map hostile systems and identify weak points before a full breach.",
            riskNote: "Professional and cautious, but they remember sloppy field work.");

        var emberCircuit = new MissionClientProfile(
            name: "Ember Circuit",
            faction: "Leak Brokers",
            agenda: "Move sensitive payloads through deniable operators.",
            riskNote: "High payout, high suspicion. Heat rises fast if you stumble.");

        var glassKey = new MissionClientProfile(
            name: "Glass Key Collective",
            faction: "Civic Leakers",
            agenda: "Pull hidden evidence into public reach.",
            riskNote: "They value clean retrieval over collateral damage.");

        _missionBoard.AddRange(new[]
        {
            new MissionData(
                id: "mission_extract_boss",
                title: "Archive Lift",
                client: morrowProxy,
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
                client: northlineLegal,
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
                client: helixOps,
                briefing: "Inspect the BuildCache folder, map the live defenses, and exfiltrate before security tightens.",
                objectiveType: MissionObjectiveType.Scan,
                targetPath: BattleConstants.RootBuildCachePath,
                turnLimit: 10,
                rewardCredits: 55,
                rewardReputation: 1,
                failurePenaltyCredits: 15,
                failureHeat: 1),
            new MissionData(
                id: "mission_extract_readme",
                title: "Mirror Snatch",
                client: glassKey,
                briefing: "Copy the root Readme intact. The client wants the document, not a cleanup operation.",
                objectiveType: MissionObjectiveType.Extract,
                targetPath: BattleConstants.RootReadmePath,
                turnLimit: 9,
                rewardCredits: 60,
                rewardReputation: 2,
                failurePenaltyCredits: 15,
                failureHeat: 1),
            new MissionData(
                id: "mission_delete_boss",
                title: "Burn Notice",
                client: emberCircuit,
                briefing: "Push through the archive and wipe Boss.zip before a broker handoff completes.",
                objectiveType: MissionObjectiveType.Delete,
                targetPath: BattleConstants.BossZipPath,
                turnLimit: 9,
                rewardCredits: 95,
                rewardReputation: 1,
                failurePenaltyCredits: 30,
                failureHeat: 3)
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
        }
        LastMissionResult = result;
    }
}
