using ProjectFR.Action.Implementations;
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

    public static BattleDungeon CreateDefaultDungeon()
    {
        var root = new FolderNode(BattleConstants.DungeonRootName, BattleConstants.DungeonRootPath);
        var buildCache = new FolderNode(BattleConstants.RootBuildCacheName, BattleConstants.RootBuildCachePath);
        var assets = new FolderNode(BattleConstants.CacheAssetsName, BattleConstants.CacheAssetsPath);

        root.AddChild(new FileNode(BattleConstants.RootReadmeName, BattleConstants.RootReadmePath, BattleConstants.RootReadmeSize));
        root.AddChild(buildCache);

        buildCache.AddChild(new FileNode(BattleConstants.CacheTempName, BattleConstants.CacheTempPath, BattleConstants.CacheTempSize));
        buildCache.AddChild(assets);

        assets.AddChild(new SpecialFileNode(BattleConstants.BossZipName, BattleConstants.BossZipPath, BattleConstants.BossZipSize));

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
                eventSummary: "A compressed archive room protecting the boss payload.",
                rewardPreview: "Boss cleanup and dungeon clear",
                depth: 2
            )
        };

        return new BattleDungeon(root, metadata);
    }

    public static List<(ActorState Actor, NodeData NodeData)> CreateEncounter(FolderNode folder, CampaignModifiers modifiers)
    {
        return folder.Children
            .Select(node => (CreateActorForNode(node, modifiers), node))
            .ToList();
    }

    private static ActorState CreateActorForNode(NodeData node, CampaignModifiers modifiers)
    {
        return node switch
        {
            SpecialFileNode => new ActorState(
                maxHp: BattleConstants.DefaultSpecialFileMaxHp + modifiers.EnemyHpBonus,
                maxAp: BattleConstants.DefaultSpecialFileMaxAp + modifiers.EnemyApBonus,
                attackPower: BattleConstants.DefaultSpecialFileAttackPower + modifiers.EnemyAttackBonus,
                displayName: node.Name
            ),
            FolderNode => new ActorState(
                maxHp: BattleConstants.DefaultFolderMaxHp + modifiers.EnemyHpBonus,
                maxAp: BattleConstants.DefaultFolderMaxAp + modifiers.EnemyApBonus,
                attackPower: BattleConstants.DefaultFolderAttackPower + modifiers.EnemyAttackBonus,
                displayName: node.Name
            ),
            _ => new ActorState(
                maxHp: BattleConstants.DefaultFileMaxHp + modifiers.EnemyHpBonus,
                maxAp: BattleConstants.DefaultFileMaxAp + modifiers.EnemyApBonus,
                attackPower: BattleConstants.DefaultFileAttackPower + modifiers.EnemyAttackBonus,
                displayName: node.Name
            )
        };
    }
}
