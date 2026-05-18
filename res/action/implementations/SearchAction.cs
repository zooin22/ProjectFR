using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class SearchAction : IAction
{
    public string ActionId => "search";
    public string DisplayName => "Search";
    public int ApCost => 1;
    public TargetType Scope => TargetType.Single;
    public List<IActionCondition> Conditions { get; }

    public SearchAction()
    {
        Conditions = new()
        {
            new MinApCondition(ApCost),
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
            return new ActionResult(false, "Cannot search");

        context.Actor.ConsumeAp(ApCost);
        return new ActionResult(true, $"Search indexed around {context.TargetNode?.Name}");
    }
}
