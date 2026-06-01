using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class PermissionOverrideAction : ActionBase
{
    public override string ActionId => ActionIds.PermissionOverride;
    public override string DisplayName => "Permission Override";
    public override int ApCost => ActionConstants.PermissionOverrideActionApCost;
    public override TargetType Scope => TargetType.Single;

    public PermissionOverrideAction()
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
            return new ActionResult(false, "Cannot override permissions");

        context.ConsumeAp(ApCost);
        return new ActionResult(true, $"Permission override challenged {context.TargetNode?.Name}");
    }
}
