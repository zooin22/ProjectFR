namespace ProjectFR.Mission;

public class CampaignModifiers
{
    public int HeatTurnPenalty { get; }
    public int EnemyAttackBonus { get; }
    public int EnemyApBonus { get; }
    public int EnemyHpBonus { get; }
    public string Summary { get; }

    public CampaignModifiers(int heatTurnPenalty, int enemyAttackBonus, int enemyApBonus, int enemyHpBonus, string summary)
    {
        HeatTurnPenalty = heatTurnPenalty;
        EnemyAttackBonus = enemyAttackBonus;
        EnemyApBonus = enemyApBonus;
        EnemyHpBonus = enemyHpBonus;
        Summary = summary;
    }
}
