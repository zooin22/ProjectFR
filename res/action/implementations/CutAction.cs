using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class CutAction : IAction
{
    public string ActionId => "cut";
    public string DisplayName => "Cut (Ctrl+X)";
    public int ApCost => ActionConstants.CutActionApCost;
    public TargetType Scope => TargetType.Single;
    public List<IActionCondition> Conditions { get; }

    public CutAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.CutActionApCost),
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
            return new ActionResult(false, "Cannot cut");

        if (context.TargetNode == null || context.Target == null)
            return new ActionResult(false, "No valid target");

        context.Actor.ConsumeAp(ApCost);
        int damage = ActionConstants.CutDamage;

        context.Target.TakeDamage(damage);

        if (context.Clipboard != null)
        {
            context.Clipboard.Cut(context.TargetNode);
        }

        return new ActionResult(true, $"Cut {context.TargetNode.Name} dealing {damage} damage", damage);
    }
}
