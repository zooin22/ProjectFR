namespace ProjectFR.Action.Conditions;

public class TargetAliveCondition : IActionCondition
{
    public string ConditionId => "target_alive";

    public bool Check(ActionContext context)
    {
        return context.Target != null && context.Target.IsAlive;
    }
}
