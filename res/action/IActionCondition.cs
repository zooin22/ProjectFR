namespace ProjectFR.Action;

public interface IActionCondition
{
    string ConditionId { get; }
    bool Check(ActionContext context);
}
