using ProjectFR.Battle;

namespace ProjectFR.Mission;

public static class MissionBoardFactory
{
    public static IReadOnlyList<MissionData> CreateDefaultBoard()
    {
        var morrowProxy = new MissionClientProfile(
            name: "Morrow Proxy",
            factionId: FactionId.CorporateEspionage,
            agenda: "Steal sealed research packages before the original owner notices.",
            riskNote: "Pays well, but leaves ugly traces when jobs go loud.");

        var northlineLegal = new MissionClientProfile(
            name: "Northline Legal",
            factionId: FactionId.LegalFixers,
            agenda: "Erase or bury documents that can become liabilities.",
            riskNote: "Low noise preferred. Failures spread quickly through compliance networks.");

        var helixOps = new MissionClientProfile(
            name: "Helix Ops",
            factionId: FactionId.SecurityContractors,
            agenda: "Map hostile systems and identify weak points before a full breach.",
            riskNote: "Professional and cautious, but they remember sloppy field work.");

        var emberCircuit = new MissionClientProfile(
            name: "Ember Circuit",
            factionId: FactionId.LeakBrokers,
            agenda: "Move sensitive payloads through deniable operators.",
            riskNote: "High payout, high suspicion. Heat rises fast if you stumble.");

        var glassKey = new MissionClientProfile(
            name: "Glass Key Collective",
            factionId: FactionId.CivicLeakers,
            agenda: "Pull hidden evidence into public reach.",
            riskNote: "They value clean retrieval over collateral damage.");

        return new MissionData[]
        {
            new(
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
            new(
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
                failureHeat: 1,
                conflictGroup: "readme_conflict"),
            new(
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
            new(
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
                failureHeat: 1,
                conflictGroup: "readme_conflict"),
            new(
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
                failureHeat: 3,
                prerequisiteMissionId: "mission_extract_boss",
                requiredFactionReputation: 2),
            new(
                id: "mission_modify_syslog",
                title: "Audit Wash",
                client: northlineLegal,
                briefing: "Forge the system.log before an automated compliance sweep captures the incriminating entries.",
                objectiveType: MissionObjectiveType.Modify,
                targetPath: BattleConstants.SystemLogPath,
                turnLimit: 8,
                rewardCredits: 75,
                rewardReputation: 2,
                failurePenaltyCredits: 20,
                failureHeat: 2),
            new(
                id: "mission_escape_only",
                title: "Clean Exfil",
                client: helixOps,
                briefing: "No package retrieval — just map the system and extract without triggering an alert cascade.",
                objectiveType: MissionObjectiveType.Escape,
                targetPath: string.Empty,
                turnLimit: 7,
                rewardCredits: 50,
                rewardReputation: 1,
                failurePenaltyCredits: 10,
                failureHeat: 1),
        };
    }
}
