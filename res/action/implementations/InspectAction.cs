using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class InspectAction : ActionBase
{
    public override string ActionId => ActionIds.Inspect;
    public override string DisplayName => "Inspect";
    public override int ApCost => ActionConstants.InspectActionApCost;
    public override TargetType Scope => TargetType.Single;

    public override bool CanExecute(ActionContext context)
    {
        return base.CanExecute(context)
            && (context.TargetNode is not null || context.Target is not null);
    }

    public override ActionResult Execute(ActionContext context)
    {
        if (context.TargetNode is null && context.Target is null)
            return new ActionResult(false, "No target to inspect");

        var nodeName = context.TargetNode?.Name ?? context.Target?.DisplayName ?? string.Empty;
        var hasName = !string.IsNullOrEmpty(nodeName);
        var result = new ActionResult(true, $"Inspected {(hasName ? nodeName : "target")}");
        if (hasName)
            result.Data["target_name"] = nodeName;
        if (context.Target is not null)
        {
            result.Data["target_hp"] = context.Target.CurrentHp;
            result.Data["target_max_hp"] = context.Target.MaxHp;
        }

        return result;
    }
}
