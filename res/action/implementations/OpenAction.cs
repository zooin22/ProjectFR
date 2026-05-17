using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;

namespace ProjectFR.Action.Implementations;

public class OpenAction : IAction
{
    public string ActionId => "open";
    public string DisplayName => "Open (Enter)";
    public int ApCost => ActionConstants.OpenActionApCost;
    public TargetType Scope => TargetType.Single;
    public List<IActionCondition> Conditions { get; }

    public OpenAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.OpenActionApCost),
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
            return new ActionResult(false, "Cannot execute Open action");

        context.Actor.ConsumeAp(ApCost);

        int damage = context.TargetNode is FolderNode
            ? ActionConstants.OpenFolderDamage
            : ActionConstants.OpenFileDamage;

        if (context.Target != null)
        {
            context.Target.TakeDamage(damage);
            return new ActionResult(true, $"Opened {context.TargetNode?.Name} dealing {damage} damage", damage);
        }

        return new ActionResult(false, "No valid target");
    }
}
