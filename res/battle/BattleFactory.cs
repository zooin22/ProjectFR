using ProjectFR.Data;
using ProjectFR.Data.Nodes;
using ProjectFR.Mission;

namespace ProjectFR.Battle;

public static class BattleFactory
{
    public static ActorState CreateDefaultPlayer()
    {
        return new ActorState(
            maxHp: BattleConstants.DefaultPlayerMaxHp,
            maxAp: BattleConstants.DefaultPlayerMaxAp,
            attackPower: BattleConstants.DefaultPlayerAttackPower,
            displayName: BattleConstants.PlayerDisplayName
        );
    }

    public static BattleDungeon CreateDefaultDungeon(MissionData? mission = null)
    {
        if (mission != null)
        {
            return mission.ObjectiveType switch
            {
                MissionObjectiveType.Delete or MissionObjectiveType.Modify => CreateCompactDungeon(mission),
                MissionObjectiveType.Scan or MissionObjectiveType.Escape => CreateDeepDungeon(mission),
                _ => CreateStandardDungeon(mission)
            };
        }

        return CreateStandardDungeon(null);
    }

    private static BattleDungeon CreateStandardDungeon(MissionData? mission)
    {
        var root = new FolderNode(BattleConstants.DungeonRootName, BattleConstants.DungeonRootPath, CreateFolderProfile("Root Directory", NodeThreatLevel.Low, 8, 2, 1));
        var buildCache = new FolderNode(BattleConstants.RootBuildCacheName, BattleConstants.RootBuildCachePath, CreateFolderProfile("Guard Folder", NodeThreatLevel.Medium, 9, 2, 2));
        var assets = new FolderNode(BattleConstants.CacheAssetsName, BattleConstants.CacheAssetsPath, CreateFolderProfile("Vault Folder", NodeThreatLevel.High, 11, 3, 3));
        var bossArchive = new ArchiveNode(
            BattleConstants.BossZipName,
            BattleConstants.BossZipPath,
            BattleConstants.BossZipSize,
            new NodeCombatProfile("Archive Boss", "Critical", NodeThreatLevel.Critical, 12, 3, 4, isBoss: true, revealsChildrenOnOpen: true, revealSummary: "Archive broken open; hostile payloads flooded the explorer."));

        root.AddChild(new FileNode(BattleConstants.RootReadmeName, BattleConstants.RootReadmePath, BattleConstants.RootReadmeSize, CreateFileProfile("Text File", NodeThreatLevel.Low, 7, 2, 1)));
        root.AddChild(buildCache);

        buildCache.AddChild(new FileNode(BattleConstants.CacheTempName, BattleConstants.CacheTempPath, BattleConstants.CacheTempSize, CreateFileProfile("Temp File", NodeThreatLevel.Medium, 8, 2, 2)));
        buildCache.AddChild(new FileNode(BattleConstants.SystemLogName, BattleConstants.SystemLogPath, BattleConstants.SystemLogSize, CreateFileProfile("Log File", NodeThreatLevel.Medium, 7, 2, 2)));
        buildCache.AddChild(assets);

        assets.AddChild(bossArchive);
        bossArchive.AddChild(new FileNode("payload.exe", $"{BattleConstants.BossZipPath}/payload.exe", 9, CreateFileProfile("Executable", NodeThreatLevel.Critical, 9, 3, 4, isBoss: true)));
        bossArchive.AddChild(new FileNode("hook.dll", $"{BattleConstants.BossZipPath}/hook.dll", 6, CreateFileProfile("Library", NodeThreatLevel.High, 8, 2, 3)));
        bossArchive.AddChild(new FileNode("trace.log", $"{BattleConstants.BossZipPath}/trace.log", 4, CreateFileProfile("Log File", NodeThreatLevel.Medium, 6, 2, 2)));

        var metadata = new Dictionary<string, DungeonFolderMetadata>
        {
            [BattleConstants.DungeonRootPath] = new(
                themeName: "Root Directory",
                eventSummary: "A noisy root folder with mixed junk and one suspicious cache directory.",
                rewardPreview: "Clipboard setup opportunity",
                depth: 0
            ),
            [BattleConstants.RootBuildCachePath] = new(
                themeName: "Build Cache",
                eventSummary: "Stale cache data leaks AP if left alive for too long.",
                rewardPreview: "Safe AP reset before deeper dive",
                depth: 1
            ),
            [BattleConstants.CacheAssetsPath] = new(
                themeName: "Packed Assets",
                eventSummary: "A volatile vault hiding a configurable payload node.",
                rewardPreview: "Clear the hostile object and collapse the route",
                depth: 2
            )
        };

        var dungeon = new BattleDungeon(root, metadata);
        if (mission != null)
        {
            ApplyMissionVariants(dungeon, mission, buildCache);
        }

        return dungeon;
    }

