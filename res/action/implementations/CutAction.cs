using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class CutAction : ActionBase
{
    public override string ActionId => ActionIds.Cut;
    public override string DisplayName => "Cut (Ctrl+X)";
    public override int ApCost => ActionConstants.CutActionApCost;
    public override TargetType Scope => TargetType.Single;

    public CutAction()
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
            return new ActionResult(false, "Cannot cut");

        if (context.TargetNode == null)
            return new ActionResult(false, "No target node");
        if (context.Clipboard == null)
            return new ActionResult(false, "No clipboard available");

        context.Actor.ConsumeAp(ApCost);
        int damage = ActionConstants.CutDamage;
        var target = context.Target;
        if (target == null)
            return new ActionResult(false, "No target available");

        target.TakeDamage(damage);
        context.Clipboard.Cut(context.TargetNode);

        return new ActionResult(true, $"Cut {context.TargetNode.Name} dealing {damage} damage", damage);
    }
}
