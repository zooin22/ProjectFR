using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class ShowHiddenAction : ActionBase
{
    public override string ActionId => ActionIds.ShowHidden;
    public override string DisplayName => "Show Hidden";
    public override int ApCost => ActionConstants.ShowHiddenActionApCost;
    public override TargetType Scope => TargetType.Single;

    public ShowHiddenAction()
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
            return new ActionResult(false, "Cannot show hidden");

        context.ConsumeAp(ApCost);
        return new ActionResult(true, $"Show Hidden probed {context.TargetNode?.Name}");
    }
}
