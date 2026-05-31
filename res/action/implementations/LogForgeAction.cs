using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class LogForgeAction : ActionBase
{
    public override string ActionId => ActionIds.LogForge;
    public override string DisplayName => "Rewrite Log";
    public override int ApCost => ActionConstants.LogForgeActionApCost;
    public override TargetType Scope => TargetType.Single;

    public LogForgeAction()
    {
        Conditions = new()
        {
            new MinApCondition(ApCost)
        };
    }

    public override ActionResult Execute(ActionContext context)
    {
        if (context.Actor == null)
            return new ActionResult(false, "No actor in context");

        if (context.TargetNode == null)
            return new ActionResult(false, "No target selected");

        context.Actor.ConsumeAp(ApCost);
        return new ActionResult(true, $"Rewrote traces around {context.TargetNode.Name}");
    }
}