    // Delete/Modify missions: shallower BuildCache (no Assets subfolder), Firewall Sentinel node adjacent to the objective.
    private static BattleDungeon CreateCompactDungeon(MissionData mission)
    {
        var root = new FolderNode(BattleConstants.DungeonRootName, BattleConstants.DungeonRootPath, CreateFolderProfile("Root Directory", NodeThreatLevel.Low, 8, 2, 1));
        var buildCache = new FolderNode(BattleConstants.RootBuildCacheName, BattleConstants.RootBuildCachePath, CreateFolderProfile("Guard Folder", NodeThreatLevel.High, 10, 2, 2));

        root.AddChild(new FileNode(BattleConstants.RootReadmeName, BattleConstants.RootReadmePath, BattleConstants.RootReadmeSize, CreateFileProfile("Text File", NodeThreatLevel.Low, 7, 2, 1)));
        root.AddChild(buildCache);

        buildCache.AddChild(new FileNode(BattleConstants.CacheTempName, BattleConstants.CacheTempPath, BattleConstants.CacheTempSize, CreateFileProfile("Temp File", NodeThreatLevel.Medium, 8, 2, 2)));
        buildCache.AddChild(new FileNode(BattleConstants.SystemLogName, BattleConstants.SystemLogPath, BattleConstants.SystemLogSize, CreateFileProfile("Log File", NodeThreatLevel.Medium, 7, 2, 2)));

        // Firewall Sentinel stands guard adjacent to the objective path inside the cache.
        var sentinelPath = $"{BattleConstants.RootBuildCachePath}/firewall_sentinel.sys";
        buildCache.AddChild(new FileNode("firewall_sentinel.sys", sentinelPath, 2,
            new NodeCombatProfile("Firewall Sentinel", "CRITICAL", NodeThreatLevel.Critical, 13, 3, 4, isBoss: false)));

        var metadata = new Dictionary<string, DungeonFolderMetadata>
        {
            [BattleConstants.DungeonRootPath] = new(
                themeName: "Root Directory",
                eventSummary: "Compact layout — the objective is close, but a sentinel guards the cache.",
                rewardPreview: "Neutralize the sentinel to clear the path",
                depth: 0
            ),
            [BattleConstants.RootBuildCachePath] = new(
                themeName: "Build Cache (Locked)",
                eventSummary: "Firewall Sentinel deployed at objective depth. Eliminate before proceeding.",
                rewardPreview: "Sentinel clear opens the objective node",
                depth: 1
            )
        };

        var dungeon = new BattleDungeon(root, metadata);
        ApplyMissionVariants(dungeon, mission, buildCache);
        return dungeon;
    }

