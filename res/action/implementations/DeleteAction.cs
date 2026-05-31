using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class DeleteAction : ActionBase
{
    public override string ActionId => ActionIds.Delete;
    public override string DisplayName => "Delete";
    public override int ApCost => ActionConstants.DeleteActionApCost;
    public override TargetType Scope => TargetType.Single;

    public DeleteAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.DeleteActionApCost),
            new TargetAliveCondition()
        };
    }

    public override ActionResult Execute(ActionContext context)
    {
        if (context.Actor == null)
            return new ActionResult(false, "No valid actor");

        if (context.Target == null)
            return new ActionResult(false, "No valid target");

        if (!CanExecute(context))
            return new ActionResult(false, "Delete prerequisites not met");

        int damage = ActionConstants.DeleteDamage;
        context.Actor.ConsumeAp(ApCost);
        context.Target.TakeDamage(damage);
        return new ActionResult(true, $"Deleted {context.TargetNode?.Name ?? "unknown"} dealing {damage} damage", damage);
    }
}
