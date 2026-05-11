namespace ProjectFR.Data;

public class ActorState
{
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int MaxAp { get; set; }
    public int CurrentAp { get; set; }
    public int AttackPower { get; set; }

    public ActorState(int maxHp = 20, int maxAp = 5, int attackPower = 3)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
        MaxAp = maxAp;
        CurrentAp = maxAp;
        AttackPower = attackPower;
    }

    public bool IsAlive => CurrentHp > 0;
    public bool HasAp => CurrentAp > 0;

    public void TakeDamage(int damage)
    {
        CurrentHp = System.Math.Max(0, CurrentHp - damage);
    }

    public void Heal(int amount)
    {
        CurrentHp = System.Math.Min(MaxHp, CurrentHp + amount);
    }

    public void ConsumeAp(int amount)
    {
        CurrentAp = System.Math.Max(0, CurrentAp - amount);
    }

    public void RestoreAp(int amount)
    {
        CurrentAp = System.Math.Min(MaxAp, CurrentAp + amount);
    }

    public void RestoreAllAp()
    {
        CurrentAp = MaxAp;
    }

    public ActorState Clone()
    {
        return new ActorState(MaxHp, MaxAp, AttackPower)
        {
            CurrentHp = CurrentHp,
            CurrentAp = CurrentAp
        };
    }
}
