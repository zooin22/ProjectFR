using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class SearchAction : ActionBase
{
    public override string ActionId => ActionIds.Search;
    public override string DisplayName => "Search";
    public override int ApCost => ActionConstants.SearchActionApCost;
    public override TargetType Scope => TargetType.Single;

    public SearchAction()
    {
        Conditions = new()
        {
            new MinApCondition(ApCost),
            new TargetAliveCondition()
        };
    }

    public override ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot search");

        context.ConsumeAp(ApCost);
        return new ActionResult(true, $"Search indexed around {context.TargetNode?.Name}");
    }
}
