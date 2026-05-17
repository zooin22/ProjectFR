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

    public static List<(ActorState Actor, NodeData NodeData)> CreateDefaultEnemies()
    {
        return new()
        {
            (
                new ActorState(
                    maxHp: BattleConstants.DefaultEnemy1MaxHp,
                    maxAp: BattleConstants.DefaultEnemy1MaxAp,
                    attackPower: BattleConstants.DefaultEnemy1AttackPower,
                    displayName: BattleConstants.DefaultEnemy1Name
                ),
                new FileNode(BattleConstants.DefaultEnemy1Name, BattleConstants.DefaultEnemy1Path, BattleConstants.DefaultEnemy1Size)
            ),
            (
                new ActorState(
                    maxHp: BattleConstants.DefaultEnemy2MaxHp,
                    maxAp: BattleConstants.DefaultEnemy2MaxAp,
                    attackPower: BattleConstants.DefaultEnemy2AttackPower,
                    displayName: BattleConstants.DefaultEnemy2Name
                ),
                new FolderNode(BattleConstants.DefaultEnemy2Name, BattleConstants.DefaultEnemy2Path)
            ),
            (
                new ActorState(
                    maxHp: BattleConstants.DefaultEnemy3MaxHp,
                    maxAp: BattleConstants.DefaultEnemy3MaxAp,
                    attackPower: BattleConstants.DefaultEnemy3AttackPower,
                    displayName: BattleConstants.DefaultEnemy3Name
                ),
                new SpecialFileNode(BattleConstants.DefaultEnemy3Name, BattleConstants.DefaultEnemy3Path, BattleConstants.DefaultEnemy3Size)
            )
        };
    }
}
