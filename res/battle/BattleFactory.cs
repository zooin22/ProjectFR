using ProjectFR.Action.Implementations;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;

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

        return new BattleDungeon(root);
    }

    public static List<(ActorState Actor, NodeData NodeData)> CreateEncounter(FolderNode folder)
    {
        return folder.Children
            .Select(node => (CreateActorForNode(node), node))
            .ToList();
    }

    private static ActorState CreateActorForNode(NodeData node)
    {
        return node switch
        {
            SpecialFileNode => new ActorState(
                maxHp: BattleConstants.DefaultSpecialFileMaxHp,
                maxAp: BattleConstants.DefaultSpecialFileMaxAp,
                attackPower: BattleConstants.DefaultSpecialFileAttackPower,
                displayName: node.Name
            ),
            FolderNode => new ActorState(
                maxHp: BattleConstants.DefaultFolderMaxHp,
                maxAp: BattleConstants.DefaultFolderMaxAp,
                attackPower: BattleConstants.DefaultFolderAttackPower,
                displayName: node.Name
            ),
            _ => new ActorState(
                maxHp: BattleConstants.DefaultFileMaxHp,
                maxAp: BattleConstants.DefaultFileMaxAp,
                attackPower: BattleConstants.DefaultFileAttackPower,
                displayName: node.Name
            )
        };
    }
}
