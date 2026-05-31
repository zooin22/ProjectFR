using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;

namespace ProjectFR.Action.Implementations;

public class OpenAction : ActionBase
{
    public override string ActionId => ActionIds.Open;
    public override string DisplayName => "Open (Enter)";
    public override int ApCost => ActionConstants.OpenActionApCost;
    public override TargetType Scope => TargetType.Single;

    public OpenAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.OpenActionApCost),
            new TargetAliveCondition()
        };
    }

    public override ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot execute Open action");

        context.Actor.ConsumeAp(ApCost);

        if (context.TargetNode is ContainerNode)
        {
            return new ActionResult(true, $"Opened {context.TargetNode.Name}");
        }

        int damage = ActionConstants.OpenFileDamage;

        if (context.Target != null)
        {
            context.Target.TakeDamage(damage);
            return new ActionResult(true, $"Opened {context.TargetNode?.Name} dealing {damage} damage", damage);
        }

        return new ActionResult(false, "No valid target");
    }
}