    // Scan/Escape missions: standard structure plus two extra subfolders for wider traversal.
    private static BattleDungeon CreateDeepDungeon(MissionData mission)
    {
        var root = new FolderNode(BattleConstants.DungeonRootName, BattleConstants.DungeonRootPath, CreateFolderProfile("Root Directory", NodeThreatLevel.Low, 8, 2, 1));
        var buildCache = new FolderNode(BattleConstants.RootBuildCacheName, BattleConstants.RootBuildCachePath, CreateFolderProfile("Guard Folder", NodeThreatLevel.Medium, 9, 2, 2));
        var assets = new FolderNode(BattleConstants.CacheAssetsName, BattleConstants.CacheAssetsPath, CreateFolderProfile("Vault Folder", NodeThreatLevel.High, 11, 3, 3));
        var bossArchive = new ArchiveNode(
            BattleConstants.BossZipName,
            BattleConstants.BossZipPath,
            BattleConstants.BossZipSize,
            new NodeCombatProfile("Archive Boss", "Critical", NodeThreatLevel.Critical, 12, 3, 4, isBoss: true, revealsChildrenOnOpen: true, revealSummary: "Archive broken open; hostile payloads flooded the explorer."));

        // Extra subfolder 1: deep store for scan targets.
        var deepStorePath = $"{BattleConstants.RootBuildCachePath}/DeepStore";
        var deepStore = new FolderNode("DeepStore", deepStorePath, CreateFolderProfile("Deep Store", NodeThreatLevel.High, 10, 2, 3));
        deepStore.AddChild(new FileNode("snapshot.bin", $"{deepStorePath}/snapshot.bin", 7, CreateFileProfile("Binary Snapshot", NodeThreatLevel.High, 9, 2, 3)));
        deepStore.AddChild(new FileNode("manifest.xml", $"{deepStorePath}/manifest.xml", 3, CreateFileProfile("Manifest", NodeThreatLevel.Medium, 7, 2, 2)));

        // Extra subfolder 2: escape route staging area.
        var stagingPath = $"{BattleConstants.RootBuildCachePath}/Staging";
        var staging = new FolderNode("Staging", stagingPath, CreateFolderProfile("Staging Area", NodeThreatLevel.Medium, 9, 2, 2));
        staging.AddChild(new FileNode("exit_vector.cfg", $"{stagingPath}/exit_vector.cfg", 4, CreateFileProfile("Config File", NodeThreatLevel.Medium, 7, 2, 2)));
        staging.AddChild(new FileNode("decoy_stub.dll", $"{stagingPath}/decoy_stub.dll", 5, CreateFileProfile("Library", NodeThreatLevel.Medium, 8, 2, 2)));

        root.AddChild(new FileNode(BattleConstants.RootReadmeName, BattleConstants.RootReadmePath, BattleConstants.RootReadmeSize, CreateFileProfile("Text File", NodeThreatLevel.Low, 7, 2, 1)));
        root.AddChild(buildCache);

        buildCache.AddChild(new FileNode(BattleConstants.CacheTempName, BattleConstants.CacheTempPath, BattleConstants.CacheTempSize, CreateFileProfile("Temp File", NodeThreatLevel.Medium, 8, 2, 2)));
        buildCache.AddChild(new FileNode(BattleConstants.SystemLogName, BattleConstants.SystemLogPath, BattleConstants.SystemLogSize, CreateFileProfile("Log File", NodeThreatLevel.Medium, 7, 2, 2)));
        buildCache.AddChild(assets);
        buildCache.AddChild(deepStore);
        buildCache.AddChild(staging);

        assets.AddChild(bossArchive);
        bossArchive.AddChild(new FileNode("payload.exe", $"{BattleConstants.BossZipPath}/payload.exe", 9, CreateFileProfile("Executable", NodeThreatLevel.Critical, 9, 3, 4, isBoss: true)));
        bossArchive.AddChild(new FileNode("hook.dll", $"{BattleConstants.BossZipPath}/hook.dll", 6, CreateFileProfile("Library", NodeThreatLevel.High, 8, 2, 3)));
        bossArchive.AddChild(new FileNode("trace.log", $"{BattleConstants.BossZipPath}/trace.log", 4, CreateFileProfile("Log File", NodeThreatLevel.Medium, 6, 2, 2)));

        var metadata = new Dictionary<string, DungeonFolderMetadata>
        {
            [BattleConstants.DungeonRootPath] = new(
                themeName: "Root Directory",
                eventSummary: "Extended layout — two extra folders deepen the traversal surface.",
                rewardPreview: "Clipboard setup opportunity",
                depth: 0
            ),
            [BattleConstants.RootBuildCachePath] = new(
                themeName: "Build Cache",
                eventSummary: "Stale cache data with additional deep storage and staging branches.",
                rewardPreview: "Safe AP reset before deeper dive",
                depth: 1
            ),
            [BattleConstants.CacheAssetsPath] = new(
                themeName: "Packed Assets",
                eventSummary: "A volatile vault hiding a configurable payload node.",
                rewardPreview: "Clear the hostile object and collapse the route",
                depth: 2
            ),
            [deepStorePath] = new(
                themeName: "Deep Store",
                eventSummary: "Archival node cache — rich scan targets but heavily monitored.",
                rewardPreview: "Scan yields high-value intelligence",
                depth: 2
            ),
            [stagingPath] = new(
                themeName: "Staging Area",
                eventSummary: "Temporary staging zone used for payload handoffs and escape vectors.",
                rewardPreview: "Escape route unlock on clear",
                depth: 2
            )
        };

        var dungeon = new BattleDungeon(root, metadata);
        ApplyMissionVariants(dungeon, mission, buildCache);
        return dungeon;
    }

