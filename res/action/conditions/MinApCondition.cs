namespace ProjectFR.Action.Conditions;

public class MinApCondition : IActionCondition
{
    public string ConditionId => "min_ap";
    public int RequiredAp { get; set; }

    public MinApCondition(int requiredAp = 1)
    {
        RequiredAp = requiredAp;
    }

    public bool Check(ActionContext context)
    {
        return context.Actor.CurrentAp >= RequiredAp;
    }
}
