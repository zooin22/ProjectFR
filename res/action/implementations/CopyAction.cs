using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class CopyAction : ActionBase
{
    public override string ActionId => ActionIds.Copy;
    public override string DisplayName => "Copy (Ctrl+C)";
    public override int ApCost => ActionConstants.CopyActionApCost;
    public override TargetType Scope => TargetType.Single;

    public CopyAction()
    {
        Conditions = new()
        {
            new MinApCondition(ApCost)
        };
    }

    public override ActionResult Execute(ActionContext context)
    {
        if (context.TargetNode == null)
            return new ActionResult(false, "No target to copy");

        if (context.Clipboard == null)
            return new ActionResult(false, "Clipboard unavailable");

        context.Actor.ConsumeAp(ApCost);
        context.Clipboard.Copy(context.TargetNode);
        return new ActionResult(true, $"Copied {context.TargetNode.Name} to clipboard");
    }
}
