using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class InspectAction : IAction
{
    public string ActionId => "inspect";
    public string DisplayName => "Inspect (Alt+Enter)";
    public int ApCost => ActionConstants.InspectActionApCost;
    public TargetType Scope => TargetType.Single;
    public List<IActionCondition> Conditions { get; }

    public InspectAction()
    {
        Conditions = new()
        {
            new TargetAliveCondition()
        };
    }

    public bool CanExecute(ActionContext context)
    {
        return Conditions.All(c => c.Check(context));
    }

    public ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot inspect target");

        var result = new ActionResult(true, $"Inspected {context.TargetNode?.Name}");
        result.Data["target_hp"] = context.Target?.CurrentHp ?? 0;
        result.Data["target_max_hp"] = context.Target?.MaxHp ?? 0;

        return result;
    }
}
