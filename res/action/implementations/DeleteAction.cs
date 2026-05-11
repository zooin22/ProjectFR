using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class DeleteAction : IAction
{
    public string ActionId => "delete";
    public string DisplayName => "Delete";
    public int ApCost => 2;
    public TargetType Scope => TargetType.Single;
    public List<IActionCondition> Conditions { get; }

    public DeleteAction()
    {
        Conditions = new()
        {
            new MinApCondition(2),
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
            return new ActionResult(false, "Cannot execute Delete action");

        context.Actor.ConsumeAp(ApCost);
        int damage = 8;

        if (context.Target != null)
        {
            context.Target.TakeDamage(damage);
            return new ActionResult(true, $"Deleted {context.TargetNode?.Name} dealing {damage} damage", damage);
        }

        return new ActionResult(false, "No valid target");
    }
}