    private static void ApplyMissionVariants(BattleDungeon dungeon, MissionData mission, FolderNode buildCache)
    {
        switch (mission.Id)
        {
            case "mission_delete_readme":
            case "mission_extract_readme":
                BoostNodeProfile(dungeon.Root, BattleConstants.RootReadmePath, NodeThreatLevel.High, hpBonus: 3, apBonus: 1);
                break;

            case "mission_modify_syslog":
                BoostNodeProfile(dungeon.Root, BattleConstants.SystemLogPath, NodeThreatLevel.High, hpBonus: 2, apBonus: 1);
                buildCache.AddChild(new FileNode("audit_snapshot.dat", $"{BattleConstants.RootBuildCachePath}/audit_snapshot.dat", 5,
                    CreateFileProfile("Audit File", NodeThreatLevel.Medium, 6, 2, 2)));
                break;

            case "mission_extract_boss":
            case "mission_delete_boss":
                BoostNodeProfile(dungeon.Root, BattleConstants.BossZipPath, NodeThreatLevel.Critical, hpBonus: 4, apBonus: 1);
                break;

            case "mission_scan_cache":
                buildCache.AddChild(new FileNode("index.db", $"{BattleConstants.RootBuildCachePath}/index.db", 8,
                    CreateFileProfile("Index File", NodeThreatLevel.Medium, 7, 2, 2)));
                buildCache.AddChild(new FileNode("scan_queue.tmp", $"{BattleConstants.RootBuildCachePath}/scan_queue.tmp", 3,
                    CreateFileProfile("Queue File", NodeThreatLevel.Low, 5, 2, 1)));
                break;

            case "mission_escape_only":
                ReduceAllNodeProfiles(dungeon.Root, hpReduction: 2);
                break;
        }
    }

    private static void BoostNodeProfile(ContainerNode container, string targetPath, NodeThreatLevel newThreat, int hpBonus, int apBonus)
    {
        foreach (var node in EnumerateAll(container))
        {
            if (!string.Equals(node.Path, targetPath, StringComparison.OrdinalIgnoreCase))
                continue;

            var p = node.CombatProfile;
            node.CombatProfile = new NodeCombatProfile(
                p.TypeName, newThreat.ToString().ToUpperInvariant(), newThreat,
                p.BaseMaxHp + hpBonus, p.BaseMaxAp + apBonus, p.BaseAttackPower + 1,
                p.IsBoss, p.RevealsChildrenOnOpen, p.RevealSummary);
            return;
        }
    }

    private static void ReduceAllNodeProfiles(ContainerNode container, int hpReduction)
    {
        foreach (var node in EnumerateAll(container))
        {
            var p = node.CombatProfile;
            node.CombatProfile = new NodeCombatProfile(
                p.TypeName, p.ThreatLabel, p.ThreatLevel,
                Math.Max(1, p.BaseMaxHp - hpReduction), p.BaseMaxAp, p.BaseAttackPower,
                p.IsBoss, p.RevealsChildrenOnOpen, p.RevealSummary);
        }
    }

    private static IEnumerable<NodeData> EnumerateAll(ContainerNode container)
    {
        foreach (var child in container.Children)
        {
            yield return child;
            if (child is ContainerNode sub)
            {
                foreach (var nested in EnumerateAll(sub))
                    yield return nested;
            }
        }
    }

    public static List<(ActorState Actor, NodeData NodeData)> CreateEncounter(ContainerNode container, CampaignModifiers modifiers, Func<NodeData, bool>? include = null)
    {
        return container.Children
            .Where(node => include?.Invoke(node) ?? true)
            .Select(node => (CreateActorForNode(node, modifiers), node))
            .ToList();
    }

    public static ActorState CreateActorForNode(NodeData node, CampaignModifiers modifiers)
    {
        var profile = node.CombatProfile;
        return new ActorState(
            maxHp: profile.BaseMaxHp + modifiers.EnemyHpBonus,
            maxAp: profile.BaseMaxAp + modifiers.EnemyApBonus,
            attackPower: profile.BaseAttackPower + modifiers.EnemyAttackBonus,
            displayName: node.Name
        );
    }

    private static NodeCombatProfile CreateFileProfile(string typeName, NodeThreatLevel threatLevel, int hp, int ap, int attackPower, bool isBoss = false)
    {
        return new NodeCombatProfile(typeName, threatLevel.ToString().ToUpperInvariant(), threatLevel, hp, ap, attackPower, isBoss: isBoss);
    }

    private static NodeCombatProfile CreateFolderProfile(string typeName, NodeThreatLevel threatLevel, int hp, int ap, int attackPower)
    {
        return new NodeCombatProfile(typeName, threatLevel.ToString().ToUpperInvariant(), threatLevel, hp, ap, attackPower, revealsChildrenOnOpen: true, revealSummary: "Container opened; hidden children spilled into view.");
    }
}
